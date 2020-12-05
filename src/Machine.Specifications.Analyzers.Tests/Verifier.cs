using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Xunit;
using Xunit.Sdk;

namespace Machine.Specifications.Analyzers.Tests
{
    public class Verifier : IVerifier
    {
        public void Equal<T>(T expected, T actual, string message = null)
        {
            if (message is null)
            {
                Assert.Equal(expected, actual);
            }
            else if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                throw new AssertActualExpectedException(expected, actual, message);
            }
        }

        public void EqualOrDiff(string expected, string actual, string message)
        {
            string GetPrefix(ChangeType type)
            {
                return type switch
                {
                    ChangeType.Inserted => "+",
                    ChangeType.Deleted => "-",
                    _ => " "
                };
            }

            if (expected != actual)
            {
                var builder = new InlineDiffBuilder();
                var diff = builder.BuildDiffModel(expected, actual, false);

                var value = new StringBuilder(message);

                var lines = diff.Lines
                    .Select(x => $"{GetPrefix(x.Type)}{x.Text}");

                foreach (var line in lines)
                {
                    value.AppendLine(line);
                }

                Fail(value.ToString());
            }
        }

        public void Fail(string message)
        {
            throw new XunitException(message);
        }
    }
}
