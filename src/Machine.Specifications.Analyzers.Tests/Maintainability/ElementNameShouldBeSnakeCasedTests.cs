using System.Threading.Tasks;
using Xunit;
using Verify = Machine.Specifications.Analyzers.Tests.CodeFixVerifier<
    Machine.Specifications.Analyzers.Naming.ElementNameShouldBeSnakeCasedAnalyzer,
    Machine.Specifications.Analyzers.Naming.ElementNameShouldBeSnakeCasedCodeFixProvider>;

namespace Machine.Specifications.Analyzers.Tests.Maintainability
{
    public class ElementNameShouldBeSnakeCasedTests
    {
        [Fact]
        public async Task NoErrorsInValidSource()
        {
            const string source = @"
using System;
using Machine.Specifications;

namespace ConsoleApplication1
{
    class when_writing_a_class
    {
        static string value;

        It should_do_something = () =>
            true.ShouldBeTrue();

        class inner_specs
        {
            Establish context = () =>
                value = string.Empty;
        }
    }
}";

            await Verify.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task RenamesClassName()
        {
            const string source = @"
using System;
using Machine.Specifications;

namespace ConsoleApplication1
{
    class {|#0:SpecsClass|}
    {
        It should_do_something = () =>
            true.ShouldBeTrue();
    }
}";

            const string fixedSource = @"
using System;
using Machine.Specifications;

namespace ConsoleApplication1
{
    class specs_class
    {
        It should_do_something = () =>
            true.ShouldBeTrue();
    }
}";

            var expected = Verify.Diagnostic(DiagnosticIds.Naming.ElementNameShouldBeSnakeCased)
                .WithLocation(0)
                .WithArguments("SpecsClass");

            await Verify.VerifyCodeFixAsync(source, expected, fixedSource);
        }
    }
}
