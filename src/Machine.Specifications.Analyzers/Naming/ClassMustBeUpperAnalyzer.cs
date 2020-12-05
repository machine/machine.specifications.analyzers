using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Machine.Specifications.Analyzers.Naming
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ClassMustBeUpperAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            DiagnosticIds.Naming.ClassMustBeUpper,
            "Class must be uppercase",
            "Class {0} must be uppercase",
            "Naming",
            DiagnosticSeverity.Error,
            true,
            "Rename the class to be uppercase",
            "http://machine.specs/link");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
        }

        private void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var symbol = (INamedTypeSymbol) context.Symbol;

            if (symbol.Name.ToCharArray().Any(char.IsLower))
            {
                var diagnostic = Diagnostic.Create(Rule, symbol.Locations[0], symbol.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
