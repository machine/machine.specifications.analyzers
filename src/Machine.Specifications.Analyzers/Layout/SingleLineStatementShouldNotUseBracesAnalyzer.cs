using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Machine.Specifications.Analyzers.Layout
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SingleLineStatementShouldNotUseBracesAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor Rule = new(
            DiagnosticIds.Layout.SingleLineStatementShouldNotUseBraces,
            "Lambda expression with single statement should not use braces",
            "Element '{0}' lambda expression with single statement should not use braces",
            DiagnosticCategories.Layout,
            DiagnosticSeverity.Warning,
            true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSyntaxNodeAction(AnalyzeVariableSyntax, SyntaxKind.VariableDeclaration);
        }

        private void AnalyzeVariableSyntax(SyntaxNodeAnalysisContext context)
        {
            var syntax = (VariableDeclarationSyntax) context.Node;

            if (!syntax.Parent.IsKind(SyntaxKind.ClassDeclaration))
            {
                return;
            }

            var variable = syntax.Variables
                .FirstOrDefault(x => !x.Identifier.IsMissing);

            if (variable?.Initializer?.Value is not LambdaExpressionSyntax lambda)
            {
                return;
            }

            if (lambda.Body is BlockSyntax {Statements: {Count: 1}} block &&
                block.Statements.First().IsKind(SyntaxKind.ExpressionStatement))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, lambda.GetLocation(), variable.Identifier));
            }
        }
    }
}
