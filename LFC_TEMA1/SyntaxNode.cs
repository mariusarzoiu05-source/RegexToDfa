using System.Collections.Generic;

namespace LFC_TEMA1.Core
{
    // Nod abstract de bază
    public abstract class SyntaxNode
    {
        public static Dictionary<int, SyntaxNode> PositionToNode { get; } = new();
        public bool Nullable { get; set; }
        public HashSet<int> FirstPos { get; } = new();
        public HashSet<int> LastPos { get; } = new();
    }

    // Nod pentru un simbol din alfabet (a, b, c, ...)
    public class SymbolNode : SyntaxNode
    {
        public char Symbol { get; }
        public int Position { get; }

        public SymbolNode(char symbol, int position)
        {
            Symbol = symbol;
            Position = position;
            Nullable = false;
            FirstPos.Add(position);
            LastPos.Add(position);

            SyntaxNode.PositionToNode[position] = this;
        }
    }

    // Nod unar: *, +, ?
    public class UnaryNode : SyntaxNode
    {
        public char Op { get; }
        public SyntaxNode Child { get; }

        public UnaryNode(char op, SyntaxNode child)
        {
            Op = op;
            Child = child;
        }
    }

    // Nod binar: . (concatenare), | (sau)
    public class BinaryNode : SyntaxNode
    {
        public char Op { get; }
        public SyntaxNode Left { get; }
        public SyntaxNode Right { get; }

        public BinaryNode(char op, SyntaxNode left, SyntaxNode right)
        {
            Op = op;
            Left = left;
            Right = right;
        }
    }
}
