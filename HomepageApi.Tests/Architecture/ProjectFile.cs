namespace HomepageApi.Tests.Architecture;

public record ProjectFile(string Name, List<string> ProjectReferences, List<string> PackageReferences);
