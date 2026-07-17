using AscNet.Common.Database;
using AscNet.Common.MsgPack;
using MessagePack;

namespace AscNet.GameServer.Handlers
{
#pragma warning disable CS8618
    [MessagePackObject(true)]
    public class AssignChapterRecord
    {
        public int ChapterId { get; set; }
        public int CharacterId { get; set; }
        public bool IsGetReward { get; set; }
    }

    [MessagePackObject(true)]
    public class AssignGroupRecord
    {
        public int GroupId { get; set; }
        public int Count { get; set; }
        public bool IsPerfect { get; set; }
        public List<int> FinishStageIds { get; set; } = new();
    }

    [MessagePackObject(true)]
    public class AssignGroupTeamRecord
    {
        public int GroupId { get; set; }
        public List<List<int>> TeamInfoList { get; set; } = new();
        public List<int> CaptainPosList { get; set; } = new();
        public List<int> FirstFightPosList { get; set; } = new();
    }

    [MessagePackObject(true)]
    public class AssignInfo
    {
        public List<AssignChapterRecord> ChapterRecords { get; set; } = new();
        public List<AssignGroupRecord> GroupRecords { get; set; } = new();
        public List<AssignGroupTeamRecord> GroupTeamRecords { get; set; } = new();
    }

    [MessagePackObject(true)]
    public class AssignGetDataResponse
    {
        public int Code { get; set; }
        public AssignInfo AssignInfo { get; set; } = new();
    }

    [MessagePackObject(true)]
    public class AssignSetTeamRequest
    {
        public int GroupId { get; set; }
        public List<List<int>> TeamList { get; set; } = new();
        public List<int> CaptainPosList { get; set; } = new();
        public List<int> FirstFightPosList { get; set; } = new();
    }

    [MessagePackObject(true)]
    public class AssignSetTeamResponse
    {
        public int Code { get; set; }
    }

    [MessagePackObject(true)]
    public class AssignSetCharacterRequest
    {
        public int ChapterId { get; set; }
        public int CharacterId { get; set; }
    }

    [MessagePackObject(true)]
    public class AssignSetCharacterResponse
    {
        public int Code { get; set; }
    }

    [MessagePackObject(true)]
    public class AssignGetRewardRequest
    {
        public int ChapterId { get; set; }
    }

    [MessagePackObject(true)]
    public class AssignGetRewardResponse
    {
        public int Code { get; set; }
        public List<RewardGoods> RewardList { get; set; } = new();
    }

    [MessagePackObject(true)]
    public class AssignResetStageRequest
    {
        public int GroupId { get; set; }
        public int StageId { get; set; }
    }

    [MessagePackObject(true)]
    public class AssignResetStageResponse
    {
        public int Code { get; set; }
    }

    internal static class AssignModule
    {
        private const int ChapterRewardId = 14010134;

        private sealed record AssignGroupDefinition(int GroupId, int ChapterId, int[] StageIds);

        private static readonly AssignGroupDefinition[] GroupDefinitions =
        [
            new(101, 2001, [14010111, 14010112, 14010113]),
            new(102, 2001, [14010121, 14010122, 14010123]),
            new(103, 2001, [14010131, 14010132, 14010133]),
            new(201, 2002, [14010211, 14010212, 14010213]),
            new(202, 2002, [14010221, 14010222, 14010223]),
            new(203, 2002, [14010231, 14010232, 14010233, 14010234]),
            new(301, 2003, [14010311, 14010312, 14010313]),
            new(302, 2003, [14010321, 14010322, 14010323]),
            new(303, 2003, [14010331, 14010332, 14010333, 14010334]),
            new(401, 2004, [14010411, 14010412, 14010413]),
            new(402, 2004, [14010421, 14010422, 14010423, 14010424]),
            new(403, 2004, [14010431, 14010432, 14010433, 14010434]),
            new(501, 2005, [14020511, 14020512, 14020513, 14020514]),
            new(502, 2005, [14020521, 14020522, 14020523, 14020524]),
            new(503, 2005, [14020531, 14020532, 14020533, 14020534]),
            new(601, 2006, [14020611, 14020612, 14020613, 14020614]),
            new(602, 2006, [14020621, 14020622, 14020623, 14020624]),
            new(603, 2006, [14020631, 14020632, 14020633, 14020634]),
            new(701, 2007, [14020711, 14020712, 14020713, 14020714]),
            new(702, 2007, [14020721, 14020722, 14020723, 14020724]),
            new(703, 2007, [14020731, 14020732, 14020733, 14020734]),
            new(801, 2008, [14020811, 14020812, 14020813, 14020814]),
            new(802, 2008, [14020821, 14020822, 14020823, 14020824]),
            new(803, 2008, [14020831, 14020832, 14020833, 14020834])
        ];

        private static readonly Dictionary<int, AssignGroupDefinition> GroupsById =
            GroupDefinitions.ToDictionary(group => group.GroupId);

        private static readonly Dictionary<int, AssignGroupDefinition> GroupsByStageId =
            GroupDefinitions
                .SelectMany(group => group.StageIds.Select(stageId => (stageId, group)))
                .ToDictionary(pair => pair.stageId, pair => pair.group);

