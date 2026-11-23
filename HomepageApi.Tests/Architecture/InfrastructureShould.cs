using FluentAssertions;

namespace HomepageApi.Tests.Architecture;

[TestClass]
public class InfrastructureShould
{
    private const string WrappersNamespace = "HomepageApi.DataAccess.Wrappers";

    private static readonly string[] _infrastructureNamespaces =
    [
        "System.Net",
        "System.Data",
        "System.IO",
        "System.Collections.Concurrent",
        "Microsoft.Data",
        "Microsoft.EntityFrameworkCore",
        "Microsoft.Extensions.Http",
        "MongoDB",
        "Dapper"
    ];

    [TestMethod]
    public void OnlyBeReachedThroughWrappers()
    {
        // Arrange
        System.Reflection.Assembly[] layersThatMustUseWrappers = [SolutionAssemblies.Domain, SolutionAssemblies.DataAccess, SolutionAssemblies.Api];

        // Act
        var violations = NamespaceDependencies.Find(layersThatMustUseWrappers, _infrastructureNamespaces, WrappersNamespace);

        // Assert
        ViolationReport.Of(violations).Should().BeEmpty();
    }
}
