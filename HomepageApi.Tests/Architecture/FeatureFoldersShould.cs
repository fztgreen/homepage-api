using System.Reflection;
using System.Runtime.CompilerServices;

using FluentAssertions;

namespace HomepageApi.Tests.Architecture;

[TestClass]
public class FeatureFoldersShould
{
    private static readonly string[] _featureFolderRoots =
    [
        "HomepageApi.Domain.Commands",
        "HomepageApi.Domain.Queries",
        "HomepageApi.Domain.DataAccess",
        "HomepageApi.Domain.Entities",
        "HomepageApi.Domain.Exceptions",
        "HomepageApi.DataAccess",
        "HomepageApi.Api.Models"
    ];

    [TestMethod]
    public void HoldEveryTypeInsteadOfTheFolderRoot()
    {
        // Arrange
        var assemblies = SolutionAssemblies.Production;

        // Act
        var typesOutsideFeatureFolders = TypesDirectlyInFeatureFolderRoots(assemblies);

        // Assert
        ViolationReport.Of(typesOutsideFeatureFolders).Should().BeEmpty();
    }

    private static List<string> TypesDirectlyInFeatureFolderRoots(Assembly[] assemblies)
    {
        var types = new List<string>();
        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (!type.IsDefined(typeof(CompilerGeneratedAttribute)) && _featureFolderRoots.Contains(type.Namespace))
                {
                    types.Add(type.FullName!);
                }
            }
        }

        return types;
    }
}
