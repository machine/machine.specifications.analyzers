using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Machine.Specifications.Analyzers
{
    public static class SyntaxNodeExtensions
    {
        public static SyntaxNode FirstFieldOrClassAncestor(this SyntaxNode node)
        {
            while (node != null)
            {
                if (node.IsKind(SyntaxKind.ClassDeclaration) || node.IsKind(SyntaxKind.FieldDeclaration))
                {
                    return node;
                }

                node = node.Parent;
            }

            return null;
        }
    }
}
