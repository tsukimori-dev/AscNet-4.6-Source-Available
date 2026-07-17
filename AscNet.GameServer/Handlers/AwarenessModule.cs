using AscNet.Common.Database;
using AscNet.Common.MsgPack;
using MessagePack;

namespace AscNet.GameServer.Handlers
{
#pragma warning disable CS8618
    [MessagePackObject(true)]
    public class AwarenessChapterRecord
    {
        public int ChapterId { get; set; }
        public int CharacterId { get; set; }
        public bool IsGetReward { get; set; }
    }

    [MessagePackObject(true)]
    public class AwarenessChallengeRecord
    {
        public int ChapterId { get; set; }
        public int Count { get; set; }
        public List<int> FinishStageIds { get; set; } = new();
    }

    [MessagePackObject(true)]
    public class AwarenessTeamRecord
    {
        public int ChapterId { get; set; }
        public List<List<int>> TeamInfoList { get; set; } = new();
        public List<int> CaptainPosList { get; set; } = new();
        public List<int> FirstFightPosList { get; set; } = new();
    }

    [MessagePackObject(true)]
    public class AwarenessInfo
    {
        public List<AwarenessChapterRecord> ChapterRecords { get; set; } = new();
        public List<AwarenessChallengeRecord> ChallengeRecords { get; set; } = new();
        public List<AwarenessTeamRecord> TeamRecords { get; set; } = new();
    }

    [MessagePackObject(true)]
    public class NotifyLoginAwarenessInfo
    {
        public AwarenessInfo AwarenessInfo { get; set; } = new();
    }

    [MessagePackObject(true)]
    public class AwarenessGetDataResponse
    {
        public int Code { get; set; }
        public AwarenessInfo AwarenessInfo { get; set; } = new();
    }

    [MessagePackObject(true)]
    public class AwarenessSetTeamRequest
    {
        public int ChapterId { get; set; }
        public List<List<int>> TeamList { get; set; } = new();
        public List<int> CaptainPosList { get; set; } = new();
        public List<int> FirstFightPosList { get; set; } = new();
    }

    [MessagePackObject(true)]
    public class AwarenessSetTeamResponse
    {
        public int Code { get; set; }
    }

    [MessagePackObject(true)]
    public class AwarenessSetCharacterRequest
    {
        public int ChapterId { get; set; }
        public int CharacterId { get; set; }
    }

    [MessagePackObject(true)]
    public class AwarenessSetCharacterResponse
    {
        public int Code { get; set; }
    }

    [MessagePackObject(true)]
    public class AwarenessGetRewardRequest
    {
        public int ChapterId { get; set; }
    }

    [MessagePackObject(true)]
    public class AwarenessGetRewardResponse
    {
        public int Code { get; set; }
        public List<RewardGoods> RewardList { get; set; } = new();
    }

    [MessagePackObject(true)]
    public class AwarenessResetStageRequest
    {
        public int ChapterId { get; set; }
        public int StageId { get; set; }
    }

    [MessagePackObject(true)]
    public class AwarenessResetStageResponse
    {
        public int Code { get; set; }
    }
#pragma warning restore CS8618

    internal static class AwarenessModule
    {
        private sealed record AwarenessChapterDefinition(
            int ChapterId,
            int[] StageIds,
            int FightEventId,
            int RewardId);

        // Pinned client contract: PGR_Data ec7069c, AwarenessChapter.json.
        private static readonly AwarenessChapterDefinition[] ChapterDefinitions =
        [
            new(3001, [14020901, 14020903, 14020905], 2270101, 14010141),
            new(3002, [14020906, 14020908, 14020910], 2270102, 14010142),
            new(3003, [14020911, 14020913, 14020915], 2270103, 14010143),
            new(3004, [14020921, 14020923, 14020925], 2270104, 14010145),
            new(3005, [14020916, 14020918, 14020920], 2270105, 14010144),
            new(3006, [14020926, 14020928, 14020930], 2270106, 14010146)
        ];

        private static readonly Dictionary<int, AwarenessChapterDefinition> ChaptersById =
            ChapterDefinitions.ToDictionary(chapter => chapter.ChapterId);

        private static readonly Dictionary<int, AwarenessChapterDefinition> ChaptersByStageId =
            ChapterDefinitions
                .SelectMany(chapter => chapter.StageIds.Select(stageId => (stageId, chapter)))
                .ToDictionary(pair => pair.stageId, pair => pair.chapter);

        public static NotifyLoginAwarenessInfo BuildLoginData(Player player)
        {
            return new NotifyLoginAwarenessInfo
            {
                AwarenessInfo = BuildAwarenessInfo(player)
            };
        }

        public static bool IsStage(uint stageId)
        {
            return stageId <= int.MaxValue && ChaptersByStageId.ContainsKey((int)stageId);
        }

