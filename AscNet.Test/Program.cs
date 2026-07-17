using System.Net;
using System.Reflection;
using AscNet.Common;
using AscNet.Common.Database;
using AscNet.Common.MsgPack;
using AscNet.GameServer;
using AscNet.GameServer.Handlers;

namespace AscNet.Test;

internal static class Program
{
    private static int Main()
    {
        try
        {
            ValidateVersionBoundary();
            ValidatePersistedCapacityArchive();
            ValidateLifeTreeResponseContract();
            ValidateLoopbackBindingGate();
            ValidateSdkLoopbackBindingGate();
            ValidateExternalCdnGate();
            Console.WriteLine("AscNet 4.6 sanitized-source checks passed.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static void ValidateVersionBoundary()
    {
        AssertEqual("4.6.0", ClientCompatibility.SupportedApplicationVersion, "supported client version");
        if (typeof(ClientCompatibility).GetField("StableApplicationVersion") is not null)
            throw new InvalidDataException("The private 4.6 snapshot must not restore a 4.5 compatibility constant.");
    }

    private static void ValidatePersistedCapacityArchive()
    {
        PropertyInfo property = typeof(Character).GetProperty(nameof(Character.ArchivedEquips))
            ?? throw new MissingMemberException(typeof(Character).FullName, nameof(Character.ArchivedEquips));

        if (property.PropertyType != typeof(List<EquipData>))
            throw new InvalidDataException("ArchivedEquips must preserve complete EquipData records.");
    }

    private static void ValidateLifeTreeResponseContract()
    {
        Type response = typeof(LifeTreeFinishProcessResponse);
        foreach (string propertyName in new[] { "Code", "IsFinishGuide", "IsFinishLifeTreePv", "FinishedChapters" })
        {
            if (response.GetProperty(propertyName) is null)
                throw new MissingMemberException(response.FullName, propertyName);
        }
    }

    private static void ValidateLoopbackBindingGate()
    {
        MethodInfo resolveBindAddress = typeof(Server).GetMethod(
            "ResolveBindAddress",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new MissingMethodException(typeof(Server).FullName, "ResolveBindAddress");

        string? previousAddress = Environment.GetEnvironmentVariable("ASCNET_GAME_BIND_ADDRESS");
        string? previousAllow = Environment.GetEnvironmentVariable("ASCNET_ALLOW_REMOTE_BIND");
        try
        {
            Environment.SetEnvironmentVariable("ASCNET_GAME_BIND_ADDRESS", null);
            Environment.SetEnvironmentVariable("ASCNET_ALLOW_REMOTE_BIND", null);
            AssertEqual(IPAddress.Loopback, InvokeAddress(resolveBindAddress), "default bind address");

            Environment.SetEnvironmentVariable("ASCNET_GAME_BIND_ADDRESS", "0.0.0.0");
            try
            {
                _ = InvokeAddress(resolveBindAddress);
                throw new InvalidDataException("A non-loopback bind was accepted without explicit authorization.");
            }
            catch (TargetInvocationException ex) when (ex.InnerException is InvalidOperationException)
            {
            }

            Environment.SetEnvironmentVariable("ASCNET_ALLOW_REMOTE_BIND", "1");
            AssertEqual(IPAddress.Any, InvokeAddress(resolveBindAddress), "authorized remote bind address");
        }
        finally
        {
            Environment.SetEnvironmentVariable("ASCNET_GAME_BIND_ADDRESS", previousAddress);
            Environment.SetEnvironmentVariable("ASCNET_ALLOW_REMOTE_BIND", previousAllow);
        }
    }

    private static void ValidateSdkLoopbackBindingGate()
    {
        MethodInfo validateBindUrl = typeof(AscNet.SDKServer.SDKServer).GetMethod(
            "ValidateBindUrl",
            BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new MissingMethodException(typeof(AscNet.SDKServer.SDKServer).FullName, "ValidateBindUrl");

        string? previousAllow = Environment.GetEnvironmentVariable("ASCNET_ALLOW_REMOTE_BIND");
        try
        {
            Environment.SetEnvironmentVariable("ASCNET_ALLOW_REMOTE_BIND", null);
            validateBindUrl.Invoke(null, ["http://127.0.0.1:26859"]);

            try
            {
                validateBindUrl.Invoke(null, ["http://0.0.0.0:26859"]);
                throw new InvalidDataException("A non-loopback SDK bind was accepted without authorization.");
            }
            catch (TargetInvocationException ex) when (ex.InnerException is InvalidOperationException)
            {
            }

            Environment.SetEnvironmentVariable("ASCNET_ALLOW_REMOTE_BIND", "1");
            validateBindUrl.Invoke(null, ["http://0.0.0.0:26859"]);
        }
        finally
        {
            Environment.SetEnvironmentVariable("ASCNET_ALLOW_REMOTE_BIND", previousAllow);
        }
    }

    private static IPAddress InvokeAddress(MethodInfo method)
    {
        return (IPAddress)(method.Invoke(null, null)
            ?? throw new InvalidDataException("ResolveBindAddress returned null."));
    }

    private static void ValidateExternalCdnGate()
    {
        string? previousPrimary = Environment.GetEnvironmentVariable("ASCNET_PRIMARY_CDNS");
        string? previousSecondary = Environment.GetEnvironmentVariable("ASCNET_SECONDARY_CDNS");
        try
        {
            Environment.SetEnvironmentVariable("ASCNET_PRIMARY_CDNS", null);
            Environment.SetEnvironmentVariable("ASCNET_SECONDARY_CDNS", null);
            try
            {
                _ = AscNet.SDKServer.RuntimeEndpointPolicy.GetRequiredCdnSetting("ASCNET_PRIMARY_CDNS");
                throw new InvalidDataException("Missing operator CDN settings were accepted.");
            }
            catch (InvalidOperationException)
            {
            }

            Environment.SetEnvironmentVariable("ASCNET_PRIMARY_CDNS", "http://127.0.0.1:28080/runtime");
            Environment.SetEnvironmentVariable("ASCNET_SECONDARY_CDNS", "http://localhost:28081/runtime");
            AssertEqual(
                "http://127.0.0.1:28080/runtime",
                AscNet.SDKServer.RuntimeEndpointPolicy.GetRequiredCdnSetting("ASCNET_PRIMARY_CDNS"),
                "primary operator CDN");
            AssertEqual(
                "http://localhost:28081/runtime",
                AscNet.SDKServer.RuntimeEndpointPolicy.GetRequiredCdnSetting("ASCNET_SECONDARY_CDNS"),
                "secondary operator CDN");
        }
        finally
        {
            Environment.SetEnvironmentVariable("ASCNET_PRIMARY_CDNS", previousPrimary);
            Environment.SetEnvironmentVariable("ASCNET_SECONDARY_CDNS", previousSecondary);
        }
    }

    private static void AssertEqual<T>(T expected, T actual, string label)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            throw new InvalidDataException($"{label}: expected {expected}, got {actual}.");
    }
}
