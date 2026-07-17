using AscNet.Common.Database;
using AscNet.Common.MsgPack;
using MessagePack;

namespace AscNet.GameServer.Handlers
{
    #region MsgPackScheme
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [MessagePackObject(true)]
    public class LifeTreeFinishProcessRequest
    {
        public int Process { get; set; }
        public int ProcessId { get; set; }
    }

    [MessagePackObject(true)]
    public class LifeTreeFinishProcessResponse
    {
        public int Code { get; set; }
        public bool IsFinishGuide { get; set; }
        public bool IsFinishLifeTreePv { get; set; }
        public List<int> FinishedChapters { get; set; } = new();
    }

    [MessagePackObject(true)]
    public class LifeTreeUnlockCharacterRequest
    {
        public int CharacterId { get; set; }
        public int Status { get; set; }
    }

    [MessagePackObject(true)]
    public class LifeTreeUnlockCharacterResponse
    {
        public int Code { get; set; }
        public LifeTreeUnlockCharacterData CharacterData { get; set; } = new();
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    #endregion

    internal class LifeTreeModule
    {
        private const int InvalidUnlockRequestCode = 1;
        private static readonly int[] FinishedProcessOneChapters = [61, 58, 57];
        private static readonly int[] FinishedProcessOneCharacterIds = [1031005, 1021007];
        private static readonly HashSet<int> CurrentClientLifeTreeChapterIds = [56, 57, 58, 59, 60, 61, 62];
        private static readonly IReadOnlyDictionary<int, int> CurrentClientMaxUnlockStatus =
            new Dictionary<int, int>
            {
                [1031005] = 1,
                [1291003] = 1,
                [1141004] = 1,
                [1391003] = 1,
                [1061004] = 1,
                [1021007] = 2
            };

        [RequestPacketHandler("LifeTreeFinishProcessRequest")]
        public static void LifeTreeFinishProcessRequestHandler(Session session, Packet.Request packet)
        {
            LifeTreeFinishProcessRequest request = packet.Deserialize<LifeTreeFinishProcessRequest>();
            bool handled = FinishProcess(session.player, request.Process, request.ProcessId);
            session.log.Debug($"LifeTreeFinishProcessRequest Process={request.Process} ProcessId={request.ProcessId}");
            if (handled)
                session.SendPush(BuildNotifyLifeTreeData(session.player));

            NotifyLifeTreeData data = BuildNotifyLifeTreeData(session.player);
            session.SendResponse(new LifeTreeFinishProcessResponse
            {
                IsFinishGuide = data.IsFinishGuide,
                IsFinishLifeTreePv = data.IsFinishLifeTreePv,
                FinishedChapters = data.FinishedChapters.ToList()
            }, packet.Id);
        }

        [RequestPacketHandler("LifeTreeUnlockCharacterRequest")]
        public static void LifeTreeUnlockCharacterRequestHandler(Session session, Packet.Request packet)
        {
            LifeTreeUnlockCharacterRequest request = packet.Deserialize<LifeTreeUnlockCharacterRequest>();
            LifeTreeUnlockCharacterResponse response = new();

            if (!CurrentClientMaxUnlockStatus.TryGetValue(request.CharacterId, out int maxUnlockStatus)
                || request.Status < 1
                || request.Status > maxUnlockStatus
                || !session.character.Characters.Any(character => character.Id == request.CharacterId))
            {
                response.Code = InvalidUnlockRequestCode;
                session.SendResponse(response, packet.Id);
                return;
            }

            NotifyLifeTreeData data = BuildNotifyLifeTreeData(session.player);
            data.UnlockCharacterData ??= new();
            data.UnlockCharacterData.TryGetValue(request.CharacterId, out LifeTreeUnlockCharacterData? existingData);
            int currentStatus = existingData?.UnlockStatus ?? 0;

            if (request.Status == currentStatus && existingData is not null)
            {
                response.CharacterData = existingData;
                session.SendResponse(response, packet.Id);
                return;
            }

            if (request.Status != currentStatus + 1)
            {
                response.Code = InvalidUnlockRequestCode;
                session.SendResponse(response, packet.Id);
                return;
            }

            LifeTreeUnlockCharacterData characterData = new()
            {
                Id = request.CharacterId,
                UnlockStatus = request.Status
            };
            data.UnlockCharacterData[request.CharacterId] = characterData;
            session.player.Save();

            response.CharacterData = characterData;
            session.SendResponse(response, packet.Id);
        }

        internal static NotifyLifeTreeData BuildNotifyLifeTreeData(Player player)
        {
            player.LifeTreeData ??= new();
            return player.LifeTreeData;
        }

        private static bool FinishProcess(Player player, int process, int processId)
        {
            if (process == 3)
            {
                if (!CurrentClientLifeTreeChapterIds.Contains(processId))
                    return false;

                player.LifeTreeData ??= new();
                NotifyLifeTreeData chapterData = player.LifeTreeData;
                chapterData.FinishedChapters ??= new();
                if (!chapterData.FinishedChapters.Contains(processId))
                {
                    chapterData.FinishedChapters.Add(processId);
                    chapterData.FinishedChapters = chapterData.FinishedChapters
                        .Distinct()
                        .OrderByDescending(chapterId => chapterId)
                        .ToList();
                    player.Save();
                }

                return true;
            }

            if (process is not (1 or 2))
                return false;

            player.LifeTreeData ??= new();
            NotifyLifeTreeData data = player.LifeTreeData;
            data.FinishedChapters ??= new();
            data.UnlockCharacterData ??= new();
            bool changed = false;

            if (!data.IsFinishGuide)
            {
                data.IsFinishGuide = true;
                changed = true;
            }

            if (!data.IsFinishLifeTreePv)
            {
                data.IsFinishLifeTreePv = true;
                changed = true;
            }

            foreach (int chapterId in FinishedProcessOneChapters)
            {
                if (!data.FinishedChapters.Contains(chapterId))
                {
                    data.FinishedChapters.Add(chapterId);
                    changed = true;
                }
            }

            foreach (int characterId in FinishedProcessOneCharacterIds)
            {
                if (!data.UnlockCharacterData.TryGetValue(characterId, out LifeTreeUnlockCharacterData? characterData)
                    || characterData.UnlockStatus != 1)
                {
                    data.UnlockCharacterData[characterId] = new LifeTreeUnlockCharacterData
                    {
                        Id = characterId,
                        UnlockStatus = 1
                    };
                    changed = true;
                }
            }

            if (changed)
            {
                data.FinishedChapters = data.FinishedChapters
                    .Distinct()
                    .OrderByDescending(chapterId => chapterId)
                    .ToList();
                player.Save();
            }

            return true;
        }
    }
}
