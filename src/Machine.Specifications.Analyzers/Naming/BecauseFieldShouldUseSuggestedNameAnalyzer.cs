using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Machine.Specifications.Analyzers.Naming
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class BecauseFieldShouldUseSuggestedNameAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor Rule = new(
            DiagnosticIds.Naming.BecauseFieldShouldUseSuggestedName,
            "Because field should use suggested naming",
            "Because field '{0}' should use suggested naming",
            DiagnosticCategories.Naming,
            DiagnosticSeverity.Info,
            true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            // Named `context` or `that`
        }
    }
}
