using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Naloga4 {

    internal class Lexer {
        private readonly StreamReader _datoteka; // vhodna datoteka
        private Token _lastToken; // trenutni osnovni leksikalni simbol
        private int _row; // vrstica vhodne datoteke
        private int _column; // stolpec vhodne datoteke
        private const int MAX_STATE = 3; //št. stanj avtomata
        private const int START_STATE = 0; //zaèetno stanje avtomata
        private const int NI_PREHODA = -1; //ni prehoda
        private const int MAX_CHAR = char.MaxValue + 1; //vsi UTF-8 znaki
        private const int NAPAKA = 0;
        private const int IGNORE = 2;
        private readonly int[][] _avtomat = new int[MAX_STATE][]; //tabela prehodov
        private readonly int[] _koncnaStanja = new int[MAX_STATE]; // tabela konènih stanj

        //konstruktor - odpre datoteko, preveri ce obstaja, ....
        public Lexer(string imeDatoteke) {
            FileStream sw = File.Open("log.txt", FileMode.Create);
            Debug.Listeners.Add(new TextWriterTraceListener(sw));
            Debug.AutoFlush = true;

            _row = 1;
            _column = 1;
            for (int i = 0; i < MAX_STATE; i++) {
                _avtomat[i] = new int[MAX_CHAR];
            }

            InitAvtomat();
            IzpisiTabelo();
            _datoteka = new StreamReader(imeDatoteke);
        }

        private void IzpisiTabelo() {
            StringBuilder sb = new StringBuilder();
            sb.Append(";");

            //Izpis zgornje vrstice (samo ASCII)
            for (int i = 0; i < 128; i++) {
                string nextChar;
                if (i == ';') {
                    nextChar = "SEMI";
                }
                else {
                    nextChar = Escape(i);
                }

                sb.AppendFormat("{0} \"{1}\";", i, nextChar);
            }
            sb.AppendLine();

            for (int x = 0; x < MAX_STATE; x++) {
                sb.Append(x + ";");
                for (int y = 0; y < 128; y++) {
                    int xy = _avtomat[x][y];

                    if (xy == -1) {
                        sb.Append(";");
                    }
                    else {
                        sb.Append(xy + ";");
                    }
                }
                sb.AppendLine();
            }

            using (StreamWriter sw = new StreamWriter(File.Open("avtomat.csv", FileMode.Create), Encoding.UTF8)) {
                sw.Write(sb.ToString());
            }
        }

        private string Escape(int charVal) {
            if (charVal == '\n') {
                return "\\n";
            }
            if (charVal == '\r') {
                return "\\r";
            }
            if (charVal == 9) {
                return "\\t";
            }
            return "" + ((char) charVal);
        }

        //private void InitAvtomat() {
        //    for (int i = 0; i < MAX_STATE; i++) {
        //        for (int j = 0; j < MAX_CHAR; j++) {
        //            _avtomat[i][j] = NI_PREHODA;
        //        }
        //    }


        //    for (int state = 0; state < MAX_STATE; state++) {
        //        if (state == 3) {
        //            continue;
        //        }

        //        if (state != 3) {
        //            _avtomat[state]['\n'] = 4;
        //            _avtomat[state]['\r'] = 4;
        //        }

        //        for (int j = 0; j < MAX_CHAR; j++) {
        //            if (j >= 32) {
        //                _avtomat[state][j] = 1;
        //            }
        //        }
        //    }

        //    _avtomat[1]['.'] = 2;
        //    _avtomat[2]['.'] = 2;
        //    _avtomat[4]['.'] = 2;

        //    _avtomat[2][' '] = 3;

        //    _koncnaStanja[0] = _koncnaStanja[1] = _koncnaStanja[2] = _koncnaStanja[4] = NAPAKA; //napaka
        //    _koncnaStanja[3] = 1; //beseda
        //    // stanje 4 je konèno, vendar se le to prezre
        //}

        private void InitAvtomat() {
            for (int i = 0; i < MAX_STATE; i++) {
                for (int j = 0; j < MAX_CHAR; j++) {
                    _avtomat[i][j] = NI_PREHODA;
                }
            }

            for (int state = 0; state < MAX_STATE; state++) {
                if (state == 1) {
                    continue;
                }

                if (state != 1) {
                    _avtomat[state]['\n'] = IGNORE;
                    _avtomat[state]['\r'] = IGNORE;
                }

                for (int j = 0; j < MAX_CHAR; j++) {
                    if (j < 32) {
                        continue;
                    }

                    if (state == 2 && j != '-') {
                        _avtomat[state][j] = 1;
                    }
                    else {
                        _avtomat[state][j] = 0;
                    }
                }
            }

            _avtomat[0][' '] = 1;

            _koncnaStanja[0] = _koncnaStanja[2] = NAPAKA; //napaka
            _koncnaStanja[1] = 1; //beseda
        }

        private Token NextTokenImp() {
            int currState = START_STATE;
            StringBuilder lexem = new StringBuilder();
            int startColumn = _column;
            int startRow = _row;

            do {
                int peek = _datoteka.Peek();
                char peekChar = (char) peek;

                int nextState = GetNextState(currState, peek);

                Debug.WriteLine("CurrState: " + currState);
                Debug.WriteLine("Peek: {0} \"{1}\"", peek, Escape(peek));
                Debug.WriteLine("NextState: " + nextState);

                if (nextState != NI_PREHODA) {
                    if (currState == IGNORE && nextState == 1) {
                        //gremo v novo stanje a ostanemo na trenutni poziciji
                    }
                    else if (nextState != IGNORE && peek != ' ') {
                        Debug.WriteLine("APPEND");
                        lexem.Append((char) Read());
                    }
                    else {
                        Debug.WriteLine("READ");
                        Read();
                    }

                    currState = nextState;
                }
                else {
                    if (IsKoncnoStanje(currState)) {
                        Debug.WriteLine("KONÈNO STANJE: " + GetKoncnoStanje(currState));
                        Debug.WriteLine("");

                        Token token = new Token(lexem.ToString(), startColumn, startRow, GetKoncnoStanje(currState), IsEOF);
                        return token;
                    }

                    if (IsEOF) {
                        Debug.WriteLine("EOF");
                        Debug.WriteLine("");

                        return new Token(lexem.ToString(), startColumn, startRow, TokenType.Prezri, IsEOF);
                    }

                    throw new Exception("Ni konèno stanje in ni prehoda");
                }

                Debug.WriteLine("");
            }
            while (true);
        }

        protected int GetNextState(int stanje, int nextChar) {
            return nextChar == -1
                       ? NI_PREHODA
                       : _avtomat[stanje][nextChar]; //_avtomat[stanje, nextChar];
        }

        protected bool IsKoncnoStanje(int stanje) {
            return _koncnaStanja[stanje] != NAPAKA;
        }

        protected TokenType GetKoncnoStanje(int aState) {
            return (TokenType) _koncnaStanja[aState];
        }

        public Token NextToken() {
            return _lastToken = NextTokenImp();
        }

        public Token CurrToken {
            get { return _lastToken; }
        }

        public int Read() {
            int tmp = 0;
            try {
                tmp = _datoteka.Read();
                _column++;
            }
            catch (IOException e) {
                Console.Error.WriteLine(e.StackTrace);
            }

            if (tmp == '\n') {
                _row++;
                _column = 1;
            }
            return tmp;
        }

        private bool IsEOF {
            get { return _datoteka.Peek() == -1; }
        }
    }

}