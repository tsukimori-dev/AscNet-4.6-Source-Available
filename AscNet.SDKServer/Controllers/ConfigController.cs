using AscNet.Common.Util;
using AscNet.SDKServer.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AscNet.SDKServer.Controllers
{
    internal class ConfigController : IRegisterable
    {
        private static readonly Dictionary<string, ServerVersionConfig> versions = new();

        static ConfigController()
        {
            versions = JsonConvert.DeserializeObject<Dictionary<string, ServerVersionConfig>>(File.ReadAllText(JsonSnapshot.ResolvePath("Configs/version_config.json")))!;
        }

        public static void Register(WebApplication app)
        {
            app.MapGet("/prod/client/config/{package}/{version}/standalone/config.tab", HandleConfigRequest);
            app.MapGet("/prod/client/config/{cdnKey}/{package}/{version}/standalone/config.tab", HandleConfigRequest);

            app.MapGet("/prod/client/notice/config/{package}/{version}/LoginNotice.json", HandleLoginNoticeRequest);
            app.MapGet("/prod/client/notice/config/{cdnKey}/{package}/{version}/LoginNotice.json", HandleLoginNoticeRequest);
            app.MapGet("/prod/client/notice/{package}/{version}/standalone/LoginNotice.json", HandleLoginNoticeRequest);
            app.MapGet("/prod/client/notice/{cdnKey}/{package}/{version}/standalone/LoginNotice.json", HandleLoginNoticeRequest);
            app.MapGet("/prod/client/notice/config/{package}/{version}/ScrollTextNotice.json", HandleScrollTextNoticeRequest);
            app.MapGet("/prod/client/notice/config/{cdnKey}/{package}/{version}/ScrollTextNotice.json", HandleScrollTextNoticeRequest);
            app.MapGet("/prod/client/notice/{package}/{version}/standalone/ScrollTextNotice.json", HandleScrollTextNoticeRequest);
            app.MapGet("/prod/client/notice/{cdnKey}/{package}/{version}/standalone/ScrollTextNotice.json", HandleScrollTextNoticeRequest);
            app.MapGet("/prod/client/notice/config/{package}/{version}/ScrollPicNotice.json", HandleScrollPicNoticeRequest);
            app.MapGet("/prod/client/notice/config/{cdnKey}/{package}/{version}/ScrollPicNotice.json", HandleScrollPicNoticeRequest);
            app.MapGet("/prod/client/notice/{package}/{version}/standalone/ScrollPicNotice.json", HandleScrollPicNoticeRequest);
            app.MapGet("/prod/client/notice/{cdnKey}/{package}/{version}/standalone/ScrollPicNotice.json", HandleScrollPicNoticeRequest);
            app.MapGet("/prod/client/notice/config/{package}/{version}/GameNotice.json", HandleGameNoticeRequest);
            app.MapGet("/prod/client/notice/config/{cdnKey}/{package}/{version}/GameNotice.json", HandleGameNoticeRequest);
            app.MapGet("/prod/client/notice/{package}/{version}/standalone/GameNotice.json", HandleGameNoticeRequest);
            app.MapGet("/prod/client/notice/{cdnKey}/{package}/{version}/standalone/GameNotice.json", HandleGameNoticeRequest);
            app.MapGet("/prod/client/notice/config/{package}/{version}/SecondMenuNotice.json", HandleSecondMenuNoticeRequest);
            app.MapGet("/prod/client/notice/config/{cdnKey}/{package}/{version}/SecondMenuNotice.json", HandleSecondMenuNoticeRequest);
            app.MapGet("/prod/client/notice/{package}/{version}/standalone/SecondMenuNotice.json", HandleSecondMenuNoticeRequest);
            app.MapGet("/prod/client/notice/{cdnKey}/{package}/{version}/standalone/SecondMenuNotice.json", HandleSecondMenuNoticeRequest);
            app.MapGet("/prod/client/notice/config/{package}/{version}/PopUpPicNotice.json", HandlePopUpPicNoticeRequest);
            app.MapGet("/prod/client/notice/config/{cdnKey}/{package}/{version}/PopUpPicNotice.json", HandlePopUpPicNoticeRequest);
            app.MapGet("/prod/client/notice/{package}/{version}/standalone/PopUpPicNotice.json", HandlePopUpPicNoticeRequest);
            app.MapGet("/prod/client/notice/{cdnKey}/{package}/{version}/standalone/PopUpPicNotice.json", HandlePopUpPicNoticeRequest);
            app.MapGet("/prod/client/notice/pic/{fileName}", HandleNoticePicRequest);
            app.MapGet("/prod/client/notice/html/{fileName}", HandleNoticeHtmlRequest);


            app.MapPost("/feedback", (HttpContext ctx) =>
            {
                SDKServer.log.Info("1");
                return "1";
            });
        }

        private static string HandleConfigRequest(HttpContext ctx)
        {
            string package = GetRouteValue(ctx, "package");
            string version = GetRouteValue(ctx, "version");
            bool currentClient = IsVersionAtLeast(version, 4, 5, 0);
            string publicHttpOrigin = PublicHttpOrigin(ctx);
            ServerVersionConfig versionConfig = GetVersionConfig(package, version);
            if (versionConfig.Ready == false)
            {
                ctx.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                return versionConfig.BlockReason
                    ?? $"AscNet compatibility for client {version} is not ready.";
            }

            List<RemoteConfig> remoteConfigs = new();
            if (currentClient)
                AddCurrentClientConfig(remoteConfigs, package, version, versionConfig, publicHttpOrigin);
            else
                AddLegacyClientConfig(remoteConfigs, package, version, versionConfig, publicHttpOrigin);

            string serializedObject = TsvTool.SerializeObject(remoteConfigs);
            SDKServer.log.Info(serializedObject);
            return serializedObject;
        }

        private static string HandleLoginNoticeRequest(HttpContext ctx)
        {
            if (TryReadNoticeFixture(ctx, "LoginNotice.json", out string fixtureJson))
                return fixtureJson;

            LoginNotice notice = new()
            {
                BeginTime = 0,
                EndTime = 0,
                HtmlUrl = "/",
                Id = "1",
                ModifyTime = DateTimeOffset.Now.ToUnixTimeSeconds(),
                Title = "NOTICE",
                LoginPlatformList = DefaultLoginPlatforms()
            };

            return SerializeAndLog(notice);
        }

        private static string HandleScrollTextNoticeRequest(HttpContext ctx)
        {
            if (TryReadNoticeFixture(ctx, "ScrollTextNotice.json", out string fixtureJson))
                return fixtureJson;

            ScrollTextNotice notice = new()
            {
                Id = "1",
                ModifyTime = DateTimeOffset.Now.ToUnixTimeSeconds(),
                BeginTime = 0,
                EndTime = 0,
                Content = "[ANNOUNCEMENT] There is no announcement.",
                ScrollInterval = 300,
                ScrollTimes = 15,
                ShowInFight = 1,
                ShowInPhotograph = 1,
                LoginPlatformList = DefaultLoginPlatforms()
            };

            return SerializeAndLog(notice);
        }

        private static string HandleScrollPicNoticeRequest(HttpContext ctx)
        {
            if (TryReadNoticeFixture(ctx, "ScrollPicNotice.json", out string fixtureJson))
                return fixtureJson;

            ScrollPicNotice notice = new()
            {
                Id = "1",
                ModifyTime = DateTimeOffset.Now.ToUnixTimeSeconds(),
                Content =
                [
                    new ScrollPicNotice.NoticeContent()
                    {
                        Id = 0,
                        PicAddr = "0",
                        JumpType = "0",
                        JumpAddr = "0",
                        PicType = "0",
                        Interval = 5,
                        BeginTime = DateTimeOffset.Now.ToUnixTimeSeconds(),
                        EndTime = DateTimeOffset.Now.ToUnixTimeSeconds() + 3600 * 24,
                        AppearanceCondition = Array.Empty<dynamic>(),
                        AppearanceDay = Array.Empty<dynamic>(),
                        AppearanceTime = Array.Empty<dynamic>(),
                        DisappearanceCondition = Array.Empty<dynamic>(),
                    }
                ],
                LoginPlatformList = DefaultLoginPlatforms()
            };

            return SerializeAndLog(notice);
        }

        private static string HandleGameNoticeRequest(HttpContext ctx)
        {
            if (TryReadNoticeFixture(ctx, "GameNotice.json", out string fixtureJson))
                return fixtureJson;

            List<GameNotice> notices = new();
            return SerializeAndLog(notices);
        }

        private static string HandleSecondMenuNoticeRequest(HttpContext ctx)
        {
            if (TryReadNoticeFixture(ctx, "SecondMenuNotice.json", out string fixtureJson))
                return fixtureJson;

            return SerializeAndLog(new
            {
                Id = "ascnet-empty-second-menu",
                ModifyTime = DateTimeOffset.Now.ToUnixTimeSeconds(),
                Content = Array.Empty<object>(),
                LoginPlatformList = DefaultLoginPlatforms()
            });
        }

        private static string HandlePopUpPicNoticeRequest(HttpContext ctx)
        {
            if (TryReadNoticeFixture(ctx, "PopUpPicNotice.json", out string fixtureJson))
                return fixtureJson;

            return SerializeAndLog(new
            {
                Id = "ascnet-empty-popup-pic",
                ModifyTime = DateTimeOffset.Now.ToUnixTimeSeconds(),
                Content = Array.Empty<object>(),
                LoginPlatformList = DefaultLoginPlatforms()
            });
        }

        private static IResult HandleNoticePicRequest(HttpContext ctx)
        {
            string fileName = Path.GetFileName(GetRouteValue(ctx, "fileName"));

            foreach (string versionDirectory in NoticeVersionDirectories())
            {
                string path = Path.Combine(versionDirectory, "client", "notice", "pic", fileName);
                if (File.Exists(path))
                    return Results.File(File.ReadAllBytes(path), "image/png");
            }

            return Results.NotFound();
        }

        private static IResult HandleNoticeHtmlRequest(HttpContext ctx)
        {
            string fileName = Path.GetFileName(GetRouteValue(ctx, "fileName"));
            if (!fileName.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
                return Results.NotFound();

            foreach (string versionDirectory in NoticeVersionDirectories())
            {
                string path = Path.Combine(versionDirectory, "client", "notice", "html", fileName);
                if (File.Exists(path))
                    return Results.File(File.ReadAllBytes(path), "text/html; charset=utf-8");
            }

            if (TryBuildNoticeHtml(fileName, out string html))
                return Results.Text(html, "text/html; charset=utf-8");

            return Results.NotFound();
        }

        private static IEnumerable<string> NoticeVersionDirectories()
        {
            string noticesRoot = JsonSnapshot.ResolveDirectoryPath(Path.Combine("Configs", "Notices"));
            if (!Directory.Exists(noticesRoot))
                return Array.Empty<string>();

            return Directory.GetDirectories(noticesRoot).OrderByDescending(Path.GetFileName);
        }

        private static bool TryGetNoticeVersionDirectory(string version, out string versionDirectory)
        {
            string[] candidates = NoticeVersionDirectories().ToArray();
            foreach (string candidate in candidates)
            {
                if (string.Equals(Path.GetFileName(candidate), version, StringComparison.OrdinalIgnoreCase))
                {
                    versionDirectory = candidate;
                    return true;
                }
            }

            if (Version.TryParse(version, out Version? requestedVersion))
            {
                string? fallback = candidates
                    .Select(candidate => (Path: candidate, Name: Path.GetFileName(candidate)))
                    .Where(candidate => Version.TryParse(candidate.Name, out _))
                    .Select(candidate => (candidate.Path, Version: Version.Parse(candidate.Name)))
                    .Where(candidate => candidate.Version.CompareTo(requestedVersion) <= 0)
                    .OrderByDescending(candidate => candidate.Version)
                    .Select(candidate => candidate.Path)
                    .FirstOrDefault();
                if (fallback is not null)
                {
                    versionDirectory = fallback;
                    return true;
                }
            }

            versionDirectory = string.Empty;
            return false;
        }

        private static bool TryBuildNoticeHtml(string fileName, out string html)
        {
            foreach (string versionDirectory in NoticeVersionDirectories())
            {
                if (TryFindLoginNoticeTitle(versionDirectory, fileName, out string title)
                    || TryFindGameNoticeTitle(versionDirectory, fileName, out title))
                {
                    html = BuildNoticeHtml(title);
                    return true;
                }
            }

            html = string.Empty;
            return false;
        }

        private static bool TryFindLoginNoticeTitle(string versionDirectory, string fileName, out string title)
        {
            string path = Path.Combine(versionDirectory, "LoginNotice.json");
            if (File.Exists(path))
            {
                JObject notice = JObject.Parse(File.ReadAllText(path));
                if (string.Equals(Path.GetFileName(notice.Value<string>("HtmlUrl") ?? string.Empty), fileName, StringComparison.OrdinalIgnoreCase))
                {
                    title = notice.Value<string>("Title") ?? "Notice";
                    return true;
                }
            }

            title = string.Empty;
            return false;
        }

        private static bool TryFindGameNoticeTitle(string versionDirectory, string fileName, out string title)
        {
            string path = Path.Combine(versionDirectory, "GameNotice.json");
            if (File.Exists(path))
            {
                foreach (JObject notice in JArray.Parse(File.ReadAllText(path)).OfType<JObject>())
                {
                    foreach (JObject content in (notice.Value<JArray>("Content") ?? new JArray()).OfType<JObject>())
                    {
                        if (string.Equals(Path.GetFileName(content.Value<string>("Url") ?? string.Empty), fileName, StringComparison.OrdinalIgnoreCase))
                        {
                            title = content.Value<string>("Title") ?? notice.Value<string>("Title") ?? "Notice";
                            return true;
                        }
                    }
                }
            }

            title = string.Empty;
            return false;
        }

        private static string BuildNoticeHtml(string title)
        {
            string encodedTitle = System.Net.WebUtility.HtmlEncode(title);
            return $$"""
                <!doctype html>
                <html>
                <head>
                    <meta charset="utf-8">
                    <meta name="viewport" content="width=device-width, initial-scale=1">
                    <title>{{encodedTitle}}</title>
                    <style>
                        body { margin: 0; padding: 24px; background: #10131a; color: #eef3ff; font-family: sans-serif; }
                        main { max-width: 720px; margin: 0 auto; }
                        h1 { font-size: 24px; line-height: 1.25; margin: 0 0 16px; }
                        p { color: #b9c2d5; font-size: 16px; line-height: 1.5; }
                    </style>
                </head>
                <body>
                    <main>
                        <h1>{{encodedTitle}}</h1>
                        <p>Please refer to the in-game notice list for event details.</p>
                    </main>
                </body>
                </html>
                """;
        }

        private static void AddLegacyClientConfig(List<RemoteConfig> remoteConfigs, string package, string version, ServerVersionConfig versionConfig, string publicHttpOrigin)
        {
            (string primaryCdns, string secondaryCdns, int channel) = GetPackageConfig(package, currentClient: false);

            remoteConfigs.AddConfig("DocumentVersion", versionConfig.DocumentVersion);
            remoteConfigs.AddConfig("LaunchModuleVersion", versionConfig.LaunchModuleVersion);
            remoteConfigs.AddConfig("IndexMd5", versionConfig.IndexMd5);
            remoteConfigs.AddConfig("IndexSha1", versionConfig.IndexSha1);
            remoteConfigs.AddConfig("LaunchIndexSha1", versionConfig.LaunchIndexSha1);
            remoteConfigs.AddConfig("ApplicationVersion", version);
            remoteConfigs.AddConfig("Debug", true);
            remoteConfigs.AddConfig("External", true);
            remoteConfigs.AddConfig("PayCallbackUrl", $"{publicHttpOrigin}/api/XPay/KuroPayResult");
            remoteConfigs.AddConfig("PrimaryCdns", primaryCdns);
            remoteConfigs.AddConfig("SecondaryCdns", secondaryCdns);
            remoteConfigs.AddConfig("Channel", channel);
            remoteConfigs.AddConfig("CdnInvalidTime", 60);
            remoteConfigs.AddConfig("MtpEnabled", false);
            remoteConfigs.AddConfig("MemoryLimit", 2048);
            remoteConfigs.AddConfig("CloseMsgEncrypt", false);
            remoteConfigs.AddConfig("ServerListStr", $"{Common.Common.config.GameServer.RegionName}#{publicHttpOrigin}/api/Login/Login");
            remoteConfigs.AddConfig("AndroidPayCallbackUrl", $"{publicHttpOrigin}/api/XPay/HeroHgAndroidPayResult");
            remoteConfigs.AddConfig("IosPayCallbackUrl", $"{publicHttpOrigin}/api/XPay/HeroHgIosPayResult");
            remoteConfigs.AddConfig("WatermarkEnabled", false);
            remoteConfigs.AddConfig("PicComposition", "empty");
            remoteConfigs.AddConfig("DeepLinkEnabled", true);
            remoteConfigs.AddConfig("DownloadMethod", 1);
            remoteConfigs.AddConfig("PcPayCallbackList", $"{publicHttpOrigin}/api/XPay/KuroPayResult");
            remoteConfigs.AddConfig("WatermarkType", 2);
            remoteConfigs.AddConfig("ChannelServerListStr", $"1#{Common.Common.config.GameServer.RegionName}#{publicHttpOrigin}/api/Login/Login");
            remoteConfigs.AddConfig("IsHideFunc", false);
            remoteConfigs.AddConfig("IsHideFuncAndroid", false);
        }

        private static void AddCurrentClientConfig(List<RemoteConfig> remoteConfigs, string package, string version, ServerVersionConfig versionConfig, string publicHttpOrigin)
        {
            (string primaryCdns, string secondaryCdns, int channel) = GetPackageConfig(package, currentClient: true);

            remoteConfigs.AddConfig("ApplicationVersion", version);
            remoteConfigs.AddConfig("DocumentVersion", versionConfig.DocumentVersion);
            remoteConfigs.AddConfig("Debug", false);
            remoteConfigs.AddConfig("External", true);
            remoteConfigs.AddConfig("Channel", channel);
            remoteConfigs.AddConfig("PayCallbackUrl", $"{publicHttpOrigin}/api/XPay/KuroPayResult");
            remoteConfigs.AddConfig("KuroPayCallbackUrl", $"{publicHttpOrigin}/api/XPay/KuroPayResult");
            remoteConfigs.AddConfig("PrimaryCdns", primaryCdns);
            remoteConfigs.AddConfig("SecondaryCdns", secondaryCdns);
            remoteConfigs.AddConfig("CdnInvalidTime", 600);
            remoteConfigs.AddConfig("MtpEnabled", true);
            remoteConfigs.AddConfig("MemoryLimit", 2048);
            remoteConfigs.AddConfig("CloseMsgEncrypt", false);
            remoteConfigs.AddConfig("ServerListStr", CurrentServerListStr(package, publicHttpOrigin));
            if (!string.IsNullOrWhiteSpace(versionConfig.IndexMd5))
                remoteConfigs.AddConfig("IndexMd5", versionConfig.IndexMd5);
            remoteConfigs.AddConfig("AndroidReturnEnabled", false);
            remoteConfigs.AddConfig("AndroidPayCallbackList", $"{publicHttpOrigin}/api/XPay/HeroHgAndroidPayResult");
            remoteConfigs.AddConfig("AndroidPayCallbackUrl", $"{publicHttpOrigin}/api/XPay/HeroHgAndroidPayResult");
            remoteConfigs.AddConfig("IosPayCallbackUrl", $"{publicHttpOrigin}/api/XPay/HeroHgIosPayResult");
            remoteConfigs.AddConfig("DEEnable", true);
            remoteConfigs.AddConfig("DEFilter", "empty");
            remoteConfigs.AddConfig("IndexSha1", versionConfig.IndexSha1);
            remoteConfigs.AddConfig("WatermarkEnabled", false);
            remoteConfigs.AddConfig("PicComposition", "empty");
            remoteConfigs.AddConfig("IosPayCallbackList", $"{publicHttpOrigin}/api/XPay/HeroHgIosPayResult");
            remoteConfigs.AddConfig("LaunchModuleVersion", versionConfig.LaunchModuleVersion);
            remoteConfigs.AddConfig("LaunchIndexSha1", versionConfig.LaunchIndexSha1);
            remoteConfigs.AddConfig("DeepLinkEnabled", true);
            remoteConfigs.AddConfig("AccountCancellationEnable", false);
            remoteConfigs.AddConfig("DownloadMethod", versionConfig.DownloadMethod ?? 1);
            remoteConfigs.AddConfig("PcPayCallbackList", $"{publicHttpOrigin}/api/XPay/KuroPayResult");
            remoteConfigs.AddConfig("PcPayCallbackUrl", $"{publicHttpOrigin}/api/XPay/KuroPayResult");
            remoteConfigs.AddConfig("ParallelDownload", 1);
            remoteConfigs.AddConfig("ParallelQueueSize", "3-7");
            if (!string.IsNullOrWhiteSpace(versionConfig.ParallelChunkSize))
                remoteConfigs.AddConfig("ParallelChunkSize", versionConfig.ParallelChunkSize);
            if (versionConfig.IndexSize is > 0)
                remoteConfigs.AddConfig("IndexSize", versionConfig.IndexSize.Value);
            if (versionConfig.LaunchIndexSize is > 0)
                remoteConfigs.AddConfig("LaunchIndexSize", versionConfig.LaunchIndexSize.Value);
            remoteConfigs.AddConfig("WatermarkType", 0);
            remoteConfigs.AddConfig("IsPCPayEnable", true);
            remoteConfigs.AddConfig("ChannelServerListStr", CurrentChannelServerListStr(package, publicHttpOrigin));
            remoteConfigs.AddConfig("IsHeXie", false);
            remoteConfigs.AddConfig("IsHideFunc", false);
            remoteConfigs.AddConfig("IsHideFuncAndroid", false);
            remoteConfigs.AddConfig("UsingXTableBehaviorNodeOptimize", true);
            remoteConfigs.AddConfig("IsUsingCDNAuth", false);
            remoteConfigs.AddConfig("AuthSignName", "sign");
            remoteConfigs.AddConfig("AuthTimeOut", 1800);
            remoteConfigs.AddConfig("AuthIsVolKey", "volcdn");
        }

        private static string CurrentServerListStr(string package, string publicHttpOrigin)
        {
            string loginUrl = $"{publicHttpOrigin}/api/Login/Login";
            if (IsMainlandPackage(package))
                return $"Local#{loginUrl}";

            return $"NorthAmerica#{loginUrl}|Europe#{loginUrl}|Asia-Pacific#{loginUrl}";
        }

        private static string CurrentChannelServerListStr(string package, string publicHttpOrigin)
        {
            if (IsMainlandPackage(package))
                return $"19#Local#{publicHttpOrigin}/api/Login/Login";

            return $"default#Globle#{publicHttpOrigin}/api/Login/Login";
        }

        private static (string PrimaryCdns, string SecondaryCdns, int Channel) GetPackageConfig(string package, bool currentClient)
        {
            string primaryCdns = RuntimeEndpointPolicy.GetRequiredCdnSetting("ASCNET_PRIMARY_CDNS");
            string secondaryCdns = RuntimeEndpointPolicy.GetRequiredCdnSetting("ASCNET_SECONDARY_CDNS");
            int channel = package switch
            {
                "com.kurogame.haru.kuro" when currentClient => 5,
                "com.kurogame.haru.kuro" => 2,
                "com.kurogame.pc.punishing.grayraven" or "com.kurogame.punishing.grayraven" when currentClient => 2,
                "com.kurogame.punishing.grayraven.en" or "com.kurogame.gplay.punishing.grayraven.en" when currentClient => 5,
                "com.kurogame.pc.punishing.grayraven.en" when currentClient => 205,
                _ => 1
            };

            return (primaryCdns, secondaryCdns, channel);
        }

        private static bool IsMainlandPackage(string package)
        {
            return package is "com.kurogame.haru.kuro" or "com.kurogame.pc.punishing.grayraven" or "com.kurogame.punishing.grayraven";
        }

        private static string? FirstForwardedValue(Microsoft.Extensions.Primitives.StringValues values)
        {
            if (values.Count < 1 || string.IsNullOrWhiteSpace(values[0]))
                return null;

            return values[0]!.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        }

        private static string PublicHttpOrigin(HttpContext ctx)
        {
            string scheme = FirstForwardedValue(ctx.Request.Headers["X-Forwarded-Proto"]) ?? ctx.Request.Scheme;
            string host = FirstForwardedValue(ctx.Request.Headers["X-Forwarded-Host"]) ?? ctx.Request.Host.Value;
            return $"{scheme}://{host}".TrimEnd('/');
        }


        private static ServerVersionConfig GetVersionConfig(string package, string version)
        {
            ServerVersionConfig versionConfig = GetVersionConfig(version);
            if (!IsMainlandPackage(package) || versionConfig.Mainland is null)
                return versionConfig;

            ServerVersionConfig mainland = versionConfig.Mainland;
            return new()
            {
                DocumentVersion = ValueOrFallback(mainland.DocumentVersion, versionConfig.DocumentVersion),
                LaunchModuleVersion = ValueOrFallback(mainland.LaunchModuleVersion, versionConfig.LaunchModuleVersion),
                IndexMd5 = ValueOrFallback(mainland.IndexMd5, versionConfig.IndexMd5),
                IndexSha1 = ValueOrFallback(mainland.IndexSha1, versionConfig.IndexSha1),
                LaunchIndexSha1 = ValueOrFallback(mainland.LaunchIndexSha1, versionConfig.LaunchIndexSha1),
                IndexSize = mainland.IndexSize ?? versionConfig.IndexSize,
                LaunchIndexSize = mainland.LaunchIndexSize ?? versionConfig.LaunchIndexSize,
                DownloadMethod = mainland.DownloadMethod ?? versionConfig.DownloadMethod,
                ParallelChunkSize = string.IsNullOrWhiteSpace(mainland.ParallelChunkSize)
                    ? versionConfig.ParallelChunkSize
                    : mainland.ParallelChunkSize,
                Ready = mainland.Ready ?? versionConfig.Ready,
                BlockReason = mainland.BlockReason ?? versionConfig.BlockReason
            };
        }

        private static ServerVersionConfig GetVersionConfig(string version)
        {
            if (versions.TryGetValue(version, out ServerVersionConfig? versionConfig))
                return versionConfig;

            if (IsVersionAtLeast(version, 4, 5, 0))
            {
                return new()
                {
                    Ready = false,
                    BlockReason = $"AscNet has no compatibility definition for client {version}."
                };
            }

            ServerVersionConfig? latestConfig = null;
            Version? latestVersion = null;

            foreach (var knownVersion in versions)
            {
                if (!Version.TryParse(knownVersion.Key, out Version? parsedVersion))
                    continue;

                if (latestVersion is null || parsedVersion.CompareTo(latestVersion) > 0)
                {
                    latestVersion = parsedVersion;
                    latestConfig = knownVersion.Value;
                }
            }

            return latestConfig ?? versions.First().Value;
        }

        private static string ValueOrFallback(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

        private static bool IsVersionAtLeast(string version, int major, int minor, int patch)
        {
            return Version.TryParse(version, out Version? parsedVersion)
                && parsedVersion.CompareTo(new Version(major, minor, patch)) >= 0;
        }

        private static string GetRouteValue(HttpContext ctx, string key)
        {
            return (string)ctx.Request.RouteValues[key]!;
        }

        private static bool TryReadNoticeFixture(HttpContext ctx, string fileName, out string fixtureJson)
        {
            string version = GetRouteValue(ctx, "version");
            if (!TryGetNoticeVersionDirectory(version, out string versionDirectory))
            {
                fixtureJson = string.Empty;
                return false;
            }

            string path = Path.Combine(versionDirectory, fileName);
            if (!File.Exists(path))
            {
                fixtureJson = string.Empty;
                return false;
            }

            fixtureJson = File.ReadAllText(path);
            SDKServer.log.Info(fixtureJson);
            return true;
        }

        private static int[] DefaultLoginPlatforms()
        {
            return [0, 1, 2];
        }

        private static string SerializeAndLog(object value)
        {
            string serializedObject = JsonConvert.SerializeObject(value);
            SDKServer.log.Info(serializedObject);
            return serializedObject;
        }
    }
}
