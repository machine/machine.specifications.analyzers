using Microsoft.CodeAnalysis;

namespace Machine.Specifications.Analyzers.Tests
{
    public readonly struct DiagnosticResult
    {
        public static readonly DiagnosticResult[] EmptyDiagnosticResults = { };

        public DiagnosticResult(DiagnosticDescriptor descriptor)
        {
            Id = descriptor.Id;
            Severity = descriptor.DefaultSeverity;
            Message = null;
            Options = DiagnosticOptions.None;
        }

        private DiagnosticResult(string id, string message, DiagnosticSeverity severity, DiagnosticOptions options)
        {
            Id = id;
            Severity = severity;
            Message = message;
            Options = options;
        }

        public string Id { get; }

        public DiagnosticSeverity Severity { get; }

        public DiagnosticOptions Options { get; }

        public string Message { get; }

        public DiagnosticResult WithSeverity(DiagnosticSeverity severity)
        {
            return new DiagnosticResult(Id, Message, severity, Options);
        }

        public DiagnosticResult WithMessage(string message)
        {
            return new DiagnosticResult(Id, message, Severity, Options);
        }

        public DiagnosticResult WithOptions(DiagnosticOptions options)
        {
            return new DiagnosticResult(Id, Message, Severity, options);
        }
    }
}
