using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Machine.Specifications.Analyzers
{
    public static class FieldDeclarationSyntaxExtensions
    {
        public static VariableDeclaratorSyntax GetVariable(this FieldDeclarationSyntax field)
        {
            return field.Declaration.Variables
                .FirstOrDefault(x => !x.Identifier.IsMissing);
        }

        public static bool IsInTypeWithSpecifications(this FieldDeclarationSyntax field, SyntaxNodeAnalysisContext context)
        {
            if (!field.Parent.IsKind(SyntaxKind.ClassDeclaration))
            {
                return false;
            }

            if (field.Parent is not TypeDeclarationSyntax type || !type.ContainsSpecifications(context))
            {
                return false;
            }

            return true;
        }

        public static bool IsSpecificationDelegate(this FieldDeclarationSyntax field, SyntaxNodeAnalysisContext context)
        {
            var symbolInfo = ModelExtensions.GetSymbolInfo(context.SemanticModel, field.Declaration.Type, context.CancellationToken);
            var symbol = symbolInfo.Symbol as ITypeSymbol;

            if (!IsMspecDelegate(symbol))
            {
                return false;
            }

            return symbol?.Name is
                MetadataNames.MspecItDelegate or
                MetadataNames.MspecBehavesLikeDelegate or
                MetadataNames.MspecBecause or
                MetadataNames.MspecEstablishDelegate;
        }

        private static bool IsMspecDelegate(ITypeSymbol symbol)
        {
            return IsMspecAssembly(symbol) &&
                   symbol.ContainingNamespace?.ToString() == MetadataNames.MspecNamespace &&
                   symbol.TypeKind == TypeKind.Delegate;
        }

        private static bool IsMspecAssembly(ITypeSymbol symbol)
        {
            return symbol.ContainingAssembly?.Name is
                MetadataNames.MspecAssemblyName or
                MetadataNames.MspecCoreAssemblyName;
        }
    }
}
