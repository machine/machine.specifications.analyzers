using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Machine.Specifications.Analyzers
{
    public abstract class MspecDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        protected abstract void AnalyzeCompilation(CompilationStartAnalysisContext context, MspecContext mspecContext);

        protected virtual MspecContext CreateContext(Compilation compilation)
        {
            return new MspecContext(compilation);
        }

        protected virtual bool ShouldAnalyze(MspecContext context)
        {
            return context.HasMspecReferences;
        }

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterCompilationStartAction(AnalyzeCompilation);
        }

        private void AnalyzeCompilation(CompilationStartAnalysisContext context)
        {
            var mspecContext = CreateContext(context.Compilation);

            if (ShouldAnalyze(mspecContext))
            {
                AnalyzeCompilation(context, mspecContext);
            }
        }
    }
}
