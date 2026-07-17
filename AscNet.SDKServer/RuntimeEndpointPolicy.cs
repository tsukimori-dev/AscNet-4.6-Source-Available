namespace AscNet.SDKServer;

public static class RuntimeEndpointPolicy
{
    public static string GetRequiredCdnSetting(string variableName)
    {
        string? value = Environment.GetEnvironmentVariable(variableName);
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"{variableName} must identify an operator-controlled HTTP(S) runtime-data endpoint.");

        string normalized = value.Trim();
        string[] endpoints = normalized.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (endpoints.Length == 0)
            throw new InvalidOperationException($"{variableName} must contain at least one HTTP(S) endpoint.");

        foreach (string endpoint in endpoints)
        {
            if (!Uri.TryCreate(endpoint, UriKind.Absolute, out Uri? uri) || uri.Scheme is not ("http" or "https"))
                throw new InvalidOperationException($"{variableName} contains an invalid HTTP(S) endpoint.");
        }

        return normalized;
    }
}
