using MessagePack;

namespace AscNet.GameServer.Handlers
{

    #region MsgPackScheme
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [MessagePackObject(true)]
    public class GuildDormRoomChannelDataResponse
    {
        public int Code;
        public List<object> ChannelDatas = new();
    }

    [MessagePackObject(true)]
    public class GuildDormPreEnterResponse
    {
        public int Code;
        public object? ConnectData;
    }

    [MessagePackObject(true)]
    public class GuildDormSetRoomBgmIdsResponse
    {
        public int Code;
    }

    [MessagePackObject(true)]
    public class GuildDormGetDailyInteractRewardResponse
    {
        public int Code;
        public List<object> RewardGoodsList = new();
    }

    [MessagePackObject(true)]
    public class GuildDormGetOneTimeInteractRewardResponse
    {
        public int Code;
        public List<object> RewardGoodsList = new();
    }

    [MessagePackObject(true)]
    public class GuildDormRecordInteractResponse
    {
        public int Code;
    }

    [MessagePackObject(true)]
    public class GuildDormCallRandomBoxResponse
    {
        public int Code;
        public object? RandomBox;
    }

    [MessagePackObject(true)]
    public class GuildDormSetRoomThemeResponse
    {
        public int Code;
        public long NextSetRoomThemeTime;
    }

    [MessagePackObject(true)]
    public class GuildDormKcpConfirmResponse
    {
        public int Code;
    }

    [MessagePackObject(true)]
    public class GuildDormEnterRoomResponse
    {
        public int Code;
        public int Port;
        public uint Conv;
        public object? RoomData;
    }

    [MessagePackObject(true)]
    public class GuildDormExitRoomResponse
    {
        public int Code;
    }

    [MessagePackObject(true)]
    public class GuildDormHeartbeatResponse
    {
        public int Code;
    }

    [MessagePackObject(true)]
    public class GuildDormChangeCharacterResponse
    {
        public int Code;
    }

    [MessagePackObject(true)]
    public class GuildDormPlayNoteResponse
    {
        public int Code;
    }

    [MessagePackObject(true)]
    public class GuildDormFurnitureInteractResponse
    {
        public int Code;
    }

    [MessagePackObject(true)]
    public class GuildDormNpcInteractResponse
    {
        public int Code;
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    #endregion

    internal class GuildDormModule
    {
        private const int NotAvailableCode = 1023;

        [RequestPacketHandler("GuildDormRoomChannelDataRequest")]
        public static void GuildDormRoomChannelDataRequestHandler(Session session, Packet.Request packet)
        {
            session.SendResponse(new GuildDormRoomChannelDataResponse(), packet.Id);
        }

        [RequestPacketHandler("GuildDormPreEnterRequest")]
        public static void GuildDormPreEnterRequestHandler(Session session, Packet.Request packet)
        {
            session.SendResponse(new GuildDormPreEnterResponse() { Code = NotAvailableCode }, packet.Id);
        }

        [RequestPacketHandler("GuildDormSetRoomBgmIdsRequest")]
        public static void GuildDormSetRoomBgmIdsRequestHandler(Session session, Packet.Request packet)
        {
            session.SendResponse(new GuildDormSetRoomBgmIdsResponse(), packet.Id);
        }

        [RequestPacketHandler("GuildDormGetDailyInteractRewardRequest")]
        public static void GuildDormGetDailyInteractRewardRequestHandler(Session session, Packet.Request packet)
        {
            session.SendResponse(new GuildDormGetDailyInteractRewardResponse(), packet.Id);
        }

        [RequestPacketHandler("GuildDormGetOneTimeInteractRewardRequest")]
        public static void GuildDormGetOneTimeInteractRewardRequestHandler(Session session, Packet.Request packet)
        {
            session.SendResponse(new GuildDormGetOneTimeInteractRewardResponse(), packet.Id);
        }

        [RequestPacketHandler("GuildDormRecordInteractRequest")]
        public static void GuildDormRecordInteractRequestHandler(Session session, Packet.Request packet)
        {
            session.SendResponse(new GuildDormRecordInteractResponse(), packet.Id);
        }

        [RequestPacketHandler("GuildDormCallRandomBoxRequest")]
        public static void GuildDormCallRandomBoxRequestHandler(Session session, Packet.Request packet)
        {
            session.SendResponse(new GuildDormCallRandomBoxResponse(), packet.Id);
        }

        [RequestPacketHandler("GuildDormSetRoomThemeRequest")]
        public static void GuildDormSetRoomThemeRequestHandler(Session session, Packet.Request packet)
        {
            session.SendResponse(new GuildDormSetRoomThemeResponse()
            {
                NextSetRoomThemeTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            }, packet.Id);
        }

        [RequestPacketHandler("GuildDormKcpConfirmRequest")]
        public static void GuildDormKcpConfirmRequestHandler(Session session, Packet.Request packet)
        {
            session.SendResponse(new GuildDormKcpConfirmResponse(), packet.Id);
        }

        [RequestPacketHandler("GuildDormEnterRoomRequest")]
        public static void GuildDormEnterRoomRequestHandler(Session session, Packet.Request packet)
        {
            session.SendResponse(new GuildDormEnterRoomResponse() { Code = NotAvailableCode }, packet.Id);
        }

        [RequestPacketHandler("GuildDormExitRoomRequest")]
        public static void GuildDormExitRoomRequestHandler(Session session, Packet.Request packet)
        {
            session.SendResponse(new GuildDormExitRoomResponse(), packet.Id);
        }

        [RequestPacketHandler("GuildDormHeartbeatRequest")]
        public static void GuildDormHeartbeatRequestHandler(Session session, Packet.Request packet)
        {
            session.SendResponse(new GuildDormHeartbeatResponse(), packet.Id);
        }

        [RequestPacketHandler("GuildDormChangeCharacterRequest")]
        public static void GuildDormChangeCharacterRequestHandler(Session session, Packet.Request packet)
        {
            session.SendResponse(new GuildDormChangeCharacterResponse(), packet.Id);
        }

        [RequestPacketHandler("GuildDormPlayNoteRequest")]
        public static void GuildDormPlayNoteRequestHandler(Session session, Packet.Request packet)
        {
            session.SendResponse(new GuildDormPlayNoteResponse(), packet.Id);
        }

        [RequestPacketHandler("NotifyGuildDormPlayNoteRequest")]
        public static void NotifyGuildDormPlayNoteRequestHandler(Session session, Packet.Request packet)
        {
        }

        [RequestPacketHandler("GuildDormFurnitureInteractRequest")]
        public static void GuildDormFurnitureInteractRequestHandler(Session session, Packet.Request packet)
        {
            session.SendResponse(new GuildDormFurnitureInteractResponse(), packet.Id);
        }

        [RequestPacketHandler("GuildDormNpcInteractRequest")]
        public static void GuildDormNpcInteractRequestHandler(Session session, Packet.Request packet)
        {
            session.SendResponse(new GuildDormNpcInteractResponse(), packet.Id);
        }
    }
}
