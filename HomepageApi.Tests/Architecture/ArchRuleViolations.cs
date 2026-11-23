using ArchUnitNET.Fluent;

namespace HomepageApi.Tests.Architecture;

public static class ArchRuleViolations
{
    public static IEnumerable<string> Of(IArchRule rule)
    {
        return rule.Evaluate(SolutionAssemblies.Architecture).Where(HasFailed).Select(Describe);
    }

    private static bool HasFailed(EvaluationResult result)
    {
        return !result.Passed;
    }

    private static string Describe(EvaluationResult result)
    {
        return result.Description;
    }
}
