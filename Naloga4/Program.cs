//#define BESEDE

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Naloga4 {

    internal class Program {
        private static void Main(string[] args) {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

#if BESEDE
            IzpisiVseBesede();
#else 
            ParsajStavke(args);
#endif
        }

        private static void ParsajStavke(string[] args) {
            if (args.Length < 4 || (args.Length == 1 && args[0].ToUpperInvariant() == "-HELP")) {
                Help();
                return;
            }

            IEnumerable<string> separatorji = GetList(args, "-SEPARATORS");
            IEnumerable<string> izjeme = GetList(args, "-EXCEPTIONS", true);

            if (!Console.IsInputRedirected) {
                Console.Error.WriteLine("Standardni vhod mora biti preusmerjen.");
                return;
            }

            string besedilo;
            using (TextReader tr = new StreamReader(Console.OpenStandardInput(), Encoding.UTF8)) {
                besedilo = tr.ReadToEnd();
            }

            if (string.IsNullOrEmpty(besedilo)) {
                Console.Error.WriteLine("Prazno besedilo");
                return;
            }

            string[] stavki;
            try {
                Parser p = new Parser(new Lexer(besedilo, Encoding.UTF8), separatorji, izjeme);
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

        private static IEnumerable<string> GetList(string[] args, string param, bool escape = false) {
            string fileName = null;
            if (args.Length >= 2 && args[0].ToUpperInvariant() == param) {
                fileName = args[1];
            }

            if (args.Length >= 4 && args[2].ToUpperInvariant() == param) {
                fileName = args[3];
            }

            if (string.IsNullOrEmpty(fileName)) {
                return null;
            }

            string[] list = File.ReadAllLines(fileName);

            if (escape) {
                for (int i = 0; i < list.Length; i++) {
                    list[i] = list[i].Replace(".", "\\.");
                }
            }
            return list;
        }

        private static void Help() {
            Console.WriteLine(
@"
program [options]

Options:
-separators fileName ... file that contains the defined separators
-exceptions fileName ... file that contains the defined exceptions
-help ... display this help and exit.");

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