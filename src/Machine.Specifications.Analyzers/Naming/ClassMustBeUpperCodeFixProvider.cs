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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ClassMustBeUpperCodeFixProvider))]
    public class ClassMustBeUpperCodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticIds.Naming.ClassMustBeUpper);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();

            var type = root.FindToken(diagnostic.Location.SourceSpan.Start)
                .Parent
                .AncestorsAndSelf()
                .OfType<TypeDeclarationSyntax>()
                .First();

            var action = CodeAction.Create("Make uppercase", c => MakeUppercaseAsync(context.Document, type, c), DiagnosticIds.Naming.ClassMustBeUpper);

            context.RegisterCodeFix(action, diagnostic);
        }

        private async Task<Solution> MakeUppercaseAsync(Document document, TypeDeclarationSyntax type, CancellationToken cancellationToken)
        {
            var name = type.Identifier.Text.ToUpperInvariant();

            var model = await document.GetSemanticModelAsync(cancellationToken);
            var symbol = model.GetDeclaredSymbol(type, cancellationToken);
            var options = document.Project.Solution.Workspace.Options;

            return await Renamer.RenameSymbolAsync(document.Project.Solution, symbol, name, options, cancellationToken);
        }
    }
}
