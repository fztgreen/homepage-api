namespace HomepageApi.Tests.Architecture;

public static class ViolationReport
{
    public static string Of(IEnumerable<string> violations)
    {
        return string.Join(Environment.NewLine, violations);
    }
}
