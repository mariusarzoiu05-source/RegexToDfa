using System.Collections.Generic;
using LFC_TEMA1.Core;

namespace LFC_TEMA1.Core
{
    public static class FollowPosBuilder
    {
        public static Dictionary<int, HashSet<int>> Build(SyntaxNode root)
        {
            var followpos = new Dictionary<int, HashSet<int>>();
            ComputeProps(root, followpos);
            return followpos;
        }

        private static void ComputeProps(SyntaxNode node, Dictionary<int, HashSet<int>> follow)
        {
            switch (node)
            {
                case SymbolNode s:
                    // deja setat în constructor
                    break;

                case UnaryNode u:
                    ComputeProps(u.Child, follow);
                    u.Nullable = (u.Op == '*') ? true : false;
                    u.FirstPos.Clear();
                    u.FirstPos.UnionWith(u.Child.FirstPos);
                    u.LastPos.Clear();
                    u.LastPos.UnionWith(u.Child.LastPos);

                    if (u.Op == '*' || u.Op == '+')
                    {
                        foreach (var p in u.LastPos)
                            foreach (var f in u.FirstPos)
                                AddFollow(follow, p, f);
                    }
                    break;

                case BinaryNode b:
                    ComputeProps(b.Left, follow);
                    ComputeProps(b.Right, follow);

                    if (b.Op == '.')
                    {
                        b.Nullable = b.Left.Nullable && b.Right.Nullable;

                        b.FirstPos.Clear();
                        b.FirstPos.UnionWith(b.Left.FirstPos);
                        if (b.Left.Nullable)
                            b.FirstPos.UnionWith(b.Right.FirstPos);

                        b.LastPos.Clear();
                        b.LastPos.UnionWith(b.Right.LastPos);
                        if (b.Right.Nullable)
                            b.LastPos.UnionWith(b.Left.LastPos);

                        // followpos pentru concatenare
                        foreach (var i in b.Left.LastPos)
                            foreach (var j in b.Right.FirstPos)
                                AddFollow(follow, i, j);
                    }
                    else if (b.Op == '|')
                    {
                        b.Nullable = b.Left.Nullable || b.Right.Nullable;
                        b.FirstPos.Clear();
                        b.FirstPos.UnionWith(b.Left.FirstPos);
                        b.FirstPos.UnionWith(b.Right.FirstPos);
                        b.LastPos.Clear();
                        b.LastPos.UnionWith(b.Left.LastPos);
                        b.LastPos.UnionWith(b.Right.LastPos);
                    }
                    break;
            }
        }

        private static void AddFollow(Dictionary<int, HashSet<int>> dict, int i, int j)
        {
            if (!dict.ContainsKey(i))
                dict[i] = new HashSet<int>();
            dict[i].Add(j);
        }
    }
}