        public static void ApplyPreFightData(uint stageId, PreFightResponse response)
        {
            if (stageId > int.MaxValue
                || !ChaptersByStageId.TryGetValue((int)stageId, out AwarenessChapterDefinition? definition))
                return;

            if (!response.FightData.EventIds.Any(eventId => Convert.ToInt32(eventId) == definition.FightEventId))
                response.FightData.EventIds.Add(definition.FightEventId);
            response.FightData.Restartable = true;
        }

        public static bool RecordStageClear(Player player, int stageId)
        {
            if (!ChaptersByStageId.TryGetValue(stageId, out AwarenessChapterDefinition? definition))
                return false;

            AwarenessModeState state = NormalizeState(player);
            AwarenessChapterState chapter = GetOrCreateChapterState(state, definition.ChapterId);
            if (chapter.FinishStageIds.Contains(stageId))
                return false;

            chapter.FinishStageIds.Add(stageId);
            chapter.FinishStageIds.Sort();
            if (definition.StageIds.All(chapter.FinishStageIds.Contains))
            {
                chapter.FightCount++;
                chapter.FinishStageIds.Clear();
            }

            return true;
        }

        [RequestPacketHandler("AwarenessGetDataRequest")]
        public static void AwarenessGetDataRequestHandler(Session session, Packet.Request packet)
        {
            session.SendResponse(new AwarenessGetDataResponse
            {
                Code = 0,
                AwarenessInfo = BuildAwarenessInfo(session.player)
            }, packet.Id);
        }

        [RequestPacketHandler("AwarenessSetTeamRequest")]
        public static void AwarenessSetTeamRequestHandler(Session session, Packet.Request packet)
        {
            AwarenessSetTeamRequest request = packet.Deserialize<AwarenessSetTeamRequest>();
            if (!ChaptersById.TryGetValue(request.ChapterId, out AwarenessChapterDefinition? definition)
                || !ValidateTeamRequest(session, definition, request))
            {
                session.SendResponse(new AwarenessSetTeamResponse { Code = 1 }, packet.Id);
                return;
            }

            AwarenessModeState state = NormalizeState(session.player);
            state.ChapterTeams[request.ChapterId] = new AwarenessTeamRecordState
            {
                TeamInfoList = request.TeamList.Select(team => team.ToList()).ToList(),
                CaptainPosList = request.CaptainPosList.ToList(),
                FirstFightPosList = request.FirstFightPosList.ToList()
            };
            session.player.Save();
            session.SendResponse(new AwarenessSetTeamResponse { Code = 0 }, packet.Id);
        }

        [RequestPacketHandler("AwarenessSetCharacterRequest")]
        public static void AwarenessSetCharacterRequestHandler(Session session, Packet.Request packet)
        {
            AwarenessSetCharacterRequest request = packet.Deserialize<AwarenessSetCharacterRequest>();
            AwarenessModeState state = NormalizeState(session.player);
            bool ownsCharacter = request.CharacterId == 0
                || session.character.Characters.Any(character => character.Id == request.CharacterId);
            bool characterUsedElsewhere = request.CharacterId > 0
                && state.ChapterCharacters.Any(pair => pair.Key != request.ChapterId && pair.Value == request.CharacterId);

            if (!ChaptersById.ContainsKey(request.ChapterId)
                || !IsChapterPassed(state, request.ChapterId)
                || !ownsCharacter
                || characterUsedElsewhere)
            {
                session.SendResponse(new AwarenessSetCharacterResponse { Code = 1 }, packet.Id);
                return;
            }

            if (request.CharacterId == 0)
                state.ChapterCharacters.Remove(request.ChapterId);
            else
                state.ChapterCharacters[request.ChapterId] = request.CharacterId;

            session.player.Save();
            session.SendResponse(new AwarenessSetCharacterResponse { Code = 0 }, packet.Id);
        }

        [RequestPacketHandler("AwarenessGetRewardRequest")]
        public static void AwarenessGetRewardRequestHandler(Session session, Packet.Request packet)
        {
            _ = packet.Deserialize<AwarenessGetRewardRequest>();

            // The pinned client exposes this legacy RPC type but has no call site for it.
            // Chapter rewards are already granted by the final stage's FirstRewardId, so
            // treating that reward as a second claim here would allow duplicate grants.
            session.SendResponse(new AwarenessGetRewardResponse { Code = 1 }, packet.Id);
        }