        public static List<dynamic> BuildLoginChapterRecords(Player player)
        {
            AssignModeState state = NormalizeState(player);
            return BuildChapterRecords(state)
                .Select(record => (dynamic)new Dictionary<string, object>
                {
                    [nameof(record.ChapterId)] = record.ChapterId,
                    [nameof(record.CharacterId)] = record.CharacterId,
                    [nameof(record.IsGetReward)] = record.IsGetReward
                })
                .ToList();
        }

        public static bool IsStage(uint stageId)
        {
            return stageId <= int.MaxValue && GroupsByStageId.ContainsKey((int)stageId);
        }

        public static bool RecordStageClear(Player player, int stageId, int rebootCount)
        {
            if (!GroupsByStageId.TryGetValue(stageId, out AssignGroupDefinition? definition))
                return false;

            AssignModeState state = NormalizeState(player);
            AssignGroupState group = GetOrCreateGroupState(state, definition.GroupId);
            if (group.FinishStageIds.Contains(stageId))
                return false;

            group.FinishStageIds.Add(stageId);
            group.FinishStageIds.Sort();
            group.RebootCount += Math.Max(0, rebootCount);

            if (definition.StageIds.All(group.FinishStageIds.Contains))
            {
                group.FightCount++;
                if (group.RebootCount == 0)
                    group.IsPerfect = true;
                group.RebootCount = 0;
                group.FinishStageIds.Clear();
            }

            return true;
        }

        [RequestPacketHandler("AssignGetDataRequest")]
        public static void AssignGetDataRequestHandler(Session session, Packet.Request packet)
        {
            session.SendResponse(new AssignGetDataResponse
            {
                Code = 0,
                AssignInfo = BuildAssignInfo(session.player)
            }, packet.Id);
        }

        [RequestPacketHandler("AssignSetTeamRequest")]
        public static void AssignSetTeamRequestHandler(Session session, Packet.Request packet)
        {
            AssignSetTeamRequest request = packet.Deserialize<AssignSetTeamRequest>();
            if (!GroupsById.TryGetValue(request.GroupId, out AssignGroupDefinition? definition)
                || !ValidateTeamRequest(session, definition, request))
            {
                session.SendResponse(new AssignSetTeamResponse { Code = 1 }, packet.Id);
                return;
            }

            AssignModeState state = NormalizeState(session.player);
            state.GroupTeams[request.GroupId] = new AssignTeamRecordState
            {
                TeamInfoList = request.TeamList.Select(team => team.ToList()).ToList(),
                CaptainPosList = request.CaptainPosList.ToList(),
                FirstFightPosList = request.FirstFightPosList.ToList()
            };
            session.player.Save();
            session.SendResponse(new AssignSetTeamResponse { Code = 0 }, packet.Id);
        }

        [RequestPacketHandler("AssignSetCharacterRequest")]
        public static void AssignSetCharacterRequestHandler(Session session, Packet.Request packet)
        {
            AssignSetCharacterRequest request = packet.Deserialize<AssignSetCharacterRequest>();
            AssignModeState state = NormalizeState(session.player);
            bool validChapter = GroupDefinitions.Any(group => group.ChapterId == request.ChapterId);
            bool ownsCharacter = request.CharacterId == 0
                || session.character.Characters.Any(character => character.Id == request.CharacterId);
            bool characterUsedElsewhere = request.CharacterId > 0
                && state.ChapterCharacters.Any(pair => pair.Key != request.ChapterId && pair.Value == request.CharacterId);

            if (!validChapter || !IsChapterPassed(state, request.ChapterId) || !ownsCharacter || characterUsedElsewhere)
            {
                session.SendResponse(new AssignSetCharacterResponse { Code = 1 }, packet.Id);
                return;
            }

            if (request.CharacterId == 0)
                state.ChapterCharacters.Remove(request.ChapterId);
            else
                state.ChapterCharacters[request.ChapterId] = request.CharacterId;

            session.player.Save();
            session.SendResponse(new AssignSetCharacterResponse { Code = 0 }, packet.Id);
        }

        [RequestPacketHandler("AssignGetRewardRequest")]
        public static void AssignGetRewardRequestHandler(Session session, Packet.Request packet)
        {
            AssignGetRewardRequest request = packet.Deserialize<AssignGetRewardRequest>();
            AssignModeState state = NormalizeState(session.player);
            List<AscNet.Table.V2.share.reward.RewardGoodsTable> rewardGoods = RewardHandler.GetRewardGoods(ChapterRewardId);
            if (!IsChapterPassed(state, request.ChapterId)
                || state.ClaimedChapterRewards.Contains(request.ChapterId)
                || rewardGoods.Count == 0)
            {
                session.SendResponse(new AssignGetRewardResponse { Code = 1 }, packet.Id);
                return;
            }

            state.ClaimedChapterRewards.Add(request.ChapterId);
            state.ClaimedChapterRewards.Sort();
            List<RewardGoods> rewards = RewardHandler.GiveRewards(rewardGoods, session);
            session.player.Save();
            session.inventory.Save();
            session.character.Save();
            session.SendResponse(new AssignGetRewardResponse
            {
                Code = 0,
                RewardList = rewards
            }, packet.Id);
        }

