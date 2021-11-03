using System.Threading.Tasks;
using Xunit;
using Verify = Machine.Specifications.Analyzers.Tests.CodeFixVerifier<
    Machine.Specifications.Analyzers.Maintainability.AccessModifierShouldNotBeUsed,
    Machine.Specifications.Analyzers.Maintainability.AccessModifierShouldNotBeUsedCodeFix>;

namespace Machine.Specifications.Analyzers.Tests.Maintainability
{
    public class AccessModifierShouldNotBeUsedTests
    {
        [Fact]
        public async Task RemovesClassAccessModifier()
        {
            const string source = @"
    using System;
    using Machine.Specifications;

    namespace ConsoleApplication1
    {
        public class {|#0:SpecsClass|}
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
        class SpecsClass
        {
            It should_do_something = () =>
                true.ShouldBeTrue();
        }
    }";

            var expected = Verify.Diagnostic(DiagnosticIds.Maintainability.AccessModifierShouldNotBeUsed)
                .WithLocation(0)
                .WithArguments("SpecsClass");

            await Verify.VerifyCodeFixAsync(source, expected, fixedSource);
        }

        [Fact]
        public async Task RemovesFieldAccessModifier()
        {
            const string source = @"
    using System;
    using Machine.Specifications;

    namespace ConsoleApplication1
    {
        class SpecsClass
        {
            private It {|#0:should_do_something|} = () =>
                true.ShouldBeTrue();
        }
    }";

            const string fixedSource = @"
    using System;
    using Machine.Specifications;

    namespace ConsoleApplication1
    {
        class SpecsClass
        {
            It should_do_something = () =>
                true.ShouldBeTrue();
        }
    }";

            var expected = Verify.Diagnostic(DiagnosticIds.Maintainability.AccessModifierShouldNotBeUsed)
                .WithLocation(0)
                .WithArguments("should_do_something");

            await Verify.VerifyCodeFixAsync(source, expected, fixedSource);
        }

        [Fact]
        public async Task RemovesFieldAndClassAccessModifiers()
        {
            const string source = @"
    using System;
    using Machine.Specifications;

    namespace ConsoleApplication1
    {
        public class {|#0:SpecsClass|}
        {
            private It {|#1:should_do_something|} = () =>
                true.ShouldBeTrue();
        }
    }";

            const string fixedSource = @"
    using System;
    using Machine.Specifications;

    namespace ConsoleApplication1
    {
        class SpecsClass
        {
            It should_do_something = () =>
                true.ShouldBeTrue();
        }
    }";

            var expectedClass = Verify.Diagnostic(DiagnosticIds.Maintainability.AccessModifierShouldNotBeUsed)
                .WithLocation(0)
                .WithArguments("SpecsClass");

            var expectedField = Verify.Diagnostic(DiagnosticIds.Maintainability.AccessModifierShouldNotBeUsed)
                .WithLocation(1)
                .WithArguments("should_do_something");

            await Verify.VerifyCodeFixAsync(source, new[] {expectedClass, expectedField}, fixedSource);
        }
    }
}
