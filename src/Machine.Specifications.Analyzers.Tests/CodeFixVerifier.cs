using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Machine.Specifications.Analyzers.Tests
{
    public class CodeFixVerifier<TAnalyzer, TCodeFix> : AnalyzerVerifier<TAnalyzer>
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new()
    {
        public static Task VerifyCodeFixAsync(string source, string fixedSource) =>
            VerifyCodeFixAsync(source, DiagnosticResult.EmptyDiagnosticResults, fixedSource);

        public static Task VerifyCodeFixAsync(string source, DiagnosticResult expected, string fixedSource) =>
            VerifyCodeFixAsync(source, new[] {expected}, fixedSource);

        public static Task VerifyCodeFixAsync(string source, DiagnosticResult[] expected, string fixedSource)
        {
            var test = new CodeFixTest<TAnalyzer, TCodeFix>
            {
                TestCode = source,
                FixedCode = fixedSource
            };

            test.ExpectedDiagnostics.AddRange(expected);

            return test.RunAsync(CancellationToken.None);
        }
    }
}
