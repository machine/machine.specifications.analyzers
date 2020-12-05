namespace Machine.Specifications.Analyzers.Tests
{
    public interface IVerifier
    {
        void Equal<T>(T expected, T actual, string message = null);

        void EqualOrDiff(string expected, string actual, string message);

        void Fail(string message);
    }
}
