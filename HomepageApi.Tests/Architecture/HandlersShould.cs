using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using FluentAssertions;

namespace HomepageApi.Tests.Architecture;

[TestClass]
public class HandlersShould
{
    private const string CommandsNamespace = "HomepageApi.Domain.Commands.";
    private const string QueriesNamespace = "HomepageApi.Domain.Queries.";
    private const string AsyncSuffix = "Async";

    private static readonly string[] _genericMethodNames = ["ExecuteAsync", "HandleAsync", "RunAsync", "ProcessAsync"];

    [TestMethod]
    public void ImplementTheirOwnInterface()
    {
        // Arrange
        var handlers = HandlerClasses();

        // Act
        var handlersWithoutOwnInterface = WithoutOwnInterface(handlers);

        // Assert
        ViolationReport.Of(handlersWithoutOwnInterface).Should().BeEmpty();
    }

    [TestMethod]
    public void ExposeExactlyOneMethod()
    {
        // Arrange
        var handlerInterfaces = HandlerInterfaces();

        // Act
        var interfacesWithOtherMethodCounts = WithMethodCountOtherThanOne(handlerInterfaces);

        // Assert
        ViolationReport.Of(interfacesWithOtherMethodCounts).Should().BeEmpty();
    }

    [TestMethod]
    public void ReturnTaskFromCommands()
    {
        // Arrange
        var commandMethods = HandlerMethodsIn(CommandsNamespace);

        // Act
        var methodsNotReturningTask = NotReturning(commandMethods, typeof(Task));

        // Assert
        ViolationReport.Of(methodsNotReturningTask).Should().BeEmpty();
    }

    [TestMethod]
    public void ReturnTaskOfResultFromQueries()
    {
        // Arrange
        var queryMethods = HandlerMethodsIn(QueriesNamespace);

        // Act
        var methodsNotReturningTaskOfResult = NotReturning(queryMethods, typeof(Task<>));

        // Assert
        ViolationReport.Of(methodsNotReturningTaskOfResult).Should().BeEmpty();
    }

    [TestMethod]
    public void NameMethodsToCompleteTheHandlerAsASentence()
    {
        // Arrange
        var handlerMethods = HandlerMethods();

        // Act
        var badlyNamedMethods = BadlyNamed(handlerMethods);

        // Assert
        ViolationReport.Of(badlyNamedMethods).Should().BeEmpty();
    }

    private static List<Type> HandlerTypes()
    {
        var handlerTypes = new List<Type>();
        foreach (var type in SolutionAssemblies.Domain.GetTypes())
        {
            if (IsHandlerNamespace(type.Namespace) && !type.IsNested && !type.IsDefined(typeof(CompilerGeneratedAttribute)))
            {
                handlerTypes.Add(type);
            }
        }

        return handlerTypes;
    }

    private static List<Type> HandlerClasses()
    {
        var classes = new List<Type>();
        foreach (var type in HandlerTypes())
        {
            if (type.IsClass)
            {
                classes.Add(type);
            }
        }

        return classes;
    }

    private static List<Type> HandlerInterfaces()
    {
        var interfaces = new List<Type>();
        foreach (var type in HandlerTypes())
        {
            if (type.IsInterface)
            {
                interfaces.Add(type);
            }
        }

        return interfaces;
    }

    private static List<MethodInfo> HandlerMethods()
    {
        var methods = new List<MethodInfo>();
        foreach (var handlerInterface in HandlerInterfaces())
        {
            methods.AddRange(handlerInterface.GetMethods());
        }

        return methods;
    }

    private static List<MethodInfo> HandlerMethodsIn(string handlerNamespace)
    {
        var methods = new List<MethodInfo>();
        foreach (var method in HandlerMethods())
        {
            if (method.DeclaringType!.Namespace!.StartsWith(handlerNamespace))
            {
                methods.Add(method);
            }
        }

        return methods;
    }

    private static List<string> WithoutOwnInterface(List<Type> handlers)
    {
        var violations = new List<string>();
        foreach (var handler in handlers)
        {
            var ownInterface = handler.GetInterface($"I{handler.Name}");
            if (ownInterface is null || ownInterface.Namespace != handler.Namespace)
            {
                violations.Add(handler.FullName!);
            }
        }

        return violations;
    }

    private static List<string> WithMethodCountOtherThanOne(List<Type> handlerInterfaces)
    {
        var violations = new List<string>();
        foreach (var handlerInterface in handlerInterfaces)
        {
            var methodCount = handlerInterface.GetMethods().Length;
            if (methodCount != 1)
            {
                violations.Add($"{handlerInterface.FullName} has {methodCount} methods");
            }
        }

        return violations;
    }

    private static List<string> NotReturning(List<MethodInfo> methods, Type expectedReturnType)
    {
        var violations = new List<string>();
        foreach (var method in methods)
        {
            if (ReturnTypeDefinition(method) != expectedReturnType)
            {
                violations.Add($"{method.DeclaringType!.FullName}.{method.Name} returns {method.ReturnType.Name}");
            }
        }

        return violations;
    }

    private static List<string> BadlyNamed(List<MethodInfo> methods)
    {
        var violations = new List<string>();
        foreach (var method in methods)
        {
            var problem = NamingProblem(method);
            if (problem is not null)
            {
                violations.Add($"{method.DeclaringType!.FullName}.{method.Name}: {problem}");
            }
        }

        return violations;
    }

    private static string? NamingProblem(MethodInfo method)
    {
        if (!method.Name.EndsWith(AsyncSuffix))
        {
            return $"must end with {AsyncSuffix}";
        }

        if (_genericMethodNames.Contains(method.Name))
        {
            return "is generic; name the word that completes the handler as a sentence";
        }

        return RepeatedWord(method);
    }

    private static string? RepeatedWord(MethodInfo method)
    {
        var methodName = method.Name[..^AsyncSuffix.Length];
        foreach (var word in NounWordsOf(method.DeclaringType!))
        {
            if (methodName.Contains(Singular(word)))
            {
                return $"repeats '{word}' from the handler name";
            }
        }

        foreach (var parameter in method.GetParameters())
        {
            if (methodName.Contains(parameter.Name!, StringComparison.OrdinalIgnoreCase))
            {
                return $"repeats the parameter name '{parameter.Name}'";
            }
        }

        return null;
    }

    private static List<string> NounWordsOf(Type handlerInterface)
    {
        var handlerName = handlerInterface.Name[1..];
        var words = new List<string>();
        foreach (Match word in Regex.Matches(handlerName, "[A-Z][a-z0-9]*"))
        {
            words.Add(word.Value);
        }

        words.RemoveAt(0);
        return words;
    }

    private static string Singular(string word)
    {
        return word.EndsWith('s') ? word[..^1] : word;
    }

    private static Type ReturnTypeDefinition(MethodInfo method)
    {
        return method.ReturnType.IsGenericType ? method.ReturnType.GetGenericTypeDefinition() : method.ReturnType;
    }

    private static bool IsHandlerNamespace(string? typeNamespace)
    {
        return typeNamespace is not null
            && (typeNamespace.StartsWith(CommandsNamespace) || typeNamespace.StartsWith(QueriesNamespace));
    }
}
