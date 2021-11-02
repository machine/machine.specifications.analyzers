using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Machine.Specifications.Analyzers.Maintainability
{
    [Shared]
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public class AccessModifierShouldNotBeUsedCodeFix : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(DiagnosticIds.Naming.AccessModifierShouldNotBeUsed);

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

                var declaration = FindParentDeclaration(node);

                if (declaration == null)
                {
                    continue;
                }

                context.RegisterCodeFix(
                    CodeAction.Create("Remove access modifier", _ => TransformAsync(context.Document, root, declaration)),
                    diagnostic);
            }
        }

        private Task<Document> TransformAsync(Document document, SyntaxNode node, SyntaxNode declaration)
        {
            if (declaration.IsKind(SyntaxKind.ClassDeclaration))
            {
            }
        }

        private SyntaxNode TransformClassDeclaration(ClassDeclarationSyntax node)
        {
            if (node.Keyword.IsMissing)
            {
                return null;
            }

            var modifiers = SyntaxFactory.Token(SyntaxKind.EmptyStatement)

            return node
                .WithModifiers(SyntaxTokenList.Create)
        }

        private SyntaxNode FindParentDeclaration(SyntaxNode node)
        {
            if (node.IsKind(SyntaxKind.ClassDeclaration))
            {
                return node;
            }

            return node?.Parent;
        }
    }
}
