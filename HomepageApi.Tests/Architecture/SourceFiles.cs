using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace HomepageApi.Tests.Architecture;

public static class SourceFiles
{
    public const string SolutionFileName = "HomepageApi.slnx";

    private static readonly CSharpParseOptions _parseOptions = new(LanguageVersion.Preview);

    private static readonly string[] _generatedFolders =
    [
        $"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}",
        $"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"
    ];

    public static IEnumerable<SyntaxTree> ParseAll()
    {
        foreach (var path in Directory.EnumerateFiles(RepositoryRoot(), "*.cs", SearchOption.AllDirectories))
        {
            if (!IsGenerated(path))
            {
                yield return CSharpSyntaxTree.ParseText(File.ReadAllText(path), _parseOptions, path);
            }
        }
    }

    public static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (!File.Exists(Path.Combine(directory.FullName, SolutionFileName)))
        {
            directory = directory.Parent ?? throw new DirectoryNotFoundException($"No {SolutionFileName} above {AppContext.BaseDirectory}.");
        }

        return directory.FullName;
    }

    private static bool IsGenerated(string path)
    {
        foreach (var generatedFolder in _generatedFolders)
        {
            if (path.Contains(generatedFolder))
            {
                return true;
            }
        }

        return false;
    }
}
