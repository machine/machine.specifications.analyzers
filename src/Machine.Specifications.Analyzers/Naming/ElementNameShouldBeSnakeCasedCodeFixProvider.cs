using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;

namespace Machine.Specifications.Analyzers.Naming
{
    [Shared]
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public class ElementNameShouldBeSnakeCasedCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(DiagnosticIds.Naming.ElementNameShouldBeSnakeCased);

        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                .ConfigureAwait(false);

            if (root == null)
            {
                return;
            }

            foreach (var diagnostic in context.Diagnostics)
            {
                var node = root.FindNode(diagnostic.Location.SourceSpan, true);

                if (node.IsMissing)
                {
                    continue;
                }

                var declaration = GetParentDeclaration(node);

                if (declaration == null)
                {
                    continue;
                }

                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Make snake case",
                        token => TransformAsync(context.Document, declaration, token),
                        nameof(ElementNameShouldBeSnakeCasedCodeFixProvider)),
                    diagnostic);
            }
        }

        private async Task<Solution> TransformAsync(Document document, SyntaxNode declaration, CancellationToken token)
        {
            return declaration.Kind() switch
            {
                SyntaxKind.ClassDeclaration => HandleDeclaration(document, (ClassDeclarationSyntax)declaration, token),
                SyntaxKind.FieldDeclaration => HandleDeclaration((FieldDeclarationSyntax)declaration),
                _ => document.Project.Solution
            };
        }

        private async Task<Solution> HandleDeclaration(Document document, ClassDeclarationSyntax type, CancellationToken token)
        {
            var parts = type.Identifier.Text.Select((x, i) => i > 0 && char.IsUpper(x)
                ? "_" + x
                : x.ToString());

            var name = string.Concat(parts).ToLower();

            var model = await document.GetSemanticModelAsync(token);
            var symbol = model?.GetDeclaredSymbol(type, token);

            if (symbol == null)
            {
                return document.Project.Solution;
            }

            return await Renamer.RenameSymbolAsync(document.Project.Solution, symbol, name, document.Project.Solution.Workspace.Options, token);
        }

        private async Task<Solution> HandleDeclaration(Document document, FieldDeclarationSyntax field, CancellationToken token)
        {
            var variable = field.Declaration.Variables
                .FirstOrDefault(x => !x.IsMissing);

            if (variable == null)
            {
                return document.Project.Solution;
            }

            var parts = variable.Identifier.Text.Select((x, i) => i > 0 && char.IsUpper(x)
                ? "_" + x
                : x.ToString());

            var name = string.Concat(parts).ToLower();

            var model = await document.GetSemanticModelAsync(token);
            var symbol = model?.GetDeclaredSymbol(variable, token);

            if (symbol == null)
            {
                return document.Project.Solution;
            }

            return await Renamer.RenameSymbolAsync(document.Project.Solution, symbol, name, document.Project.Solution.Workspace.Options, token);
        }

        private async Task<Solution> RenameDeclaration(Document document, CancellationToken token)

        private SyntaxNode GetParentDeclaration(SyntaxNode declaration)
        {
            while (declaration != null)
            {
                if (declaration.IsKind(SyntaxKind.ClassDeclaration) || declaration.IsKind(SyntaxKind.FieldDeclaration))
                {
                    return declaration;
                }

                declaration = declaration.Parent;
            }

            return null;
        }
    }
}
