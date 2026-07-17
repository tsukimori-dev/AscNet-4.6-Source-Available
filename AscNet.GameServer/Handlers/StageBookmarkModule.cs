using MessagePack;

namespace AscNet.GameServer.Handlers
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [MessagePackObject(true)]
    public class StageBookmarkData
    {
        public int StageId { get; set; }
        public string MovieId { get; set; } = string.Empty;
        public int ActionId { get; set; }
        public long CreateTime { get; set; }
        public long UpdateTime { get; set; }
    }

    [MessagePackObject(true)]
    public class GetStageBookmarkResponse
    {
        public int Code { get; set; }
        public List<StageBookmarkData> StageBookmarkList { get; set; } = new();
        public List<StageBookmarkData> BookmarkList { get; set; } = new();
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    internal class StageBookmarkModule
    {
        [RequestPacketHandler("GetStageBookmarkRequest")]
        public static void GetStageBookmarkRequestHandler(Session session, Packet.Request packet)
        {
            // The current client asks for optional stage resume points when entering the
            // chapter UI. AscNet does not persist per-movie checkpoints yet, so return a
            // successful empty bookmark set instead of leaving the request unresolved.
            session.SendResponse(new GetStageBookmarkResponse(), packet.Id);
        }
    }
}
