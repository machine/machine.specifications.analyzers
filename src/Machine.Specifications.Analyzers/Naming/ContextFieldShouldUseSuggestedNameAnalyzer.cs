using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Machine.Specifications.Analyzers.Naming
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ContextFieldShouldUseSuggestedNameAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor Rule = new(
            DiagnosticIds.Naming.ContextFieldShouldUseSuggestedName,
            "Context field should use suggested naming",
            "Context field '{0}' should use suggested naming",
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
