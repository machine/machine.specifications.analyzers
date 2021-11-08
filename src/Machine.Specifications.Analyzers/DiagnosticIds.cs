namespace Machine.Specifications.Analyzers
{
    public static class DiagnosticIds
    {
        public static class Naming
        {
            public const string ElementNameShouldBeSnakeCased = "MSP1001";

            public const string ContextFieldShouldUseSuggestedName = "MSP1002";

            public const string BecauseFieldShouldUseSuggestedName = "MSP1003";
        }

        public static class Maintainability
        {
            public const string AccessModifierShouldNotBeUsed = "MSP2002";
        }

        public static class Layout
        {
            public const string DelegateShouldNotBeOnSingleLine = "MSP3001";

            public const string SingleLineStatementShouldNotUseBraces = "MSP3002";
        }
    }
}
