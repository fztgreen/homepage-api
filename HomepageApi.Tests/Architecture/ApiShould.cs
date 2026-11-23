using System.Reflection;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc;

namespace HomepageApi.Tests.Architecture;

[TestClass]
public class ApiShould
{
    private const string ModelsNamespace = "HomepageApi.Api.Models";

    [TestMethod]
    public void ExposeNoDomainTypesFromModels()
    {
        // Arrange
        var models = ApiModels();

        // Act
        var domainProperties = DomainTypedPropertiesOf(models);

        // Assert
        ViolationReport.Of(domainProperties).Should().BeEmpty();
    }

    [TestMethod]
    public void ExposeNoDomainTypesFromControllerActions()
    {
        // Arrange
        var actions = ControllerActions();

        // Act
        var domainTypedActions = DomainTypedSignaturesOf(actions);

        // Assert
        ViolationReport.Of(domainTypedActions).Should().BeEmpty();
    }

    private static List<Type> ApiModels()
    {
        var models = new List<Type>();
        foreach (var type in SolutionAssemblies.Api.GetTypes())
        {
            if (type.IsPublic && type.Namespace is not null && type.Namespace.StartsWith(ModelsNamespace))
            {
                models.Add(type);
            }
        }

        return models;
    }

    private static List<MethodInfo> ControllerActions()
    {
        var actions = new List<MethodInfo>();
        foreach (var type in SolutionAssemblies.Api.GetTypes())
        {
            if (typeof(ControllerBase).IsAssignableFrom(type))
            {
                actions.AddRange(type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            }
        }

        return actions;
    }

    private static List<string> DomainTypedPropertiesOf(List<Type> models)
    {
        var violations = new List<string>();
        foreach (var model in models)
        {
            foreach (var property in model.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (ReferencesDomain(property.PropertyType))
                {
                    violations.Add($"{model.FullName}.{property.Name} is {property.PropertyType.Name}");
                }
            }
        }

        return violations;
    }

    private static List<string> DomainTypedSignaturesOf(List<MethodInfo> actions)
    {
        var violations = new List<string>();
        foreach (var action in actions)
        {
            foreach (var type in SignatureTypesOf(action))
            {
                if (ReferencesDomain(type))
                {
                    violations.Add($"{action.DeclaringType!.Name}.{action.Name} exposes {type.Name}");
                }
            }
        }

        return violations;
    }

    private static IEnumerable<Type> SignatureTypesOf(MethodInfo action)
    {
        yield return action.ReturnType;
        foreach (var parameter in action.GetParameters())
        {
            yield return parameter.ParameterType;
        }

        foreach (var responseType in action.GetCustomAttributes<ProducesResponseTypeAttribute>())
        {
            if (responseType.Type is not null)
            {
                yield return responseType.Type;
            }
        }
    }

    private static bool ReferencesDomain(Type type)
    {
        if (type.Assembly == SolutionAssemblies.Domain)
        {
            return true;
        }

        if (type.HasElementType && ReferencesDomain(type.GetElementType()!))
        {
            return true;
        }

        foreach (var typeArgument in type.GenericTypeArguments)
        {
            if (ReferencesDomain(typeArgument))
            {
                return true;
            }
        }

        return false;
    }
}
