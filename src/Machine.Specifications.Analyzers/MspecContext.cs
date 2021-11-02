using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Machine.Specifications.Analyzers
{
    public class MspecContext
    {
        public MspecContext(Compilation compilation)
        {
            HasMspecReferences = compilation.ReferencedAssemblyNames
                .Any(x => x.Name.Equals("machine.specifications", StringComparison.OrdinalIgnoreCase));

            SpecificationType = compilation.GetTypeByMetadataName("Machine.Specifications.It");
            BehavesLikeType = compilation.GetTypeByMetadataName("Machine.Specifications.Behaves_like");
            BecauseType = compilation.GetTypeByMetadataName("Machine.Specifications.Because");
            EstablishType = compilation.GetTypeByMetadataName("Machine.Specifications.Establish");
        }

        public bool HasMspecReferences { get; }

        public INamedTypeSymbol SpecificationType { get; }

        public INamedTypeSymbol BehavesLikeType { get; }

        public INamedTypeSymbol BecauseType { get; }

        public INamedTypeSymbol EstablishType { get; }

        public bool IsSpecificationType(ITypeSymbol symbol)
        {
            return SpecificationType.IsAssignableFrom(symbol) || BehavesLikeType.IsAssignableFrom(symbol);
        }
    }
}
