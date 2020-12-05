using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;

namespace Machine.Specifications.Analyzers.Tests
{
    public class CodeFixTest<TAnalyzer, TCodeFix> : AnalyzerTest<TAnalyzer>
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new()
    {
        public string FixedCode { get; set; }

        protected IEnumerable<CodeFixProvider> GetCodeFixProviders()
        {
            yield return new TCodeFix();
        }

        public override async Task RunAsync(CancellationToken cancellationToken = default)
        {
            var project = CreateProject();

            var diagnostics = await GetDiagnostics(project, cancellationToken);

            VerifyDiagnostics(diagnostics, ExpectedDiagnostics.ToArray());

            if (FixedCode != null)
            {
                var fixedProject = await ApplyCodeFixes(project, diagnostics, cancellationToken);

                await VerifyFixes(fixedProject, cancellationToken);
            }
        }

        private async Task<Project> ApplyCodeFixes(Project project, ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken)
        {
            var codeFixProviders = GetCodeFixProviders().ToArray();

            var current = project;

            while (diagnostics.Any())
            {
                var fixableDiagnostics = diagnostics
                    .Where(x => codeFixProviders.Any(y => y.FixableDiagnosticIds.Contains(x.Id)))
                    .Where(x => project.GetDocument(x.Location.SourceTree) is { });

                foreach (var diagnostic in fixableDiagnostics)
                {
                    var actions = await GetCodeActions(project, codeFixProviders, diagnostic, cancellationToken);

                    current = await ApplyCodeAction(current, actions, cancellationToken);
                }

                diagnostics = await GetDiagnostics(current, cancellationToken);
            }

            return current;
        }

        private async Task<ImmutableArray<CodeAction>> GetCodeActions(Project project, CodeFixProvider[] codeFixProviders, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            var actions = ImmutableArray.CreateBuilder<CodeAction>();

            foreach (var codeFixProvider in codeFixProviders)
            {
                if (codeFixProvider.FixableDiagnosticIds.Contains(diagnostic.Id))
                {
                    var context = new CodeFixContext(
                        project.GetDocument(diagnostic.Location.SourceTree),
                        diagnostic,
                        (action, array) => actions.Add(action),
                        cancellationToken);

                    await codeFixProvider.RegisterCodeFixesAsync(context);
                }
            }

            return actions.ToImmutable();
        }

        private async Task<Project> ApplyCodeAction(Project project, ImmutableArray<CodeAction> codeActions, CancellationToken cancellationToken)
        {
            var codeAction = codeActions.FirstOrDefault();

            if (codeAction == null)
            {
                return project;
            }

            var operations = await codeAction.GetOperationsAsync(cancellationToken);

            return operations.OfType<ApplyChangesOperation>()
                .Single()
                .ChangedSolution
                .GetProject(project.Id);
        }

        private async Task VerifyFixes(Project project, CancellationToken cancellationToken)
        {
            var document = project.Documents.FirstOrDefault();
            var source = await GetSourceTextFromDocument(document, cancellationToken);

            Verifier.Equal(1, project.Documents.Count(), $"Expected 1 document, found {project.Documents.Count()} instead");
            Verifier.EqualOrDiff(FixedCode, source.ToString(), "Actual and expected source code differs");
        }

        private async Task<SourceText> GetSourceTextFromDocument(Document document, CancellationToken cancellationToken)
        {
            var simplifiedDoc = await Simplifier.ReduceAsync(document, Simplifier.Annotation, null, cancellationToken);
            var formatted = await Formatter.FormatAsync(simplifiedDoc, Formatter.Annotation, null, cancellationToken);

            return await formatted.GetTextAsync(cancellationToken);
        }
    }
}
