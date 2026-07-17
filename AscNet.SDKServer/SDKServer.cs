using AscNet.Logging;
using System.Net;

namespace AscNet.SDKServer
{
    public class SDKServer
    {
        public static readonly Logger log = new(typeof(SDKServer), Logging.LogLevel.DEBUG, Logging.LogLevel.DEBUG);

        public static void Main(string[] args)
        {
            log.LogLevelColor[Logging.LogLevel.INFO] = ConsoleColor.Blue;
            var builder = WebApplication.CreateBuilder(args);

            // Disables default logger
            builder.Logging.ClearProviders();

            var app = builder.Build();

            string[] requestedUrls = app.Urls
                .Concat(GetUrls(args))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            app.Urls.Clear();

            if (requestedUrls.Length == 0)
                requestedUrls = ["http://127.0.0.1:26859"];

            foreach (string url in requestedUrls)
            {
                ValidateBindUrl(url);
                app.Urls.Add(url);
            }

            IEnumerable<Type> controllers = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(IRegisterable).IsAssignableFrom(p) && !p.IsInterface)
                .Select(x => x);

            foreach (Type controller in controllers)
            {
                controller.GetMethod(nameof(IRegisterable.Register))!.Invoke(null, new object[] { app });
#if DEBUG
                log.Info($"Registered HTTP controller '{controller.Name}'");
#endif
            }

            app.UseMiddleware<RequestLoggingMiddleware>();

            new Thread(() => app.Run()).Start();
            log.Info($"{nameof(SDKServer)} started in port {string.Join(", ", app.Urls.Select(x => x.Split(':').Last()))}!");
        }

        private static IEnumerable<string> GetUrls(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--urls" && i + 1 < args.Length)
                    return args[i + 1].Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (args[i].StartsWith("--urls=", StringComparison.Ordinal))
                    return args[i]["--urls=".Length..].Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            }

            return [];
        }

        private static void ValidateBindUrl(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? parsed)
                || parsed.Scheme is not ("http" or "https")
                || string.IsNullOrWhiteSpace(parsed.Host))
            {
                throw new InvalidOperationException($"Invalid SDK bind URL: {url}");
            }

            bool loopback = string.Equals(parsed.Host, "localhost", StringComparison.OrdinalIgnoreCase)
                || (IPAddress.TryParse(parsed.Host, out IPAddress? address) && IPAddress.IsLoopback(address));
            if (loopback)
                return;

            bool remoteBindAllowed = string.Equals(
                Environment.GetEnvironmentVariable("ASCNET_ALLOW_REMOTE_BIND"),
                "1",
                StringComparison.Ordinal);
            if (!remoteBindAllowed)
            {
                throw new InvalidOperationException(
                    "Non-loopback SDK binding requires ASCNET_ALLOW_REMOTE_BIND=1.");
            }
        }

        private class RequestLoggingMiddleware
        {
            private readonly RequestDelegate _next;
            private static readonly string[] surpressedRoutes = ["/feedback"];

            public RequestLoggingMiddleware(RequestDelegate next)
            {
                _next = next;
            }

            public async Task Invoke(HttpContext context)
            {
                try
                {
                    await _next(context);
                }
                catch (Exception ex)
                {
#if DEBUG
                    log.Error($"{ex} Request below:");
#else
                    log.Error($"{ex.Message} Request below:");
#endif
                }
                finally
                {
                     log.Info($"{context.Response.StatusCode} {context.Request.Method.ToUpper()} {context.Request.Path}");
                }
            }
        }
    }

    public interface IRegisterable
    {
        public abstract static void Register(WebApplication app);
    }
}
