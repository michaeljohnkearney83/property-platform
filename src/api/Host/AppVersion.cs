using System.Reflection;

namespace PropertyPlatform.Host;

/// <summary>
/// The running build's version. <see cref="Version"/> comes from &lt;Version&gt; in Directory.Build.props
/// (managed by release-please); <see cref="Commit"/> is the git SHA passed to the Docker build
/// as SOURCE_REVISION (".NET appends it to the informational version as "+sha").
/// </summary>
public static class AppVersion
{
    private static readonly string Informational =
        typeof(AppVersion).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0";

    public static string Version { get; } = Informational.Split('+')[0];

    public static string? Commit { get; } = Informational.Contains('+', StringComparison.Ordinal)
        ? Informational.Split('+')[1][..Math.Min(7, Informational.Split('+')[1].Length)]
        : null;

    public static string Display => Commit is null ? Version : $"{Version}+{Commit}";
}

public sealed record VersionInfo(string Version, string? Commit, string Environment);
