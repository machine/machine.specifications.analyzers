using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace Machine.Specifications.Analyzers.Tests.Verifiers
{
    public static class RefactoringVerifier<TCodeRefactoring>
        where TCodeRefactoring : CodeRefactoringProvider, new()
    {
        public static async Task VerifyRefactoringAsync(string source, string fixedSource)
        {
            await VerifyRefactoringAsync(source, DiagnosticResult.EmptyDiagnosticResults, fixedSource);
        }

        public static async Task VerifyRefactoringAsync(string source, DiagnosticResult expected, string fixedSource)
        {
            await VerifyRefactoringAsync(source, new[] { expected }, fixedSource);
        }

        public static async Task VerifyRefactoringAsync(string source, DiagnosticResult[] expected, string fixedSource)
        {
            var test = new Test
            {
                TestCode = source,
                FixedCode = fixedSource,
            };

            test.ExpectedDiagnostics.AddRange(expected);

            await test.RunAsync(CancellationToken.None);
        }

        private class Test : CSharpCodeRefactoringTest<TCodeRefactoring, XUnitVerifier>
        {
            public Test()
            {
                SolutionTransforms.Add(VerifierHelper.GetNullableTransform);
            }
        }
    }
}
