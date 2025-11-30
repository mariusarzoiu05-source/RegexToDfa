using System;
using System.Collections.Generic;

namespace LFC_TEMA1.Core
{
    /// Calculează nullable, firstpos, lastpos și followpos pentru arborele sintactic.
    /// Se bazează pe nodurile definite în SyntaxNode.cs.
    public static class FollowPosBuilder
    {
        /// întoarcem dicționarul followpos: poziție -> mulțimea de poziții
        public static Dictionary<int, HashSet<int>> Build(SyntaxNode root)
        {
            var followpos = new Dictionary<int, HashSet<int>>();
            Compute(root, followpos);
            return followpos;
        }

        private static void Ensure(Dictionary<int, HashSet<int>> fp, int pos)
        {
            if (!fp.ContainsKey(pos))
                fp[pos] = new HashSet<int>();
        }

        /// Parcurgere bottom-up pe arbore:
        ///  - calculează pentru fiecare nod: Nullable, FirstPos, LastPos
        ///  - actualizează followpos pentru operatorii . și *, +
        private static void Compute(
            SyntaxNode node,
            Dictionary<int, HashSet<int>> followpos)
        {
            switch (node)
            {
                case SymbolNode s:
                    // La frunză avem deja setate Nullable / FirstPos / LastPos în constructor.
                    Ensure(followpos, s.Position);
                    break;

                case UnaryNode u:
                    // calculează copilul
                    Compute(u.Child, followpos);

                    // preluăm first/last de la copil
                    u.FirstPos.Clear();
                    u.FirstPos.UnionWith(u.Child.FirstPos);

                    u.LastPos.Clear();
                    u.LastPos.UnionWith(u.Child.LastPos);

                    // nullable + followpos în funcție de operator
                    switch (u.Op)
                    {
                        case '*':
                            // e* este mereu nullable
                            u.Nullable = true;

                            // pentru fiecare i din lastpos(e) adaugăm firstpos(e) la followpos(i)
                            foreach (var i in u.Child.LastPos)
                            {
                                Ensure(followpos, i);
                                followpos[i].UnionWith(u.Child.FirstPos);
                            }
                            break;

                        case '+':
                            // e+ are aceleași first/last, dar nullable = nullable(e)
                            u.Nullable = u.Child.Nullable;

                            // ca la *, și aici pot urma alte repetări
                            foreach (var i in u.Child.LastPos)
                            {
                                Ensure(followpos, i);
                                followpos[i].UnionWith(u.Child.FirstPos);
                            }
                            break;

                        case '?':
                            // e? este întotdeauna nullable
                            u.Nullable = true;
                            // nu modificăm followpos
                            break;

                        default:
                            throw new InvalidOperationException($"Operator unar necunoscut: {u.Op}");
                    }

                    break;

                case BinaryNode b:
                    // întâi copii
                    Compute(b.Left, followpos);
                    Compute(b.Right, followpos);

                    if (b.Op == '|')
                    {
                        // OR
                        b.Nullable = b.Left.Nullable || b.Right.Nullable;

                        b.FirstPos.Clear();
                        b.FirstPos.UnionWith(b.Left.FirstPos);
                        b.FirstPos.UnionWith(b.Right.FirstPos);

                        b.LastPos.Clear();
                        b.LastPos.UnionWith(b.Left.LastPos);
                        b.LastPos.UnionWith(b.Right.LastPos);
                    }
                    else if (b.Op == '.')
                    {
                        // CONCAT
                        b.Nullable = b.Left.Nullable && b.Right.Nullable;

                        b.FirstPos.Clear();
                        if (b.Left.Nullable)
                        {
                            b.FirstPos.UnionWith(b.Left.FirstPos);
                            b.FirstPos.UnionWith(b.Right.FirstPos);
                        }
                        else
                        {
                            b.FirstPos.UnionWith(b.Left.FirstPos);
                        }

                        b.LastPos.Clear();
                        if (b.Right.Nullable)
                        {
                            b.LastPos.UnionWith(b.Left.LastPos);
                            b.LastPos.UnionWith(b.Right.LastPos);
                        }
                        else
                        {
                            b.LastPos.UnionWith(b.Right.LastPos);
                        }

                        // followpos: pentru fiecare i din lastpos(stânga) adăugăm firstpos(dreapta) la followpos(i)
                        foreach (var i in b.Left.LastPos)
                        {
                            Ensure(followpos, i);
                            followpos[i].UnionWith(b.Right.FirstPos);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException($"Operator binar necunoscut: {b.Op}");
                    }

                    break;

                default:
                    throw new InvalidOperationException("Tip de nod necunoscut în arbore.");
            }
        }
    }
}
