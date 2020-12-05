using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Machine.Specifications.Analyzers.Tests
{
    public class AnalyzerVerifier<TAnalyzer>
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        private static readonly TAnalyzer Analyzer = new TAnalyzer();

        public static DiagnosticResult Diagnostic()
        {
            return Diagnostic(Analyzer.SupportedDiagnostics.Single());
        }

        public static DiagnosticResult Diagnostic(string diagnosticId)
        {
            return Diagnostic(Analyzer.SupportedDiagnostics.Single(x => x.Id == diagnosticId));
        }

        public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor)
        {
            return new DiagnosticResult(descriptor);
        }

        public static async Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
        {
            var test = new AnalyzerTest<TAnalyzer>
            {
                TestCode = source
            };
            
            test.ExpectedDiagnostics.AddRange(expected);

            await test.RunAsync(CancellationToken.None);
        }
    }
}
