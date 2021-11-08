using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
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
                var declaration = root
                    .FindNode(diagnostic.Location.SourceSpan)
                    .FirstFieldOrClassAncestor();

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
            return declaration switch
            {
                ClassDeclarationSyntax type => await HandleClassDeclaration(document, type, token).ConfigureAwait(false),
                FieldDeclarationSyntax field => await HandleFieldDeclaration(document, field, token).ConfigureAwait(false),
                _ => document.Project.Solution
            };
        }

        private async Task<Solution> HandleClassDeclaration(Document document, ClassDeclarationSyntax type, CancellationToken token)
        {
            return await RenameDeclarationAsync(document, type, type.Identifier, token).ConfigureAwait(false);
        }

        private async Task<Solution> HandleFieldDeclaration(Document document, FieldDeclarationSyntax field, CancellationToken token)
        {
            var variable = field.GetVariable();

            if (variable == null)
            {
                return document.Project.Solution;
            }

            return await RenameDeclarationAsync(document, variable, variable.Identifier, token)
                .ConfigureAwait(false);
        }

        private async Task<Solution> RenameDeclarationAsync(Document document, SyntaxNode node, SyntaxToken identifier, CancellationToken token)
        {
            var name = GetSnakeCase(identifier.Text);

            var model = await document.GetSemanticModelAsync(token).ConfigureAwait(false);
            var symbol = model?.GetDeclaredSymbol(node, token);
            var options = document.Project.Solution.Workspace.Options;

            if (symbol == null)
            {
                return document.Project.Solution;
            }

            return await Renamer.RenameSymbolAsync(document.Project.Solution, symbol, name, options, token)
                .ConfigureAwait(false);
        }

        private string GetSnakeCase(string value)
        {
            var parts = value.Select((x, i) => i > 0 && char.IsUpper(x)
                ? "_" + x
                : x.ToString());

            return string.Concat(parts).ToLower();
        }
    }
}
