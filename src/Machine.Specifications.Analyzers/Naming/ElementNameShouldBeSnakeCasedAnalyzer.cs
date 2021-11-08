using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Machine.Specifications.Analyzers.Naming
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ElementNameShouldBeSnakeCasedAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor Rule = new(
            DiagnosticIds.Naming.ElementNameShouldBeSnakeCased,
            "Element should be snake cased",
            "Element name '{0}' should be snake cased",
            DiagnosticCategories.Naming,
            DiagnosticSeverity.Warning,
            true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSyntaxNodeAction(AnalyzeTypeSyntax, SyntaxKind.ClassDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeFieldSyntax, SyntaxKind.FieldDeclaration);
        }

        private void AnalyzeTypeSyntax(SyntaxNodeAnalysisContext context)
        {
            var type = (TypeDeclarationSyntax) context.Node;

            if (!type.IsSpecificationClass(context))
            {
                return;
            }

            if (IsSnakeCased(type.Identifier))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, type.Identifier.GetLocation(), type.Identifier));
        }

        private void AnalyzeFieldSyntax(SyntaxNodeAnalysisContext context)
        {
            var field = (FieldDeclarationSyntax) context.Node;

            if (!field.Parent.IsKind(SyntaxKind.ClassDeclaration))
            {
                return;
            }

            if (field.Parent is not TypeDeclarationSyntax type || !type.IsSpecificationClass(context))
            {
                return;
            }

            var variable = field.Declaration.Variables
                .FirstOrDefault(x => !x.Identifier.IsMissing);

            if (variable == null)
            {
                return;
            }

            if (IsSnakeCased(variable.Identifier))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Rule, variable.Identifier.GetLocation(), variable.Identifier));
        }

        private bool IsSnakeCased(SyntaxToken token)
        {
            return token.Text.All(x => char.IsLower(x) || char.IsDigit(x) || x == '_');
        }
    }
}
