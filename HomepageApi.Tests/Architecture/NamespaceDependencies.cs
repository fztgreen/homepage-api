using ArchUnitNET.Domain;
using ArchUnitNET.Domain.Dependencies;

namespace HomepageApi.Tests.Architecture;

public static class NamespaceDependencies
{
    public static List<string> Find(System.Reflection.Assembly[] dependentAssemblies, string[] targetNamespaces, string exemptNamespace)
    {
        var violations = new List<string>();
        foreach (var type in SolutionAssemblies.Architecture.Types)
        {
            if (IsInAny(type, dependentAssemblies) && type.Namespace.FullName != exemptNamespace)
            {
                violations.AddRange(DependenciesOn(type, targetNamespaces));
            }
        }

        return violations;
    }

    private static IEnumerable<string> DependenciesOn(IType type, string[] targetNamespaces)
    {
        foreach (var dependency in type.Dependencies)
        {
            if (TargetsAny(dependency, targetNamespaces))
            {
                yield return $"{type.FullName} depends on {dependency.Target.FullName}";
            }
        }
    }

    private static bool IsInAny(IType type, System.Reflection.Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            if (type.Assembly.FullName == assembly.FullName)
            {
                return true;
            }
        }

        return false;
    }

    private static bool TargetsAny(ITypeDependency dependency, string[] targetNamespaces)
    {
        var dependencyNamespace = dependency.Target.Namespace.FullName;
        foreach (var targetNamespace in targetNamespaces)
        {
            if (dependencyNamespace == targetNamespace || dependencyNamespace.StartsWith($"{targetNamespace}."))
            {
                return true;
            }
        }

        return false;
    }
}
