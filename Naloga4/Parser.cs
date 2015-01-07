using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Naloga4 {

    internal class Parser {
        private readonly Lexer _lexer;
        private readonly IEnumerable<string> _separatorji;
        private readonly IEnumerable<string> _izjeme;
        private readonly LinkedList<string> _stavki;
        private StringBuilder _trenStavek;
        private bool _checkNext; //če moremo naslednjo besedo preveri za veliko začetnico.

        public Parser(Lexer lexer, IEnumerable<string> separatorji, IEnumerable<string> izjeme) {
            _lexer = lexer;
            _separatorji = separatorji;
            _izjeme = izjeme;
            _trenStavek = new StringBuilder();
            _stavki = new LinkedList<string>();
        }

        public string[] Parse() {
            _stavki.Clear();

            Token t;
            do {
                t = _lexer.NextToken();
                string lexem = t.Lexem;

                if (_checkNext) {
                    if (string.IsNullOrEmpty(lexem)) {
                        continue;
                    }

                    //Preverimo če ima naslednja beseda za separatorjem Veliko začetnico ali številom
                    if (Char.IsUpper(lexem, 0) || Char.IsDigit(lexem, 0)) {
                        _stavki.AddLast(_trenStavek.ToString());
                        _trenStavek = _trenStavek.Clear();
                        _checkNext = false;
                    }
                }

                //pogldeamo če se beseda konča z veljavnim separatorjem
                //TODO: Izjeme
                foreach (string separator in _separatorji) {
                    if (t.Lexem.EndsWith(separator)) {
                        _checkNext = true;
                        break;
                    }
                }

                _trenStavek.Append(lexem + " ");
            }
            while (!t.EOF);

            _stavki.AddLast(_trenStavek.ToString());

            return _stavki.ToArray();
        }
    }

}