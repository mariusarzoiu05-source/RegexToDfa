using System;
using System.Collections.Generic;

namespace LFC_TEMA1.Core
{
    public static class RegexToNfaBuilder
    {
        public static NfaLambda BuildFromPostfix(string postfix)
        {
            var stack = new Stack<NfaLambda>();
            int nextState = 0;

            foreach (char t in postfix)
            {
                if (t == '.' || t == '|' || t == '*')
                {
                    // operatori
                    if (t == '*')
                    {
                        var a = stack.Pop();
                        stack.Push(Star(a, ref nextState));
                    }
                    else if (t == '.')
                    {
                        var b = stack.Pop(); // dreapta
                        var a = stack.Pop(); // stânga
                        stack.Push(Concat(a, b));
                    }
                    else if (t == '|')
                    {
                        var b = stack.Pop();
                        var a = stack.Pop();
                        stack.Push(Union(a, b, ref nextState));
                    }
                }
                else
                {
                    // simbol din alfabet
                    stack.Push(SymbolNfa(t, ref nextState));
                }
            }

            if (stack.Count != 1)
                throw new Exception("Postfix invalid – NFA incomplet.");

            return stack.Pop();
        }

        private static NfaLambda SymbolNfa(char a, ref int nextState)
        {
            var nfa = new NfaLambda();
            int s = nextState++;
            int f = nextState++;

            nfa.Start = s;
            nfa.AcceptStates.Add(f);
            nfa.AddTransition(s, a, f);

            return nfa;
        }

        private static NfaLambda Concat(NfaLambda a, NfaLambda b)
        {
            var nfa = new NfaLambda();

            // copiem stările, tranzițiile, lambdele
            CopyInto(a, nfa);
            CopyInto(b, nfa);

            // λ de la toate finale lui A către startul lui B
            foreach (var fa in a.AcceptStates)
            {
                nfa.AddLambda(fa, b.Start);
            }

            nfa.Start = a.Start;
            nfa.AcceptStates.Clear();
            foreach (var fb in b.AcceptStates)
                nfa.AcceptStates.Add(fb);

            return nfa;
        }

        private static NfaLambda Union(NfaLambda a, NfaLambda b, ref int nextState)
        {
            var nfa = new NfaLambda();
            CopyInto(a, nfa);
            CopyInto(b, nfa);

            int s = nextState++;
            int f = nextState++;

            nfa.Start = s;
            nfa.States.Add(s);
            nfa.States.Add(f);

            // λ la starturile lui A și B
            nfa.AddLambda(s, a.Start);
            nfa.AddLambda(s, b.Start);

            // λ de la finale lui A și B către noul final
            foreach (var fa in a.AcceptStates)
                nfa.AddLambda(fa, f);
            foreach (var fb in b.AcceptStates)
                nfa.AddLambda(fb, f);

            nfa.AcceptStates.Clear();
            nfa.AcceptStates.Add(f);

            return nfa;
        }

        private static NfaLambda Star(NfaLambda a, ref int nextState)
        {
            var nfa = new NfaLambda();
            CopyInto(a, nfa);

            int s = nextState++;
            int f = nextState++;

            nfa.Start = s;
            nfa.States.Add(s);
            nfa.States.Add(f);

            // λ(s, f) și λ(s, start(A))
            nfa.AddLambda(s, f);
            nfa.AddLambda(s, a.Start);

            // λ(final(A), start(A)) și λ(final(A), f)
            foreach (var fa in a.AcceptStates)
            {
                nfa.AddLambda(fa, a.Start);
                nfa.AddLambda(fa, f);
            }

            nfa.AcceptStates.Clear();
            nfa.AcceptStates.Add(f);

            return nfa;
        }

        private static void CopyInto(NfaLambda src, NfaLambda dst)
        {
            dst.States.UnionWith(src.States);
            dst.Sigma.UnionWith(src.Sigma);

            foreach (var kv in src.Delta)
            {
                if (!dst.Delta.ContainsKey(kv.Key))
                    dst.Delta[kv.Key] = new HashSet<int>();
                dst.Delta[kv.Key].UnionWith(kv.Value);
            }

            foreach (var kv in src.Lambda)
            {
                if (!dst.Lambda.ContainsKey(kv.Key))
                    dst.Lambda[kv.Key] = new HashSet<int>();
                dst.Lambda[kv.Key].UnionWith(kv.Value);
            }

        }
    }
}
