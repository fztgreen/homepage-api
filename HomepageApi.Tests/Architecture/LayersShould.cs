using System.Reflection;

using FluentAssertions;

namespace HomepageApi.Tests.Architecture;

[TestClass]
public class LayersShould
{
    [TestMethod]
    public void KeepDomainFreeOfDependencies()
    {
        // Arrange
        var domainReferences = SolutionAssemblies.Domain.GetReferencedAssemblies();

        // Act
        var nonSystemReferences = NamesNotStartingWith(domainReferences, "System");

        // Assert
        ViolationReport.Of(nonSystemReferences).Should().BeEmpty();
    }

    [TestMethod]
    public void KeepApiIndependentOfDataAccessAndIoC()
    {
        // Arrange
        var apiReferences = SolutionAssemblies.Api.GetReferencedAssemblies();

        // Act
        var forbiddenReferences = NamesOf(apiReferences, SolutionAssemblies.DataAccess, SolutionAssemblies.IoC);

        // Assert
        ViolationReport.Of(forbiddenReferences).Should().BeEmpty();
    }

    [TestMethod]
    public void KeepDataAccessIndependentOfApiAndIoC()
    {
        // Arrange
        var dataAccessReferences = SolutionAssemblies.DataAccess.GetReferencedAssemblies();

        // Act
        var forbiddenReferences = NamesOf(dataAccessReferences, SolutionAssemblies.Api, SolutionAssemblies.IoC);

        // Assert
        ViolationReport.Of(forbiddenReferences).Should().BeEmpty();
    }

    [TestMethod]
    public void DeclareProjectReferencesOnlyInward()
    {
        // Arrange
        var projects = ProjectFiles.LoadAll();

        // Act
        var forbiddenReferences = ProjectReferencesOutside(projects, AllowedProjectReferences());

        // Assert
        ViolationReport.Of(forbiddenReferences).Should().BeEmpty();
    }

    [TestMethod]
    public void KeepDomainFreeOfPackageReferences()
    {
        // Arrange
        var domain = ProjectNamed(ProjectFiles.LoadAll(), NameOf(SolutionAssemblies.Domain));

        // Act
        var packageReferences = domain.PackageReferences;

        // Assert
        ViolationReport.Of(packageReferences).Should().BeEmpty();
    }

    private static LayerReferences[] AllowedProjectReferences()
    {
        var domain = NameOf(SolutionAssemblies.Domain);
        var dataAccess = NameOf(SolutionAssemblies.DataAccess);
        var api = NameOf(SolutionAssemblies.Api);
        var ioC = NameOf(SolutionAssemblies.IoC);

        return
        [
            new(domain, []),
            new(dataAccess, [domain]),
            new(api, [domain]),
            new(ioC, [api, dataAccess, domain])
        ];
    }

    private static List<string> ProjectReferencesOutside(List<ProjectFile> projects, LayerReferences[] allowedReferences)
    {
        var violations = new List<string>();
        foreach (var layer in allowedReferences)
        {
            foreach (var reference in ProjectNamed(projects, layer.Project).ProjectReferences)
            {
                if (!layer.MayReference.Contains(reference))
                {
                    violations.Add($"{layer.Project} references {reference}");
                }
            }
        }

        return violations;
    }

    private static ProjectFile ProjectNamed(List<ProjectFile> projects, string name)
    {
        return projects.Single(project => project.Name == name);
    }

    private static string NameOf(Assembly assembly)
    {
        return assembly.GetName().Name!;
    }

    private static List<string> NamesNotStartingWith(AssemblyName[] references, string allowedPrefix)
    {
        var names = new List<string>();
        foreach (var reference in references)
        {
            if (!reference.Name!.StartsWith(allowedPrefix))
            {
                names.Add(reference.Name);
            }
        }

        return names;
    }

    private static List<string> NamesOf(AssemblyName[] references, params Assembly[] forbiddenAssemblies)
    {
        var names = new List<string>();
        foreach (var reference in references)
        {
            foreach (var forbiddenAssembly in forbiddenAssemblies)
            {
                if (reference.Name == forbiddenAssembly.GetName().Name)
                {
                    names.Add(reference.Name!);
                }
            }
        }

        return names;
    }
}
