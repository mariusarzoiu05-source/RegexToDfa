using LFC_TEMA1.Core;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;  


class Program
{
    static string? _regex;                 // regex curent 
    static DeterministicFiniteAutomaton? _dfa; // DFA curent 
    static string? _postfix;              // postfix-ul expresiei regulate
    static SyntaxNode? _syntaxTree;       // arborele sintactic 


    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        LoadRegexFromFile();
        TokenizeRegex();

        while (true)
        {
            Console.WriteLine("\n=== MENIU TEMA1 ===");
            Console.WriteLine("1) Afișează forma poloneză postfixată");
            Console.WriteLine("2) Afișează arborele sintactic");
            Console.WriteLine("3) Afișează automat curent");
            Console.WriteLine("4) Verifică un cuvânt");
            Console.WriteLine("0) Ieșire");
            Console.Write("Alege: ");

            var opt = Console.ReadLine();
            Console.WriteLine();

            switch (opt)
            {
                case "1":
                    ShowPostfixPlaceholder();
                    break;

                case "2":
                    ShowSyntaxTree();
                    break;

                case "3":
                    BuildDfaFromTree();
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
        var path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, "input.txt");
        if (!File.Exists(path))
        {
            Console.WriteLine($"Nu am găsit {path}. Creează un fișier input.txt lângă .csproj.");
            return;
        }

        _regex = File.ReadAllText(path).Trim();
        Console.WriteLine($"Regex încărcat: {_regex}");
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
        if (string.IsNullOrWhiteSpace(_regex))
        {
            Console.WriteLine("Întâi încarcă expresia regulată.");
            return;
        }

        var tokens = new List<string>();
        foreach (char c in _regex)
            tokens.Add(c.ToString());

        _regex = InsertConcatenationOperators(tokens);

        Console.WriteLine($"Regex concatenat: {_regex}");
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

                stack.Pop(); // ștergem '('
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
            Console.WriteLine("Întâi alege încarcă regex și concatenează.");
            return;
        }

        _postfix = RegexToPostfix();
        Console.WriteLine($"Postfix: {_postfix}");
    }
    static void ShowSyntaxTree()
    {
        if (string.IsNullOrWhiteSpace(_postfix))
        {
            Console.WriteLine("Întâi folosește opțiunea 1 (afișează postfix).");
            return;
        }

        _syntaxTree = SyntaxTreeBuilder.BuildFromPostfix(_postfix);

        Console.WriteLine("Arbore sintactic (structură):");
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
        if (_dfa == null)
        {
            Console.WriteLine("Nu există încă un DFA.");
            return;
        }

        if (!_dfa.VerifyAutomaton())
        {
            Console.WriteLine("Automat invalid.");
            return;
        }

        // Construim textul DFA-ului
        using var sw = new StringWriter();
        _dfa.PrintAutomaton(sw);
        string dfaText = sw.ToString();

        // Salvăm în fișier lângă input.txt
        var projectDir = Directory.GetParent(Directory.GetCurrentDirectory())!
                                  .Parent!
                                  .Parent!
                                  .FullName;

        var path = Path.Combine(projectDir, "dfa_out.txt");
        File.WriteAllText(path, dfaText);
        Console.WriteLine("DFA generat și salvat în dfa_out.txt.");
    }



    static void CheckWordInDfa()
    {
        Console.Write("Introdu cuvântul: ");
        var w = Console.ReadLine() ?? string.Empty;
        var ok = _dfa.CheckWord(w);
        Console.WriteLine(ok ? "Acceptat" : "Respins");
    }

    static void BuildDfaFromTree()
    {
        if (string.IsNullOrWhiteSpace(_postfix))
        {
            Console.WriteLine("Mai întâi generează postfixul.");
            return;
        }

        // PAS I: Regex -> AFN cu λ 
        var nfa = RegexToNfaBuilder.BuildFromPostfix(_postfix);

        // PAS II: AFN-λ -> AFD
        _dfa = NfaToDfaConverter.Convert(nfa);

        Console.WriteLine("DFA construit.");
    }


}
