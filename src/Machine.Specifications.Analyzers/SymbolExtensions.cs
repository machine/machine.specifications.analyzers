using Microsoft.CodeAnalysis;

namespace Machine.Specifications.Analyzers
{
    public static class SymbolExtensions
    {
        public static bool IsAssignableFrom(this ITypeSymbol symbol, ITypeSymbol source)
        {
            if (symbol is null)
            {
                return false;
            }

            while (source is not null)
            {
                if (source.Equals(symbol, SymbolEqualityComparer.Default))
                {
                    return true;
                }

                source = source.BaseType;
            }

            return false;
        }

        public static bool IsMember(this ITypeSymbol symbol)
        {
            return symbol.IsMspecAssembly() &&
                   symbol.ContainingNamespace?.ToString() == MetadataNames.MspecNamespace &&
                   symbol.TypeKind == TypeKind.Delegate;
        }

        private static bool IsMspecAssembly(this ITypeSymbol symbol)
        {
            return symbol.ContainingAssembly?.Name is MetadataNames.MspecAssemblyName or MetadataNames.MspecCoreAssemblyName;
        }
    }
}
