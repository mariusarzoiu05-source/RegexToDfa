using System;
using System.Collections.Generic;

namespace LFC_TEMA1.Core
{
    public class NfaLambda
    {
        public HashSet<int> States { get; } = new();
        public HashSet<char> Sigma { get; } = new();
        public Dictionary<(int state, char symbol), HashSet<int>> Delta { get; } = new();
        public Dictionary<int, HashSet<int>> Lambda { get; } = new();
        public int Start { get; set; }
        public HashSet<int> AcceptStates { get; } = new();

        public void AddTransition(int from, char symbol, int to)
        {
            Sigma.Add(symbol);
            var key = (from, symbol);
            if (!Delta.ContainsKey(key))
                Delta[key] = new HashSet<int>();
            Delta[key].Add(to);

            States.Add(from);
            States.Add(to);
        }

        public void AddLambda(int from, int to)
        {
            if (!Lambda.ContainsKey(from))
                Lambda[from] = new HashSet<int>();
            Lambda[from].Add(to);

            States.Add(from);
            States.Add(to);
        }

        // λ-închiderea unei singure stări
        public HashSet<int> LambdaClosure(int state)
        {
            var closure = new HashSet<int> { state };
            var stack = new Stack<int>();
            stack.Push(state);

            while (stack.Count > 0)
            {
                var q = stack.Pop();
                if (Lambda.TryGetValue(q, out var nexts))
                {
                    foreach (var t in nexts)
                    {
                        if (closure.Add(t))
                            stack.Push(t);
                    }
                }
            }

            return closure;
        }

        // λ-închiderea unei mulțimi de stări
        public HashSet<int> LambdaClosure(IEnumerable<int> states)
        {
            var closure = new HashSet<int>();
            foreach (var s in states)
            {
                closure.UnionWith(LambdaClosure(s));
            }
            return closure;
        }
    }
}
