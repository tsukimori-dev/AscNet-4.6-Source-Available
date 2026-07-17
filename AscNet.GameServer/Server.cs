using System.Net.Sockets;
using System.Net;
using AscNet.Logging;

namespace AscNet.GameServer
{
    public class Server
    {
        public static Logger log;
        public readonly Dictionary<string, Session> Sessions = new();
        private static Server? _instance;
        private readonly TcpListener listener;

        public static Server Instance
        {
            get
            {
                return _instance ??= new Server();
            }
        }

        static Server()
        {
            // TODO: add loglevel based on appsettings
            LogLevel logLevel = LogLevel.DEBUG;
            LogLevel fileLogLevel = LogLevel.DEBUG;
            log = new(typeof(Server), logLevel, fileLogLevel);
        }

        public Server()
        {
            listener = new(ResolveBindAddress(), Common.Common.config.GameServer.Port);
        }

        private static IPAddress ResolveBindAddress()
        {
            string? configuredAddress = Environment.GetEnvironmentVariable("ASCNET_GAME_BIND_ADDRESS");
            if (string.IsNullOrWhiteSpace(configuredAddress))
                return IPAddress.Loopback;

            if (!IPAddress.TryParse(configuredAddress, out IPAddress? address))
                throw new InvalidOperationException("ASCNET_GAME_BIND_ADDRESS must be a valid IP address.");

            bool remoteBindAllowed = string.Equals(
                Environment.GetEnvironmentVariable("ASCNET_ALLOW_REMOTE_BIND"),
                "1",
                StringComparison.Ordinal);
            if (!IPAddress.IsLoopback(address) && !remoteBindAllowed)
            {
                throw new InvalidOperationException(
                    "Non-loopback game-server binding requires ASCNET_ALLOW_REMOTE_BIND=1.");
            }

            return address;
        }

        public void Start()
        {
            while (true)
            {
                try
                {
                    listener.Start();
                    log.Info($"{nameof(GameServer)} started and listening on port {Common.Common.config.GameServer.Port}");

                    while (true)
                    {
                        TcpClient tcpClient = listener.AcceptTcpClient();
                        string id = tcpClient.Client.RemoteEndPoint!.ToString()!;

                        log.Warn($"{id} connected");
                        Sessions.Add(id, new Session(id, tcpClient));
                    }
                }
                catch (Exception ex)
                {
                    log.Error("TCP listener error: " + ex.Message);
                    log.Info("Waiting 3 seconds before restarting...");
                    Thread.Sleep(3000);
                }
            }
        }

        public Session? SessionFromUID(long uid)
        {
            return Sessions.Select(x => x.Value).FirstOrDefault(x => x.player.PlayerData.Id == uid);
        }
    }
}
