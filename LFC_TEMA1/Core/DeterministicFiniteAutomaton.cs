using System;
using System.Collections.Generic;
using System.IO;

namespace LFC_TEMA1.Core
{
    public class DeterministicFiniteAutomaton
    {
        public HashSet<int> States { get; } = new();
        public HashSet<char> Sigma { get; } = new();
        public Dictionary<(int state, char symbol), int> Delta { get; } = new();
        public int Q0 { get; set; }
        public HashSet<int> F { get; } = new();

        public bool VerifyAutomaton()
        {
            if (!States.Contains(Q0)) return false;
            foreach (var f in F)
                if (!States.Contains(f)) return false;

            foreach (var kv in Delta)
            {
                if (!States.Contains(kv.Key.state)) return false;
                if (!Sigma.Contains(kv.Key.symbol)) return false;
                if (!States.Contains(kv.Value)) return false;
            }
            return true;
        }

        public void PrintAutomaton(TextWriter tw)
        {
            tw.WriteLine($"States: {{{string.Join(",", States)}}}");
            tw.WriteLine($"Sigma: {{{string.Join(",", Sigma)}}}");
            tw.WriteLine($"Q0: {Q0}");
            tw.WriteLine($"F: {{{string.Join(",", F)}}}");
            foreach (var ((q, a), to) in Delta)
                tw.WriteLine($"δ({q}, '{a}') = {to}");
        }

        public bool CheckWord(string w)
        {
            var q = Q0;
            foreach (var ch in w)
            {
                if (!Delta.TryGetValue((q, ch), out var next))
                    return false;
                q = next;
            }
            return F.Contains(q);
        }
    }
}
