using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Machine.Specifications.Analyzers
{
    public static class FieldDeclarationSyntaxExtensions
    {
        public static bool IsSpecification(this FieldDeclarationSyntax syntax, SyntaxNodeAnalysisContext context)
        {
            var symbolInfo = context.SemanticModel.GetSymbolInfo(syntax.Declaration.Type, context.CancellationToken);
            var symbol = symbolInfo.Symbol as ITypeSymbol;

            return symbol.IsMember() && symbol?.Name is MetadataNames.MspecItDelegate or MetadataNames.MspecBehavesLikeDelegate;
        }
    }
}