        [RequestPacketHandler("AwarenessResetStageRequest")]
        public static void AwarenessResetStageRequestHandler(Session session, Packet.Request packet)
        {
            AwarenessResetStageRequest request = packet.Deserialize<AwarenessResetStageRequest>();
            if (!ChaptersById.TryGetValue(request.ChapterId, out AwarenessChapterDefinition? definition)
                || !definition.StageIds.Contains(request.StageId))
            {
                session.SendResponse(new AwarenessResetStageResponse { Code = 1 }, packet.Id);
                return;
            }

            AwarenessModeState state = NormalizeState(session.player);
            if (state.Chapters.TryGetValue(request.ChapterId, out AwarenessChapterState? chapter))
                chapter.FinishStageIds.Remove(request.StageId);
            session.player.Save();
            session.SendResponse(new AwarenessResetStageResponse { Code = 0 }, packet.Id);
        }

        private static AwarenessInfo BuildAwarenessInfo(Player player)
        {
            AwarenessModeState state = NormalizeState(player);
            return new AwarenessInfo
            {
                ChapterRecords = ChapterDefinitions
                    .Where(chapter => IsChapterPassed(state, chapter.ChapterId))
                    .OrderBy(chapter => chapter.ChapterId)
                    .Select(chapter => new AwarenessChapterRecord
                    {
                        ChapterId = chapter.ChapterId,
                        CharacterId = state.ChapterCharacters.GetValueOrDefault(chapter.ChapterId),
                        IsGetReward = state.ClaimedChapterRewards.Contains(chapter.ChapterId)
                    })
                    .ToList(),
                ChallengeRecords = state.Chapters
                    .OrderBy(pair => pair.Key)
                    .Select(pair => new AwarenessChallengeRecord
                    {
                        ChapterId = pair.Key,
                        Count = pair.Value.FightCount,
                        FinishStageIds = pair.Value.FinishStageIds.ToList()
                    })
                    .ToList(),
                TeamRecords = state.ChapterTeams
                    .OrderBy(pair => pair.Key)
                    .Select(pair => new AwarenessTeamRecord
                    {
                        ChapterId = pair.Key,
                        TeamInfoList = pair.Value.TeamInfoList.Select(team => team.ToList()).ToList(),
                        CaptainPosList = pair.Value.CaptainPosList.ToList(),
                        FirstFightPosList = pair.Value.FirstFightPosList.ToList()
                    })
                    .ToList()
            };
        }

        private static bool ValidateTeamRequest(
            Session session,
            AwarenessChapterDefinition definition,
            AwarenessSetTeamRequest request)
        {
            if (request.TeamList.Count != definition.StageIds.Length
                || request.CaptainPosList.Count != request.TeamList.Count
                || request.FirstFightPosList.Count != request.TeamList.Count)
                return false;

            HashSet<int> ownedCharacterIds = session.character.Characters
                .Select(character => (int)character.Id)
                .ToHashSet();
            HashSet<int> usedCharacterIds = new();

            for (int i = 0; i < request.TeamList.Count; i++)
            {
                List<int> team = request.TeamList[i];
                if (team.Count != 3 || team.Any(characterId => characterId <= 0))
                    return false;

                foreach (int characterId in team)
                {
                    if (!ownedCharacterIds.Contains(characterId) || !usedCharacterIds.Add(characterId))
                        return false;
                }

                int captainPos = request.CaptainPosList[i];
                int firstFightPos = request.FirstFightPosList[i];
                if (captainPos < 1 || captainPos > team.Count
                    || firstFightPos < 1 || firstFightPos > team.Count)
                    return false;
            }

            return true;
        }

        private static bool IsChapterPassed(AwarenessModeState state, int chapterId)
        {
            return state.Chapters.TryGetValue(chapterId, out AwarenessChapterState? chapter)
                && chapter.FightCount > 0;
        }

        private static AwarenessModeState NormalizeState(Player player)
        {
            player.AwarenessMode ??= new();
            player.AwarenessMode.Chapters ??= new();
            player.AwarenessMode.ChapterTeams ??= new();
            player.AwarenessMode.ChapterCharacters ??= new();
            player.AwarenessMode.ClaimedChapterRewards ??= new();

            foreach (AwarenessChapterState chapter in player.AwarenessMode.Chapters.Values)
                chapter.FinishStageIds ??= new();
            foreach (AwarenessTeamRecordState team in player.AwarenessMode.ChapterTeams.Values)
            {
                team.TeamInfoList ??= new();
                team.CaptainPosList ??= new();
                team.FirstFightPosList ??= new();
            }

            return player.AwarenessMode;
        }

        private static AwarenessChapterState GetOrCreateChapterState(AwarenessModeState state, int chapterId)
        {
            if (!state.Chapters.TryGetValue(chapterId, out AwarenessChapterState? chapter))
            {
                chapter = new AwarenessChapterState();
                state.Chapters[chapterId] = chapter;
            }

            chapter.FinishStageIds ??= new();
            return chapter;
        }
    }
}
