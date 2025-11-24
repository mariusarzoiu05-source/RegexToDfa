using LFC_TEMA1.Core;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;  // dacă nu exista deja


class Program
{
    static string? _regex;                 // regex curent (din fișier)
    static DeterministicFiniteAutomaton _dfa = BuildSampleDfa(); // DFA curent (de test)
    static string? _postfix;              // postfix-ul expresiei regulate
    static SyntaxNode? _syntaxTree;       // arborele sintactic curent


    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        while (true)
        {
            Console.WriteLine("\n=== MENIU LFC_TEMA1 ===");
            Console.WriteLine("1) Citește expresia regulată din input.txt");
            Console.WriteLine("2) Afișează expresia regulată cu concatenare");
            Console.WriteLine("3) Afișează postfix");
            Console.WriteLine("4) Afișează DFA curent");
            Console.WriteLine("5) Verifică un cuvânt în DFA curent");
            Console.WriteLine("6) Afișează arborele sintactic");
            Console.WriteLine("7) Construiește DFA din arbore");
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
                    TokenizeRegex();
                    break; 

                case "3":
                    ShowPostfixPlaceholder();
                    break;

                case "4":
                    ShowDfa();
                    break;

                case "5":
                    CheckWordInDfa();
                    break;

                case "6":
                    ShowSyntaxTree();
                    break;

                case "7":
                    BuildDfaFromTree();
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
        var path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, "input.txt");
        if (!File.Exists(path))
        {
            Console.WriteLine($"❌ Nu am găsit {path}. Creează un fișier input.txt lângă .csproj.");
            return;
        }

        _regex = File.ReadAllText(path).Trim();
        Console.WriteLine($"✅ Regex încărcat: {_regex}");
    }

    static string InsertConcatenationOperators(List<string> tokens)
    {
        var result = new List<string>();

        for (int i = 0; i < tokens.Count; i++)
        {
            string current = tokens[i];
            result.Add(current);

            if (i == tokens.Count - 1)
                break;

            string next = tokens[i + 1];

            bool currentIsOperator = "*|+?()".Contains(current[0]);
            bool nextIsOperator = "*|+?()".Contains(next[0]);

            bool currentIsSymbol = !currentIsOperator || current == ")";
            bool nextIsSymbol = !nextIsOperator || next == "(";

            bool needsConcat = (currentIsSymbol && nextIsSymbol) || (currentIsSymbol && next == "(") ||
                               (current == ")" && nextIsSymbol) || (current == ")" && next == "(") ||
                               ((current == "*" || current == "+" || current == "?") && (nextIsSymbol || next == "(")); 

            if (needsConcat)
                result.Add(".");
        }

        return string.Join("", result);
    }
    static void TokenizeRegex()
    {
        var tokens = new List<string>();

        foreach (char c in _regex)
            tokens.Add(c.ToString()); 

        _regex = InsertConcatenationOperators(tokens);
        _regex = $"({_regex}).#";

        Console.WriteLine($"✅ Regex concatenat: {_regex}");
    }
    static string RegexToPostfix()
    {
        var output = new List<char>();
        var stack = new Stack<char>();
        var prec = new Dictionary<char, int> { { '*', 3 }, { '+', 3 }, { '.', 2 }, { '|', 1 } };

        foreach (var token in _regex)
        {
            if (!"*|+.()".Contains(token)) 
            {
                output.Add(token);
            }
            else if (token == '(')
            {
                stack.Push(token);
            }
            else if (token == ')')
            {
                while (stack.Count > 0 && stack.Peek() != '(')
                    output.Add(stack.Pop());

                if (stack.Count == 0)
                    throw new Exception("Mismatched parentheses");

                stack.Pop(); // stergem '('
            }
            else 
            {
                while (stack.Count > 0 && stack.Peek() != '(' && prec[stack.Peek()] >= prec[token])
                    output.Add(stack.Pop());

                stack.Push(token);
            }
        }

        while (stack.Count > 0)
        {
            if (stack.Peek() == '(' || stack.Peek() == ')')
                throw new Exception("Mismatched parentheses");

            output.Add(stack.Pop());
        }

        return new string(output.ToArray());
    }

    static void ShowPostfixPlaceholder()
    {
        if (string.IsNullOrWhiteSpace(_regex))
        {
            Console.WriteLine("ℹ️ Întâi alege opțiunea 1 (încarcă regex) și 2 (concatenare).");
            return;
        }

        _postfix = RegexToPostfix();
        Console.WriteLine($"📌 Postfix: {_postfix}");
    }
    static void ShowSyntaxTree()
    {
        if (string.IsNullOrWhiteSpace(_postfix))
        {
            Console.WriteLine("ℹ️ Întâi folosește opțiunea 3 (afișează postfix).");
            return;
        }

        // Construim arborele din postfix și îl memorăm
        _syntaxTree = SyntaxTreeBuilder.BuildFromPostfix(_postfix);

        Console.WriteLine("📌 Arbore sintactic (structură):");
        PrintSyntaxTree(_syntaxTree, "", true);
    }

    // Funcție recursivă pentru afișarea arborelui
    static void PrintSyntaxTree(SyntaxNode node, string indent, bool last)
    {
        Console.Write(indent);
        Console.Write(last ? "└─" : "├─");

        switch (node)
        {
            case SymbolNode s:
                Console.WriteLine($"{s.Symbol} (pos={s.Position})");
                break;

            case UnaryNode u:
                Console.WriteLine(u.Op);
                PrintSyntaxTree(u.Child, indent + (last ? "  " : "│ "), true);
                break;

            case BinaryNode b:
                Console.WriteLine(b.Op);
                PrintSyntaxTree(b.Left, indent + (last ? "  " : "│ "), false);
                PrintSyntaxTree(b.Right, indent + (last ? "  " : "│ "), true);
                break;
        }
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

    static void BuildDfaFromTree()
    {
        if (_syntaxTree == null)
        {
            Console.WriteLine("Mai întâi construiește arborele (opțiunea 6).");
            return;
        }

        _dfa = RegexToDfaBuilder.BuildDfa(_syntaxTree);
        Console.WriteLine("DFA construit din expresia regulată!");
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
