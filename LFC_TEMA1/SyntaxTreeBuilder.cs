using System.Collections.Generic;

namespace LFC_TEMA1.Core
{
    public static class SyntaxTreeBuilder
    {
        // Construim arborele din expresia postfix
        public static SyntaxNode BuildFromPostfix(string postfix)
        {
            var stack = new Stack<SyntaxNode>();
            int pos = 1; // numerotăm simbolurile

            foreach (char t in postfix)
            {
                if ("*+?|.".Contains(t))
                {
                    // operator unar: *, +, ?
                    if (t == '*' || t == '+' || t == '?')
                    {
                        var child = stack.Pop();
                        stack.Push(new UnaryNode(t, child));
                    }
                    else // operator binar: . sau |
                    {
                        var right = stack.Pop();
                        var left = stack.Pop();
                        stack.Push(new BinaryNode(t, left, right));
                    }
                }
                else
                {
                    // simbol normal din alfabet
                    stack.Push(new SymbolNode(t, pos++));
                }
            }

            if (stack.Count != 1)
                throw new System.Exception("Postfix invalid – nu s-a obținut un singur arbore.");

            return stack.Pop();
        }
    }
}
