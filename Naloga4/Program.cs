//#define BESEDE

using System;
using System.Text;

namespace Naloga4 {

    internal class Program {
        private static void Main() {
            Console.OutputEncoding = Encoding.UTF8;

#if BESEDE
            IzpisiVseBesede();
#else 
            ParsajStavke();
#endif
        }

        private static void ParsajStavke() {
            string[] separatorji = { "." };//{ ".", "?", "!" };
            string[] izjeme = { "npr.", "itd.", "[0-9]+"};

            string[] stavki;
            try {
                Parser p = new Parser(new Lexer("tesniIzraz"), separatorji, izjeme);
                stavki = p.Parse();
            }
            catch (Exception e) {
                Console.WriteLine(e);
                stavki = new string[0];
            }

            foreach (string stavek in stavki) {
                Console.WriteLine(stavek);
            }
        }

        private static void IzpisiVseBesede() {
            Lexer izraz = new Lexer("tesniIzraz");

            Token trenutni;
            do {
                trenutni = izraz.NextToken();
                string lexem = trenutni.Lexem;
                Console.WriteLine(lexem);
                if (trenutni.TokenType == TokenType.Error) {
                    Console.Error.WriteLine("Napaka v razpoznavanju! ({0})", trenutni);
                    Console.ReadLine();
                    return;
                }
            }
            while (!trenutni.EOF);
        }
    }

}