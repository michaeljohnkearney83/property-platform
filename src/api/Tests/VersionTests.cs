using System.Text.RegularExpressions;
using PropertyPlatform.Host;

namespace PropertyPlatform.Tests;

public partial class VersionTests
{
    [Fact]
    public void Version_is_semver()
    {
        Assert.Matches(SemVer(), AppVersion.Version);
    }

    [Fact]
    public void Version_matches_version_txt()
    {
        // version.txt (release-please) and <Version> in Directory.Build.props must never drift apart.
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "version.txt")))
        {
            dir = dir.Parent;
        }

        Assert.NotNull(dir);
        Assert.Equal(File.ReadAllText(Path.Combine(dir.FullName, "version.txt")).Trim(), AppVersion.Version);
    }

    [GeneratedRegex(@"^\d+\.\d+\.\d+(-[0-9A-Za-z.-]+)?$")]
    private static partial Regex SemVer();
}
