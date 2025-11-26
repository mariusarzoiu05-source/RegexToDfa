using System;
using System.Collections.Generic;

namespace LFC_TEMA1.Core
{
    public static class NfaToDfaConverter
    {
        public static DeterministicFiniteAutomaton Convert(NfaLambda nfa)
        {
            var dfa = new DeterministicFiniteAutomaton();
            dfa.Sigma.UnionWith(nfa.Sigma);

            // map: mulțime de stări NFA -> stare DFA (int)
            var stateMap = new Dictionary<string, int>();
            var queue = new Queue<HashSet<int>>();
            int nextId = 0;

            // λ-închiderea lui start(NFA)
            var startSet = nfa.LambdaClosure(nfa.Start);
            string startKey = SetKey(startSet);
            stateMap[startKey] = nextId;
            dfa.States.Add(nextId);
            dfa.Q0 = nextId;
            queue.Enqueue(startSet);

            // determinism
            while (queue.Count > 0)
            {
                var S = queue.Dequeue();
                int fromId = stateMap[SetKey(S)];

                // pentru fiecare simbol din alfabet
                foreach (var a in nfa.Sigma)
                {
                    var moveSet = new HashSet<int>();

                    // mergem pe 'a' din fiecare stare din S
                    foreach (var q in S)
                    {
                        var key = (q, a);
                        if (nfa.Delta.TryGetValue(key, out var dests))
                        {
                            moveSet.UnionWith(dests);
                        }
                    }

                    if (moveSet.Count == 0)
                        continue;

                    // λ-închiderea rezultatului
                    var closure = nfa.LambdaClosure(moveSet);
                    string keySet = SetKey(closure);

                    if (!stateMap.ContainsKey(keySet))
                    {
                        int newId = ++nextId;
                        stateMap[keySet] = newId;
                        dfa.States.Add(newId);
                        queue.Enqueue(closure);
                    }

                    int toId = stateMap[keySet];
                    dfa.Delta[(fromId, a)] = toId;
                }
            }

            // stări finale: cele care conțin vreun final din NFA
            foreach (var kv in stateMap)
            {
                string key = kv.Key;
                int dfaState = kv.Value;

                var nfaStates = ParseSetKey(key);
                foreach (var qf in nfa.AcceptStates)
                {
                    if (nfaStates.Contains(qf))
                    {
                        dfa.F.Add(dfaState);
                        break;
                    }
                }
            }

            return dfa;
        }

        private static string SetKey(HashSet<int> s)
        {
            var list = new List<int>(s);
            list.Sort();
            return string.Join(",", list);
        }

        private static HashSet<int> ParseSetKey(string key)
        {
            var result = new HashSet<int>();
            if (string.IsNullOrEmpty(key))
                return result;

            var parts = key.Split(',');
            foreach (var p in parts)
                if (int.TryParse(p, out int v))
                    result.Add(v);

            return result;
        }
    }
}
