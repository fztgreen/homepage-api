using System.Xml.Linq;

namespace HomepageApi.Tests.Architecture;

public static class ProjectFiles
{
    public static List<ProjectFile> LoadAll()
    {
        var repositoryRoot = SourceFiles.RepositoryRoot();
        var solution = XDocument.Load(Path.Combine(repositoryRoot, SourceFiles.SolutionFileName));

        var projects = new List<ProjectFile>();
        foreach (var project in solution.Descendants("Project"))
        {
            projects.Add(Load(Path.Combine(repositoryRoot, project.Attribute("Path")!.Value)));
        }

        return projects;
    }

    private static ProjectFile Load(string path)
    {
        var project = XDocument.Load(path);
        return new ProjectFile(ProjectNameOf(path), ProjectReferencesIn(project), PackageReferencesIn(project));
    }

    private static List<string> ProjectReferencesIn(XDocument project)
    {
        var names = new List<string>();
        foreach (var include in IncludesOf(project, "ProjectReference"))
        {
            names.Add(ProjectNameOf(include));
        }

        return names;
    }

    private static List<string> PackageReferencesIn(XDocument project)
    {
        return [.. IncludesOf(project, "PackageReference")];
    }

    private static IEnumerable<string> IncludesOf(XDocument project, string itemType)
    {
        foreach (var item in project.Descendants(itemType))
        {
            yield return item.Attribute("Include")!.Value;
        }
    }

    private static string ProjectNameOf(string path)
    {
        return Path.GetFileNameWithoutExtension(path.Replace('\\', '/'));
    }
}
