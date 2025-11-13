using System;
using System.IO;
using LFC_TEMA1.Core;

class Program
{
    static string? _regex;                 // regex curent (din fișier)
    static DeterministicFiniteAutomaton _dfa = BuildSampleDfa(); // DFA curent (de test)

    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        while (true)
        {
            Console.WriteLine("\n=== MENIU LFC_TEMA1 ===");
            Console.WriteLine("1) Citește expresia regulată din input.txt");
            Console.WriteLine("2) Afișează postfix");
            Console.WriteLine("3) Afișează DFA curent");
            Console.WriteLine("4) Verifică un cuvânt în DFA curent");
            Console.WriteLine("0) Ieșire");
            Console.Write("Alege: ");

            var opt = Console.ReadLine();
            Console.WriteLine();

            switch (opt)
            {
                case "1":
                    LoadRegexFromFile();
                    break;

                case "2":
                    ShowPostfixPlaceholder();
                    break;

                case "3":
                    ShowDfa();
                    break;

                case "4":
                    CheckWordInDfa();
                    break;

                case "0":
                    return;

                default:
                    Console.WriteLine("Opțiune invalidă.");
                    break;
            }
        }
    }

    // ---------- Opțiuni meniu ----------
    static void LoadRegexFromFile()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "input.txt");
        if (!File.Exists(path))
        {
            Console.WriteLine($"❌ Nu am găsit {path}. Creează un fișier input.txt lângă .csproj.");
            return;
        }

        _regex = File.ReadAllText(path).Trim();
        Console.WriteLine($"✅ Regex încărcat: {_regex}");
    }

    static void ShowPostfixPlaceholder()
    {
        if (string.IsNullOrWhiteSpace(_regex))
        {
            Console.WriteLine("ℹ️ Întâi alege opțiunea 1 (încarcă regex din input.txt).");
            return;
        }

        // aici veți conecta ulterior: var postfix = RegexToPostfix.Convert(_regex);
        Console.WriteLine("📌 Postfix: (în curând – va fi implementat de colega)");
    }

    static void ShowDfa()
    {
        if (!_dfa.VerifyAutomaton())
        {
            Console.WriteLine("❌ Automat invalid (ar trebui reconstruit).");
            return;
        }
        Console.WriteLine("✅ Automat valid:");
        _dfa.PrintAutomaton(Console.Out);
    }

    static void CheckWordInDfa()
    {
        Console.Write("Introdu cuvântul: ");
        var w = Console.ReadLine() ?? string.Empty;
        var ok = _dfa.CheckWord(w);
        Console.WriteLine(ok ? "✅ Acceptat" : "❌ Respins");
    }

    // ---------- DFA de probă: acceptă DOAR „ab” ----------
    static DeterministicFiniteAutomaton BuildSampleDfa()
    {
        var dfa = new DeterministicFiniteAutomaton();
        dfa.States.UnionWith(new[] { 0, 1, 2 });
        dfa.Sigma.UnionWith(new[] { 'a', 'b' });
        dfa.Q0 = 0;
        dfa.F.Add(2);
        dfa.Delta[(0, 'a')] = 1;
        dfa.Delta[(1, 'b')] = 2;
        return dfa;
    }
}
