namespace AscNet.Common;

public static class ClientCompatibility
{
    public const string SupportedApplicationVersion = "4.6.0";

    public static string ApplicationVersion =>
        Normalize(Common.config.Client.ApplicationVersion);

    public static string StudyCompatibilityVersion =>
        Normalize(Common.config.Client.StudyCompatibilityVersion);

    public static string VersionedTableDirectoryName => $"table-{ApplicationVersion}";

    public static bool IsConfiguredApplicationVersion(string? version)
    {
        return string.Equals(version, ApplicationVersion, StringComparison.Ordinal);
    }

    private static string Normalize(string? version)
    {
        return string.IsNullOrWhiteSpace(version)
            ? SupportedApplicationVersion
            : version.Trim();
    }
}
