using MessagePack;

namespace AscNet.GameServer.Handlers
{

    #region MsgPackScheme
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [MessagePackObject(true)]
    public class MuralShareRewardResponse
    {
        public int Code;
        public List<object> RewardGoods = new();
    }

    [MessagePackObject(true)]
    public class MuralShareCollectRewardResponse
    {
        public int Code;
        public List<object> RewardGoods = new();
    }

    [MessagePackObject(true)]
    public class MuralShareSaveResponse
    {
        public int Code;
    }

    [MessagePackObject(true)]
    public class MuralShareUnlockPaintingResponse
    {
        public int Code;
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    #endregion

    internal class MuralShareModule
    {
        [RequestPacketHandler("MuralShareRewardRequest")]
        public static void MuralShareRewardRequestHandler(Session session, Packet.Request packet)
        {
            session.SendResponse(new MuralShareRewardResponse(), packet.Id);
        }

        [RequestPacketHandler("MuralShareCollectRewardRequest")]
        public static void MuralShareCollectRewardRequestHandler(Session session, Packet.Request packet)
        {
            session.SendResponse(new MuralShareCollectRewardResponse(), packet.Id);
        }

        [RequestPacketHandler("MuralShareSaveRequest")]
        public static void MuralShareSaveRequestHandler(Session session, Packet.Request packet)
        {
            session.SendResponse(new MuralShareSaveResponse(), packet.Id);
        }

        [RequestPacketHandler("MuralShareUnlockPaintingRequest")]
        public static void MuralShareUnlockPaintingRequestHandler(Session session, Packet.Request packet)
        {
            session.SendResponse(new MuralShareUnlockPaintingResponse(), packet.Id);
        }
    }
}
