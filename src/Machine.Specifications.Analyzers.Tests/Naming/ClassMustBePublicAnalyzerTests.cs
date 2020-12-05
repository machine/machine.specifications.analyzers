using System.Threading.Tasks;
using Xunit;
using Verify = Machine.Specifications.Analyzers.Tests.CodeFixVerifier<
    Machine.Specifications.Analyzers.Naming.ClassMustBeUpperAnalyzer,
    Machine.Specifications.Analyzers.CodeFixes.Naming.ClassMustBeUpperCodeFixProvider>;

namespace Machine.Specifications.Analyzers.Tests.Naming
{
    public class ClassMustBePublicAnalyzerTests
    {
        [Fact]
        public async Task NoErrorsFoundInPublicClass()
        {
            const string source = @"public class TESTCLASS { }";

            await Verify.VerifyAnalyzerAsync(source);
        }

        [Fact]
        public async Task CanApplyCodeFix()
        {
            const string source = @"
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class typename
        {   
        }
    }";

            const string fixedSource = @"
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TYPENAME
        {   
        }
    }";

            var expected = Verify.Diagnostic(DiagnosticIds.Naming.ClassMustBeUpper);

            await Verify.VerifyCodeFixAsync(source, expected, fixedSource);
        }
    }
}
