using AscNet.Common.Database;
using AscNet.Common.MsgPack;
using MessagePack;

namespace AscNet.GameServer.Handlers
{
#pragma warning disable CS8618
    [MessagePackObject(true)]
    public class NotifyNameplateInfo
    {
        public dynamic Nameplate { get; set; }
    }

    [MessagePackObject(true)]
    public class WearNameplateRequest
    {
        public int NameplateId { get; set; }
    }

    [MessagePackObject(true)]
    public class WearNameplateResponse
    {
        public int Code { get; set; }
    }
#pragma warning restore CS8618

    internal static class NameplateModule
    {
        [RequestPacketHandler("WearNameplateRequest")]
        public static void WearNameplateRequestHandler(Session session, Packet.Request packet)
        {
            WearNameplateRequest request = packet.Deserialize<WearNameplateRequest>();
            Normalize(session.player);
            if (request.NameplateId != 0
                && session.player.UnlockNameplates.All(nameplate => nameplate.Id != request.NameplateId))
            {
                session.SendResponse(new WearNameplateResponse { Code = 1 }, packet.Id);
                return;
            }

            session.player.CurrentWearNameplate = request.NameplateId;
            session.player.Save();
            session.SendPush(BuildLoginData(session.player));
            session.SendResponse(new WearNameplateResponse { Code = 0 }, packet.Id);
        }

        internal static bool Grant(Session session, int nameplateId)
        {
            Normalize(session.player);
            if (session.player.UnlockNameplates.Any(nameplate => nameplate.Id == nameplateId))
                return false;

            PlayerNameplateState nameplate = new()
            {
                Id = nameplateId,
                Exp = 0,
                EndTime = 0,
                GetTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            session.player.UnlockNameplates.Add(nameplate);
            session.player.UnlockNameplates = session.player.UnlockNameplates
                .OrderBy(entry => entry.Id)
                .ToList();
            session.SendPush(new NotifyNameplateInfo
            {
                Nameplate = BuildNameplatePayload(nameplate)
            });
            return true;
        }

        internal static NotifyNameplateLoginData BuildLoginData(Player player)
        {
            Normalize(player);
            return new NotifyNameplateLoginData
            {
                CurrentWearNameplate = player.CurrentWearNameplate,
                UnlockNameplates = player.UnlockNameplates
                    .Select(BuildNameplatePayload)
                    .Cast<dynamic>()
                    .ToList()
            };
        }

        private static Dictionary<string, object> BuildNameplatePayload(PlayerNameplateState nameplate)
        {
            return new Dictionary<string, object>
            {
                ["Id"] = nameplate.Id,
                ["Exp"] = nameplate.Exp,
                ["EndTime"] = nameplate.EndTime,
                ["GetTime"] = nameplate.GetTime
            };
        }

        private static void Normalize(Player player)
        {
            player.UnlockNameplates ??= [];
            if (player.CurrentWearNameplate != 0
                && player.UnlockNameplates.All(nameplate => nameplate.Id != player.CurrentWearNameplate))
            {
                player.CurrentWearNameplate = 0;
            }
        }
    }
}
