using System;
using System.Collections.Generic;
using LFC_TEMA1.Core;

namespace LFC_TEMA1.Core
{
    public static class RegexToDfaBuilder
    {
        public static DeterministicFiniteAutomaton BuildDfa(SyntaxNode root)
        {
            // Construim followpos
            var followpos = FollowPosBuilder.Build(root);

            // Mapăm pozițiile simbolurilor
            var symbols = new Dictionary<int, char>();
            CollectSymbols(root, symbols);  // include și #

            // Inițializăm DFA
            var dfa = new DeterministicFiniteAutomaton();
            var start = new HashSet<int>(root.FirstPos);
            var stateIdMap = new Dictionary<string, int>(); // key = set de poziții
            var queue = new Queue<HashSet<int>>();
            int nextId = 0;

            stateIdMap[SetKey(start)] = nextId;
            dfa.States.Add(nextId);
            dfa.Q0 = nextId;
            queue.Enqueue(start);

            // Construim stările DFA
            while (queue.Count > 0)
            {
                var S = queue.Dequeue();
                int currentState = stateIdMap[SetKey(S)];

                // mapare simbol → următoarele poziții
                var transitions = new Dictionary<char, HashSet<int>>();

                foreach (var pos in S)
                {
                    char a = symbols[pos];
                    if (a == '#') continue; // nu tranzitionăm pe simbolul final

                    if (!transitions.ContainsKey(a))
                        transitions[a] = new HashSet<int>();

                    if (followpos.ContainsKey(pos))
                        transitions[a].UnionWith(followpos[pos]);
                }

                foreach (var kv in transitions)
                {
                    char a = kv.Key;
                    var T = kv.Value;
                    string key = SetKey(T);

                    if (!stateIdMap.ContainsKey(key))
                    {
                        nextId++;
                        stateIdMap[key] = nextId;
                        dfa.States.Add(nextId);
                        queue.Enqueue(T);
                    }

                    dfa.Sigma.Add(a);
                    dfa.Delta[(currentState, a)] = stateIdMap[key];
                }
            }

            // Determinăm stările finale (care conțin poziția lui #)
            int endPos = -1;
            foreach (var kv in symbols)
                if (kv.Value == '#')
                    endPos = kv.Key;

            foreach (var kv in stateIdMap)
            {
                string[] parts = kv.Key.Split(',');
                int state = kv.Value;
                foreach (var pStr in parts)
                {
                    if (int.TryParse(pStr, out int pos) && pos == endPos)
                    {
                        dfa.F.Add(state);
                        break;
                    }
                }
            }

            return dfa;
        }

        private static void CollectSymbols(SyntaxNode node, Dictionary<int, char> map)
        {
            switch (node)
            {
                case SymbolNode s:
                    map[s.Position] = s.Symbol;
                    break;

                case UnaryNode u:
                    CollectSymbols(u.Child, map);
                    break;

                case BinaryNode b:
                    CollectSymbols(b.Left, map);
                    CollectSymbols(b.Right, map);
                    break;
            }
        }

        private static string SetKey(HashSet<int> s)
        {
            var arr = new List<int>(s);
            arr.Sort();
            return string.Join(",", arr);
        }
    }
}