        [RequestPacketHandler("AssignResetStageRequest")]
        public static void AssignResetStageRequestHandler(Session session, Packet.Request packet)
        {
            AssignResetStageRequest request = packet.Deserialize<AssignResetStageRequest>();
            if (!GroupsById.TryGetValue(request.GroupId, out AssignGroupDefinition? definition)
                || !definition.StageIds.Contains(request.StageId))
            {
                session.SendResponse(new AssignResetStageResponse { Code = 1 }, packet.Id);
                return;
            }

            AssignModeState state = NormalizeState(session.player);
            if (state.Groups.TryGetValue(request.GroupId, out AssignGroupState? group))
            {
                group.FinishStageIds.Remove(request.StageId);
                group.RebootCount = 0;
            }
            session.player.Save();
            session.SendResponse(new AssignResetStageResponse { Code = 0 }, packet.Id);
        }

        private static AssignInfo BuildAssignInfo(Player player)
        {
            AssignModeState state = NormalizeState(player);
            return new AssignInfo
            {
                ChapterRecords = BuildChapterRecords(state),
                GroupRecords = state.Groups
                    .OrderBy(pair => pair.Key)
                    .Select(pair => new AssignGroupRecord
                    {
                        GroupId = pair.Key,
                        Count = pair.Value.FightCount,
                        IsPerfect = pair.Value.IsPerfect,
                        FinishStageIds = pair.Value.FinishStageIds.ToList()
                    })
                    .ToList(),
                GroupTeamRecords = state.GroupTeams
                    .OrderBy(pair => pair.Key)
                    .Select(pair => new AssignGroupTeamRecord
                    {
                        GroupId = pair.Key,
                        TeamInfoList = pair.Value.TeamInfoList.Select(team => team.ToList()).ToList(),
                        CaptainPosList = pair.Value.CaptainPosList.ToList(),
                        FirstFightPosList = pair.Value.FirstFightPosList.ToList()
                    })
                    .ToList()
            };
        }

        private static List<AssignChapterRecord> BuildChapterRecords(AssignModeState state)
        {
            return GroupDefinitions
                .Select(group => group.ChapterId)
                .Distinct()
                .Where(chapterId => IsChapterPassed(state, chapterId))
                .OrderBy(chapterId => chapterId)
                .Select(chapterId => new AssignChapterRecord
                {
                    ChapterId = chapterId,
                    CharacterId = state.ChapterCharacters.GetValueOrDefault(chapterId),
                    IsGetReward = state.ClaimedChapterRewards.Contains(chapterId)
                })
                .ToList();
        }

        private static bool IsChapterPassed(AssignModeState state, int chapterId)
        {
            AssignGroupDefinition[] chapterGroups = GroupDefinitions
                .Where(group => group.ChapterId == chapterId)
                .ToArray();
            return chapterGroups.Length > 0
                && chapterGroups.All(group => state.Groups.TryGetValue(group.GroupId, out AssignGroupState? progress)
                    && progress.FightCount > 0);
        }

        private static bool ValidateTeamRequest(
            Session session,
            AssignGroupDefinition definition,
            AssignSetTeamRequest request)
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
                if (team.Count is < 1 or > 3 || team.Any(characterId => characterId < 0))
                    return false;

                foreach (int characterId in team.Where(characterId => characterId > 0))
                {
                    if (!ownedCharacterIds.Contains(characterId) || !usedCharacterIds.Add(characterId))
                        return false;
                }

                int captainPos = request.CaptainPosList[i];
                int firstFightPos = request.FirstFightPosList[i];
                if (captainPos < 1 || captainPos > team.Count || team[captainPos - 1] == 0
                    || firstFightPos < 1 || firstFightPos > team.Count || team[firstFightPos - 1] == 0)
                    return false;
            }

            return true;
        }

        private static AssignModeState NormalizeState(Player player)
        {
            player.AssignMode ??= new();
            player.AssignMode.Groups ??= new();
            player.AssignMode.GroupTeams ??= new();
            player.AssignMode.ChapterCharacters ??= new();
            player.AssignMode.ClaimedChapterRewards ??= new();

            foreach (AssignGroupState group in player.AssignMode.Groups.Values)
                group.FinishStageIds ??= new();
            foreach (AssignTeamRecordState team in player.AssignMode.GroupTeams.Values)
            {
                team.TeamInfoList ??= new();
                team.CaptainPosList ??= new();
                team.FirstFightPosList ??= new();
            }

            return player.AssignMode;
        }

        private static AssignGroupState GetOrCreateGroupState(AssignModeState state, int groupId)
        {
            if (!state.Groups.TryGetValue(groupId, out AssignGroupState? group))
            {
                group = new AssignGroupState();
                state.Groups[groupId] = group;
            }
            group.FinishStageIds ??= new();
            return group;
        }
    }
}
