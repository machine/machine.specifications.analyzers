using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Machine.Specifications.Analyzers.Tests
{
    public class AnalyzerTest<TAnalyzer>
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        private const string ProjectFileName = "TestProject";

        private const string FileName = "/0/Test0.cs";

        private readonly ImmutableArray<DiagnosticAnalyzer> analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(new TAnalyzer());

        protected IVerifier Verifier { get; } = new Verifier();

        public string TestCode { get; set; }

        public List<DiagnosticResult> ExpectedDiagnostics { get; } = new List<DiagnosticResult>();

        public virtual async Task RunAsync(CancellationToken cancellationToken = default)
        {
            var project = CreateProject();

            var diagnostics = await GetDiagnostics(project, cancellationToken);

            VerifyDiagnostics(diagnostics, ExpectedDiagnostics.ToArray());
        }

        protected Project CreateProject()
        {
            var compilation = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            var parse = new CSharpParseOptions(LanguageVersion.Default, DocumentationMode.Diagnose);

            var projectId = ProjectId.CreateNewId(ProjectFileName);

            var solution = new AdhocWorkspace()
                .CurrentSolution
                .AddProject(projectId, ProjectFileName, ProjectFileName, LanguageNames.CSharp)
                .WithProjectCompilationOptions(projectId, compilation)
                .WithProjectParseOptions(projectId, parse)
                .AddMetadataReference(projectId, MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                .AddDocument(DocumentId.CreateNewId(projectId, FileName), FileName, SourceText.From(TestCode), filePath: FileName);

            return solution.GetProject(projectId);
        }

        protected async Task<ImmutableArray<Diagnostic>> GetDiagnostics(Project project, CancellationToken cancellationToken)
        {
            var compilation = await project.GetCompilationAsync(cancellationToken);

            var diagnostics = await compilation
                .WithAnalyzers(analyzers, project.AnalyzerOptions, cancellationToken)
                .GetAllDiagnosticsAsync(cancellationToken);

            return diagnostics
                .Where(x => !x.Descriptor.CustomTags.Contains(WellKnownDiagnosticTags.Compiler))
                .ToImmutableArray();
        }

        protected void VerifyDiagnostics(ImmutableArray<Diagnostic> actual, DiagnosticResult[] expected)
        {
            var matched = MatchDiagnostics(actual, expected);

            Verifier.Equal(expected.Length, actual.Length, "Number of expected diagnostics does not match actual diagnostics");
            
            foreach (var (matchedActual, matchedExpected) in matched)
            {
                if (matchedActual is null && matchedExpected is { })
                {
                    Verifier.Fail($"Expected '{matchedExpected.Value.Id}' but actual diagnostic was not raised");
                }

                if (matchedActual is { } && matchedExpected is null)
                {
                    Verifier.Fail($"Actual diagnostic '{matchedActual.Id}' was raised but was not expected");
                }
            }
        }

        private ImmutableArray<(Diagnostic actual, DiagnosticResult? expected)> MatchDiagnostics(ImmutableArray<Diagnostic> actual, DiagnosticResult[] expected)
        {
            var results = ImmutableArray.CreateBuilder<(Diagnostic actual, DiagnosticResult? expected)>();

            var actualById = actual.ToLookup(x => x.Id);

            foreach (var value in expected)
            {
                var actualValues = actualById[value.Id].ToArray();

                if (actualValues.Any() &&
                    actualValues.Any(x => IsSeverityMatch(x, value)) &&
                    actualValues.Any(x => IsMessageMatch(x, value)))
                {
                    results.Add((actualValues.First(), value));
                }
                else
                {
                    results.Add((null, value));
                }
            }

            var actualUsed = results
                .Select(x => x.actual)
                .Where(x => x is { });

            foreach (var unused in actual.Except(actualUsed))
            {
                results.Add((unused, null));
            }

            return results.ToImmutable();
        }

        private bool IsSeverityMatch(Diagnostic diagnostic, DiagnosticResult result)
        {
            return result.Options == DiagnosticOptions.IgnoreSeverity || diagnostic.Severity == result.Severity;
        }

        private bool IsMessageMatch(Diagnostic diagnostic, DiagnosticResult result)
        {
            return result.Message is null || string.Equals(result.Message, diagnostic.GetMessage());
        }
    }
}
