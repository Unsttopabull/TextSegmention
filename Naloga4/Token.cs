using System;
using System.Collections.Generic;

namespace Naloga4 {

    public enum TokenType {
        Error,
        Beseda,
        Prezri
    }

    public class Token {
        private readonly List<string> _tabela = new List<string>();

        public Token() {
            NapolniTabelo();
        }

        public Token(string lexem, int column, int row, TokenType tokenType, bool eof) {
            NapolniTabelo();

            Lexem = lexem;
            Column = column;
            Row = row;
            TokenType = tokenType;
            EOF = eof;
        }

        private void NapolniTabelo() {
            _tabela.Add("error");
            _tabela.Add("beseda");
            _tabela.Add("prezri");
        }

        public string Lexem { get; set; } // "20,2" | "*"
        public int Column { get; set; } // 1
        public int Row { get; set; } // 1
        public TokenType TokenType { get; set; } // vrednost, ki predstavlja (float, separator, operator, ...)
        public bool EOF { get; set; } // konec datoteke (true/false)

        public override String ToString() {
            return string.Format("\"{0}\" {1} ({2}, {3}) {4}", Lexem, TokenType, Row, Column, (EOF ? "true" : "false"));
        }
    }

}