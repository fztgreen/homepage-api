using System.Reflection;

using ArchUnitNET.Loader;

using HomepageApi.Api;
using HomepageApi.DataAccess.Wrappers;
using HomepageApi.Domain.Entities.Feedback;

namespace HomepageApi.Tests.Architecture;

public static class SolutionAssemblies
{
    public static readonly Assembly Domain = typeof(FeedbackSubmission).Assembly;
    public static readonly Assembly DataAccess = typeof(ISqlDatabaseWrapper).Assembly;
    public static readonly Assembly Api = typeof(ApiConfiguration).Assembly;
    public static readonly Assembly IoC = typeof(Program).Assembly;
    public static readonly Assembly Tests = typeof(SolutionAssemblies).Assembly;

    public static readonly Assembly[] Production = [Domain, DataAccess, Api, IoC];
    public static readonly Assembly[] All = [.. Production, Tests];

    public static readonly ArchUnitNET.Domain.Architecture Architecture = new ArchLoader().LoadAssemblies(All).Build();
}
