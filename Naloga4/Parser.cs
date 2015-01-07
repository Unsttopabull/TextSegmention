using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Naloga4 {

    internal class Parser {
        private readonly Lexer _lexer;
        private readonly IEnumerable<string> _separatorji;
        private readonly LinkedList<Regex> _izjeme;
        private readonly LinkedList<string> _stavki;
        private StringBuilder _trenStavek;
        private bool _checkNext; //če moremo naslednjo besedo preveri za veliko začetnico.

        public Parser(Lexer lexer, IEnumerable<string> separatorji, IEnumerable<string> izjeme) {
            _lexer = lexer;
            _separatorji = separatorji ?? new LinkedList<string>(new[] { ".", "!", "?" });
            _trenStavek = new StringBuilder();
            _stavki = new LinkedList<string>();

            _izjeme = new LinkedList<Regex>();

            if (izjeme != null) {
                foreach (string izjema in izjeme) {
                    _izjeme.AddLast(new Regex(izjema, RegexOptions.Compiled));
                }
            }
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

                    //Preverimo če ima naslednja beseda za separatorjem Veliko začetnico ali pa je število
                    if (Char.IsUpper(lexem, 0) || Char.IsDigit(lexem, 0)) {
                        _trenStavek.Length = _trenStavek.Length - 1;
                        _stavki.AddLast(_trenStavek.ToString());
                        _trenStavek = _trenStavek.Clear();
                        _checkNext = false;
                    }
                }

                foreach (Regex izjema in _izjeme) {
                    Match match = izjema.Match(lexem);
                    if (match.Success && match.Length == lexem.Length) {
                        lexem = null;
                        break;
                    }
                    if (match.Success) {
                        lexem = lexem.Replace(match.Value, "");
                    }                    
                }

                if (!string.IsNullOrEmpty(lexem)) {
                    //pogldeamo če se beseda konča z veljavnim separatorjem
                    foreach (string separator in _separatorji) {
                        if (lexem.EndsWith(separator)) {
                            _checkNext = true;
                            break;
                        }
                    }
                }


                _trenStavek.Append(t.Lexem + " ");
            }
            while (!t.EOF);

            _stavki.AddLast(_trenStavek.ToString());

            return _stavki.ToArray();
        }
    }

}