using System.Reflection;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HomepageApi.Tests.Architecture;

[TestClass]
public class CodeShould
{
    private const string AnonymousTypePrefix = "<>f__AnonymousType";

    private const BindingFlags EveryDeclaredMethod =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

    private static readonly string[] _bannedNamespaces = ["Newtonsoft.Json", "Microsoft.EntityFrameworkCore"];

    [TestMethod]
    public void NotUseBannedLibraries()
    {
        // Arrange
        var noExemptNamespace = string.Empty;

        // Act
        var violations = NamespaceDependencies.Find(SolutionAssemblies.All, _bannedNamespaces, noExemptNamespace);

        // Assert
        ViolationReport.Of(violations).Should().BeEmpty();
    }

    [TestMethod]
    public void NotReferenceBannedPackages()
    {
        // Arrange
        var projects = ProjectFiles.LoadAll();

        // Act
        var violations = BannedPackageReferencesIn(projects);

        // Assert
        ViolationReport.Of(violations).Should().BeEmpty();
    }

    [TestMethod]
    public void NotUseAnonymousTypes()
    {
        // Arrange
        var assemblies = SolutionAssemblies.All;

        // Act
        var anonymousTypes = AnonymousTypesIn(assemblies);

        // Assert
        ViolationReport.Of(anonymousTypes).Should().BeEmpty();
    }

    [TestMethod]
    public void KeepLambdasToOneLineExpressions()
    {
        // Arrange
        var sourceFiles = SourceFiles.ParseAll();

        // Act
        var longLambdas = LongLambdasIn(sourceFiles);

        // Assert
        ViolationReport.Of(longLambdas).Should().BeEmpty();
    }

    [TestMethod]
    public void NameAsyncMethodsWithAsyncSuffix()
    {
        // Arrange
        var assemblies = SolutionAssemblies.Production;

        // Act
        var methodsMissingSuffix = AsyncMethodsWithoutSuffixIn(assemblies);

        // Assert
        ViolationReport.Of(methodsMissingSuffix).Should().BeEmpty();
    }

    private static List<string> BannedPackageReferencesIn(List<ProjectFile> projects)
    {
        var violations = new List<string>();
        foreach (var project in projects)
        {
            foreach (var package in project.PackageReferences)
            {
                if (IsBanned(package))
                {
                    violations.Add($"{project.Name} references {package}");
                }
            }
        }

        return violations;
    }

    private static bool IsBanned(string package)
    {
        return _bannedNamespaces.Any(banned => package == banned || package.StartsWith($"{banned}."));
    }

    private static List<string> AsyncMethodsWithoutSuffixIn(Assembly[] assemblies)
    {
        var methods = new List<string>();
        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (!IsCompilerGenerated(type.Name) && !IsController(type))
                {
                    methods.AddRange(AsyncMethodsWithoutSuffixOn(type));
                }
            }
        }

        return methods;
    }

    private static IEnumerable<string> AsyncMethodsWithoutSuffixOn(Type type)
    {
        foreach (var method in type.GetMethods(EveryDeclaredMethod))
        {
            if (IsAwaitable(method.ReturnType) && !method.Name.EndsWith("Async") && !IsCompilerGenerated(method.Name))
            {
                yield return $"{type.FullName}.{method.Name}";
            }
        }
    }

    private static bool IsAwaitable(Type returnType)
    {
        var definition = returnType.IsGenericType ? returnType.GetGenericTypeDefinition() : returnType;
        return definition == typeof(Task) || definition == typeof(Task<>) || definition == typeof(ValueTask) || definition == typeof(ValueTask<>);
    }

    private static bool IsController(Type type)
    {
        return typeof(ControllerBase).IsAssignableFrom(type);
    }

    private static bool IsCompilerGenerated(string name)
    {
        return name.StartsWith('<');
    }

    private static List<string> AnonymousTypesIn(Assembly[] assemblies)
    {
        var anonymousTypes = new List<string>();
        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.Name.StartsWith(AnonymousTypePrefix))
                {
                    anonymousTypes.Add($"{assembly.GetName().Name}: {type.Name}");
                }
            }
        }

        return anonymousTypes;
    }

    private static List<string> LongLambdasIn(IEnumerable<SyntaxTree> sourceFiles)
    {
        var longLambdas = new List<string>();
        foreach (var sourceFile in sourceFiles)
        {
            foreach (var anonymousFunction in sourceFile.GetRoot().DescendantNodes().OfType<AnonymousFunctionExpressionSyntax>())
            {
                var problem = LengthProblem(anonymousFunction);
                if (problem is not null)
                {
                    longLambdas.Add($"{Location(anonymousFunction)}: {problem}");
                }
            }
        }

        return longLambdas;
    }

    private static string? LengthProblem(AnonymousFunctionExpressionSyntax anonymousFunction)
    {
        if (anonymousFunction is AnonymousMethodExpressionSyntax)
        {
            return "delegate block; use a short lambda or a named method";
        }

        if (anonymousFunction.Block is not null)
        {
            return "lambda has a block body; extract it into a named method";
        }

        var lineSpan = anonymousFunction.GetLocation().GetLineSpan();
        if (lineSpan.StartLinePosition.Line != lineSpan.EndLinePosition.Line)
        {
            return "lambda spans multiple lines; extract it into a named method";
        }

        return null;
    }

    private static string Location(SyntaxNode node)
    {
        var lineSpan = node.GetLocation().GetLineSpan();
        return $"{lineSpan.Path}({lineSpan.StartLinePosition.Line + 1})";
    }
}
