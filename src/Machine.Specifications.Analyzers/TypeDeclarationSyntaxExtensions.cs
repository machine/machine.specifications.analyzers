using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Machine.Specifications.Analyzers
{
    public static class TypeDeclarationSyntaxExtensions
    {
        public static bool ContainsSpecifications(this TypeDeclarationSyntax type, SyntaxNodeAnalysisContext context)
        {
            return type
                .DescendantNodesAndSelf()
                .OfType<TypeDeclarationSyntax>()
                .Any(x => x.Members
                    .OfType<FieldDeclarationSyntax>()
                    .Any(y => y.IsSpecificationDelegate(context)));
        }
    }
}
