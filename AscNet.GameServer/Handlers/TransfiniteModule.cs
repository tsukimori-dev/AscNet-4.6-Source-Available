using AscNet.Common.Database;
using AscNet.Common.MsgPack;
using AscNet.Common.Util;
using MessagePack;
using Newtonsoft.Json.Linq;

namespace AscNet.GameServer.Handlers
{
#pragma warning disable CS8618
    [MessagePackObject(true)]
    public sealed class NotifyTransfiniteData
    {
        public TransfiniteDataPayload TransfiniteData { get; set; } = new();
    }

    [MessagePackObject(true)]
    public sealed class TransfiniteDataPayload
    {
        public int ActivityId { get; set; }
        public long BeginTime { get; set; }
        public int RegionId { get; set; }
        public List<TransfiniteBattleInfo> BattleInfo { get; set; } = new();
        public List<TransfiniteBestSpendTime> BestSpendTime { get; set; } = new();
        public int CircleId { get; set; }
        public dynamic? RotateSettleInfo { get; set; }
        public int MaxRotateStageProgressIndex { get; set; }
        public List<int> GotScoreRewardIndex { get; set; } = new();
        public int StageGroupId { get; set; }
    }

    [MessagePackObject(true)]
    public sealed class TransfiniteBestSpendTime
    {
        public int StageGroupId { get; set; }
        public int BestSpendTime { get; set; }
    }

    [MessagePackObject(true)]
    public sealed class TransfiniteBattleInfo
    {
        public int StageGroupId { get; set; }
        public TransfiniteTeamInfo? TeamInfo { get; set; }
        public List<TransfiniteStageInfo> StageInfo { get; set; } = new();
        public int StageProgressIndex { get; set; }
        public int StartStageProgress { get; set; }
        public TransfiniteBattleResult? Result { get; set; }
        public TransfiniteBattleResult? LastResult { get; set; }
        public List<TransfiniteBattleResult> HistoryResults { get; set; } = new();
    }

    [MessagePackObject(true)]
    public sealed class TransfiniteTeamInfo
    {
        public List<int> CharacterIdList { get; set; } = new();
        public int CaptainPos { get; set; }
        public int FirstFightPos { get; set; }
        public int SelectedGeneralSkill { get; set; }
        public int EnterCgIndex { get; set; }
        public int SettleCgIndex { get; set; }
    }

    [MessagePackObject(true)]
    public sealed class TransfiniteStageInfo
    {
        public int StageId { get; set; }
        public bool IsWin { get; set; }
        public int SpendTime { get; set; }
        public int Score { get; set; }
    }

    [MessagePackObject(true)]
    public sealed class TransfiniteBattleResult
    {
        public int LastWinStageId { get; set; }
        public int StageSpendTime { get; set; }
        public List<TransfiniteCharacterResult> CharacterResultList { get; set; } = new();
    }

    [MessagePackObject(true)]
    public sealed class TransfiniteCharacterResult
    {
        public int CharacterId { get; set; }
        public int HpPercent { get; set; }
        public int Energy { get; set; }
    }

    [MessagePackObject(true)]
    public sealed class TransfiniteSetTeamRequest
    {
        public int StageGroupId { get; set; }
        public TransfiniteTeamInfo TeamInfo { get; set; } = new();
        public bool ResetStageIndex { get; set; }
    }

    [MessagePackObject(true)]
    public sealed class TransfiniteSetTeamResponse
    {
        public int Code { get; set; }
    }

    [MessagePackObject(true)]
    public sealed class TransfiniteConfirmBattleResultRequest
    {
        public int StageGroupId { get; set; }
        public bool IsGiveUp { get; set; }
    }

    [MessagePackObject(true)]
    public sealed class TransfiniteConfirmBattleResultResponse
    {
        public int Code { get; set; }
        public TransfiniteBattleInfo? BattleInfo { get; set; }
        public List<RewardGoods> RewardGoodsList { get; set; } = new();
    }

    [MessagePackObject(true)]
    public sealed class TransfiniteResetStageGroupRequest
    {
        public int StageGroupId { get; set; }
    }

    [MessagePackObject(true)]
    public sealed class TransfiniteResetStageGroupResponse
    {
        public int Code { get; set; }
        public TransfiniteBattleInfo? BattleInfo { get; set; }
        public List<RewardGoods> RewardGoodsList { get; set; } = new();
    }

    [MessagePackObject(true)]
    public sealed class TransfiniteGetScoreRewardRequest
    {
        public List<int> ScoreRewardIndex { get; set; } = new();
    }

    [MessagePackObject(true)]
    public sealed class TransfiniteGetScoreRewardResponse
    {
        public int Code { get; set; }
        public List<int> GotScoreRewardIndex { get; set; } = new();
        public List<RewardGoods> RewardGoodsList { get; set; } = new();
    }

    [MessagePackObject(true)]
    public sealed class TransfiniteGetRotateSettleInfoRequest
    {
    }

    [MessagePackObject(true)]
    public sealed class TransfiniteGetRotateSettleInfoResponse
    {
        public int Code { get; set; }
        public List<RewardGoods> RewardGoodsList { get; set; } = new();
        public int MaxStageProgressIndex { get; set; }
        public int SettleTransfiniteScore { get; set; }
        public int UnSettleTransfiniteScore { get; set; }
    }
#pragma warning restore CS8618

    internal static class TransfiniteModule
    {
        private const string ConfigPath = "Configs/transfinite.json";
        private const int SuccessCode = 0;
        private const int InvalidRequestCode = 1;
        private static readonly Lazy<JObject> Config = new(() => JsonSnapshot.LoadObject(ConfigPath));
        private static readonly Lazy<IReadOnlyList<int>> StageScores = new(() =>
            ReadIntArray(Config.Value["StageScores"], "StageScores", requirePositive: false));
        private static readonly Lazy<HashSet<int>> ExtraStageIndexes = new(() =>
            ReadIntArray(Config.Value["ExtraStageIndexes"], "ExtraStageIndexes").ToHashSet());
        private static readonly Lazy<IReadOnlyList<int>> ScoreRewardThresholds = new(() =>
            ReadIntArray(Config.Value["ScoreRewardThresholds"], "ScoreRewardThresholds"));
        private static readonly Lazy<IReadOnlyDictionary<int, IReadOnlyList<ScoreRewardDefinition>>> ScoreRewards =
            new(ReadScoreRewards);
        private static readonly Lazy<IReadOnlyList<RegionDefinition>> Regions = new(ReadRegions);
        private static readonly Lazy<IReadOnlyDictionary<int, StageGroupDefinition>> StageGroups = new(ReadStageGroups);
        private static readonly Lazy<IReadOnlyDictionary<uint, StageGroupDefinition>> StageToGroup = new(() =>
            StageGroups.Value.Values
                .SelectMany(group => group.StageIds.Select(stageId => (StageId: (uint)stageId, Group: group)))
                .ToDictionary(pair => pair.StageId, pair => pair.Group));

        private sealed class RegionDefinition
        {
            public int Id { get; init; }
            public int MinLevel { get; init; }
            public int MaxLevel { get; init; }
            public IReadOnlyList<int> StageGroupIds { get; init; } = [];
        }

        private sealed class StageGroupDefinition
        {
            public int Id { get; init; }
            public IReadOnlyList<int> StageIds { get; init; } = [];
            public IReadOnlyList<int> GroupEventIds { get; init; } = [];
            public IReadOnlyList<int> NormalEventIds { get; init; } = [];
            public IReadOnlyList<int> HiddenEventIds { get; init; } = [];
            public IReadOnlyList<int> FinalEventIds { get; init; } = [];
        }

        private sealed class ScoreRewardDefinition
        {
            public int Id { get; init; }
            public int TemplateId { get; init; }
            public int Count { get; init; }
        }

        public static void PrepareLogin(Session session, long? now = null)
        {
            TransfiniteModeState state = EnsureState(
                session.player,
                now ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                out bool cycleReset);
            bool inventoryChanged = SynchronizeScoreItem(session.inventory, state.Score, out _);
            if (inventoryChanged)
                session.inventory.Save();
            if (cycleReset)
                session.player.Save();
        }

        public static NotifyTransfiniteData BuildLoginData(Player player, long? now = null)
        {
            TransfiniteModeState state = EnsureState(
                player,
                now ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                out _);
            NotifyTransfiniteData result = new()
            {
                TransfiniteData = new TransfiniteDataPayload
                {
                    ActivityId = state.ActivityId,
                    BeginTime = state.BeginTime,
                    RegionId = state.RegionId,
                    BattleInfo = [BuildBattleInfo(state)],
                    CircleId = state.CircleId,
                    RotateSettleInfo = null,
                    MaxRotateStageProgressIndex = state.MaxRotateStageProgressIndex,
                    GotScoreRewardIndex = state.ClaimedScoreRewardIndexes.ToList(),
                    StageGroupId = state.StageGroupId
                }
            };
            if (state.BestSpendTime > 0)
            {
                result.TransfiniteData.BestSpendTime.Add(new TransfiniteBestSpendTime
                {
                    StageGroupId = state.StageGroupId,
                    BestSpendTime = state.BestSpendTime
                });
            }
            return result;
        }

        public static bool IsStage(uint stageId)
        {
            return StageToGroup.Value.ContainsKey(stageId);
        }

        public static bool TryPreparePreFight(
            Session session,
            PreFightRequest.PreFightRequestPreFightData preFightData,
            out int errorCode)
        {
            errorCode = SuccessCode;
            if (!IsStage(preFightData.StageId))
                return true;

            TransfiniteModeState state = EnsureSessionState(session);
            StageGroupDefinition group = StageToGroup.Value[preFightData.StageId];
            if (group.Id != state.StageGroupId
                || state.PendingResult is not null
                || state.StageProgressIndex >= group.StageIds.Count
                || CurrentStageId(state, group) != preFightData.StageId)
            {
                errorCode = InvalidRequestCode;
                return false;
            }

            List<int> requestedCharacterIds = NormalizeCharacterIds(preFightData.CardIds?.Select(id => (int)id));
            if (state.Team is null)
            {
                if (!ValidateOwnedTeam(session, requestedCharacterIds))
                {
                    errorCode = InvalidRequestCode;
                    return false;
                }
                state.Team = BuildTeamState(
                    requestedCharacterIds,
                    preFightData.CaptainPos,
                    preFightData.FirstFightPos,
                    preFightData.GeneralSkill,
                    preFightData.EnterCgIndex,
                    preFightData.SettleCgIndex,
                    null);
                session.player.Save();
            }
            else
            {
                List<int> savedCharacterIds = state.Team.CharacterIds.Where(id => id > 0).ToList();
                if (requestedCharacterIds.All(id => id == 0))
                {
                    preFightData.CardIds = savedCharacterIds.Select(id => (uint)id).ToList();
                }
                else if (!requestedCharacterIds.Where(id => id > 0).SequenceEqual(savedCharacterIds)
                    || !ValidateOwnedTeam(session, requestedCharacterIds))
                {
                    errorCode = InvalidRequestCode;
                    return false;
                }
            }

            return true;
        }

        public static void ApplyPreFight(Player player, PreFightResponse.PreFightResponseFightData fightData)
        {
            if (!StageToGroup.Value.TryGetValue(fightData.StageId, out StageGroupDefinition? group))
                return;

            TransfiniteModeState state = EnsureState(
                player,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                out _);
            if (state.StageGroupId != group.Id)
                return;

            int stageIndex = FindStageIndex(group, (int)fightData.StageId);
            List<int> eventIds = group.GroupEventIds.ToList();
            eventIds.AddRange(stageIndex switch
            {
                >= 0 and <= 9 => group.NormalEventIds,
                >= 10 and <= 12 => group.HiddenEventIds,
                13 => group.FinalEventIds,
                _ => []
            });
            int monsterLevel = Math.Clamp((int)player.PlayerData.Level, 80, 120);
            fightData.RebootId = RequiredPositiveInt("RebootId");
            fightData.PassTimeLimit = RequiredPositiveInt("PassTimeLimit");
            fightData.MonsterLevel = [monsterLevel, monsterLevel, monsterLevel];
            fightData.EventIds = eventIds.Distinct().Select(eventId => (dynamic)eventId).ToList();
            fightData.Restartable = true;
            fightData.PlayerLevel = (int)player.PlayerData.Level;
        }

        public static TransfiniteBattleResult? RecordFightSettle(Session session, FightSettleRequest request)
        {
            uint stageId = request.Result.StageId;
            if (!StageToGroup.Value.TryGetValue(stageId, out StageGroupDefinition? group))
                return null;

            TransfiniteModeState state = EnsureSessionState(session);
            if (state.StageGroupId != group.Id || CurrentStageId(state, group) != stageId || state.Team is null)
                throw new InvalidDataException($"{ConfigPath}: rejected out-of-order Transfinite settlement for stage {stageId}.");

            int spendTime = ResolveStageSpendTime(request.Result);
            TransfiniteBattleResultState pending = new()
            {
                LastWinStageId = (int)stageId,
                StageSpendTime = spendTime,
                CharacterResultList = state.Team.CharacterResults
                    .Select(CloneCharacterResult)
                    .ToList()
            };
            state.PendingResult = pending;
            return ToProtocolResult(pending);
        }

        [RequestPacketHandler("TransfiniteSetTeamRequest")]
        public static void TransfiniteSetTeamRequestHandler(Session session, Packet.Request packet)
        {
            TransfiniteSetTeamRequest request = packet.Deserialize<TransfiniteSetTeamRequest>();
            TransfiniteModeState state = EnsureSessionState(session);
            TransfiniteTeamInfo? teamInfo = request.TeamInfo;
            List<int> characterIds = NormalizeCharacterIds(teamInfo?.CharacterIdList);
            StageGroupDefinition group = StageGroups.Value[state.StageGroupId];
            if (teamInfo is null
                || request.StageGroupId != state.StageGroupId
                || (!request.ResetStageIndex && state.StageProgressIndex >= group.StageIds.Count)
                || !ValidateOwnedTeam(session, characterIds))
            {
                session.SendResponse(new TransfiniteSetTeamResponse { Code = InvalidRequestCode }, packet.Id);
                return;
            }

            if (request.ResetStageIndex)
                ResetRun(state, clearTeam: false);
            state.Team = BuildTeamState(
                characterIds,
                teamInfo.CaptainPos,
                teamInfo.FirstFightPos,
                teamInfo.SelectedGeneralSkill,
                teamInfo.EnterCgIndex,
                teamInfo.SettleCgIndex,
                state.Team);
            state.PendingResult = null;
            session.player.Save();
            session.SendResponse(new TransfiniteSetTeamResponse { Code = SuccessCode }, packet.Id);
        }

        [RequestPacketHandler("TransfiniteConfirmBattleResultRequest")]
        public static void TransfiniteConfirmBattleResultRequestHandler(Session session, Packet.Request packet)
        {
            TransfiniteConfirmBattleResultRequest request = packet.Deserialize<TransfiniteConfirmBattleResultRequest>();
            TransfiniteModeState state = EnsureSessionState(session);
            if (request.StageGroupId != state.StageGroupId)
            {
                session.SendResponse(new TransfiniteConfirmBattleResultResponse { Code = InvalidRequestCode }, packet.Id);
                return;
            }

            Item? changedScoreItem = null;
            if (request.IsGiveUp)
            {
                state.PendingResult = null;
            }
            else if (state.PendingResult is not null)
            {
                StageGroupDefinition group = StageGroups.Value[state.StageGroupId];
                int expectedStageId = (int)CurrentStageId(state, group);
                if (state.PendingResult.LastWinStageId != expectedStageId)
                {
                    session.SendResponse(new TransfiniteConfirmBattleResultResponse { Code = InvalidRequestCode }, packet.Id);
                    return;
                }

                int stageIndex = FindStageIndex(group, expectedStageId);
                int awardedScore = ResolveStageScore(stageIndex, state.PendingResult.StageSpendTime);
                state.StageRecords.RemoveAll(record => record.StageId == expectedStageId);
                state.StageRecords.Add(new TransfiniteStageRecordState
                {
                    StageId = expectedStageId,
                    IsWin = true,
                    SpendTime = state.PendingResult.StageSpendTime,
                    Score = awardedScore
                });
                state.ConfirmedResult = CloneBattleResult(state.PendingResult);
                if (state.Team is not null)
                {
                    state.Team.CharacterResults = state.PendingResult.CharacterResultList
                        .Select(CloneCharacterResult)
                        .ToList();
                }
                state.PendingResult = null;
                state.StageProgressIndex = Math.Min(state.StageProgressIndex + 1, group.StageIds.Count);
                state.MaxRotateStageProgressIndex = Math.Max(
                    state.MaxRotateStageProgressIndex,
                    state.StageProgressIndex);

                int previousScore = state.Score;
                state.Score = Math.Min(RequiredPositiveInt("ScoreLimit"), state.Score + awardedScore);
                int scoreDelta = state.Score - previousScore;
                if (scoreDelta > 0)
                    changedScoreItem = session.inventory.Do(RequiredPositiveInt("ScoreItemId"), scoreDelta);

                if (state.StageProgressIndex >= group.StageIds.Count)
                {
                    int totalSpendTime = state.StageRecords.Where(record => record.IsWin).Sum(record => record.SpendTime);
                    if (totalSpendTime > 0 && (state.BestSpendTime <= 0 || totalSpendTime < state.BestSpendTime))
                        state.BestSpendTime = totalSpendTime;
                    state.Team = null;
                }
            }

            session.player.Save();
            if (changedScoreItem is not null)
            {
                session.inventory.Save();
                session.SendPush(new NotifyItemDataList { ItemDataList = [changedScoreItem] });
            }
            session.SendResponse(new TransfiniteConfirmBattleResultResponse
            {
                Code = SuccessCode,
                BattleInfo = BuildBattleInfo(state),
                RewardGoodsList = []
            }, packet.Id);
        }

        [RequestPacketHandler("TransfiniteResetStageGroupRequest")]
        public static void TransfiniteResetStageGroupRequestHandler(Session session, Packet.Request packet)
        {
            TransfiniteResetStageGroupRequest request = packet.Deserialize<TransfiniteResetStageGroupRequest>();
            TransfiniteModeState state = EnsureSessionState(session);
            if (request.StageGroupId != state.StageGroupId)
            {
                session.SendResponse(new TransfiniteResetStageGroupResponse { Code = InvalidRequestCode }, packet.Id);
                return;
            }

            ResetRun(state, clearTeam: true);
            session.player.Save();
            session.SendResponse(new TransfiniteResetStageGroupResponse
            {
                Code = SuccessCode,
                BattleInfo = BuildBattleInfo(state),
                RewardGoodsList = []
            }, packet.Id);
        }

        [RequestPacketHandler("TransfiniteGetScoreRewardRequest")]
        public static void TransfiniteGetScoreRewardRequestHandler(Session session, Packet.Request packet)
        {
            TransfiniteGetScoreRewardRequest request = packet.Deserialize<TransfiniteGetScoreRewardRequest>();
            TransfiniteModeState state = EnsureSessionState(session);
            List<int> requestedIndexes = (request.ScoreRewardIndex ?? [])
                .Distinct()
                .OrderBy(index => index)
                .ToList();
            bool isValid = requestedIndexes.All(index =>
                index >= 0
                && index < ScoreRewardThresholds.Value.Count
                && state.Score >= ScoreRewardThresholds.Value[index]);
            if (!isValid)
            {
                session.SendResponse(new TransfiniteGetScoreRewardResponse
                {
                    Code = InvalidRequestCode,
                    GotScoreRewardIndex = state.ClaimedScoreRewardIndexes.ToList()
                }, packet.Id);
                return;
            }

            IReadOnlyList<ScoreRewardDefinition> regionRewards = ScoreRewards.Value[state.RegionId];
            List<int> newlyClaimedIndexes = requestedIndexes
                .Where(index => !state.ClaimedScoreRewardIndexes.Contains(index))
                .ToList();
            List<ScoreRewardDefinition> grantedRewards = newlyClaimedIndexes
                .Select(index => regionRewards[index])
                .ToList();
            if (grantedRewards.Any(reward => !Inventory.IsValidClientItemId(reward.TemplateId)))
            {
                session.SendResponse(new TransfiniteGetScoreRewardResponse
                {
                    Code = InvalidRequestCode,
                    GotScoreRewardIndex = state.ClaimedScoreRewardIndexes.ToList()
                }, packet.Id);
                return;
            }

            List<RewardGoods> rewardGoodsList = grantedRewards
                .Select(reward => new RewardGoods
                {
                    Id = reward.Id,
                    TemplateId = reward.TemplateId,
                    Count = reward.Count,
                    RewardType = (int)RewardType.Item
                })
                .ToList();
            List<Item> changedItems = grantedRewards
                .GroupBy(reward => reward.TemplateId)
                .Select(rewards => session.inventory.Do(
                    rewards.Key,
                    checked(rewards.Sum(reward => reward.Count))))
                .ToList();
            state.ClaimedScoreRewardIndexes.AddRange(newlyClaimedIndexes);
            state.ClaimedScoreRewardIndexes.Sort();
            if (newlyClaimedIndexes.Count > 0)
            {
                session.player.Save();
                session.inventory.Save();
                if (changedItems.Count > 0)
                    session.SendPush(new NotifyItemDataList { ItemDataList = changedItems });
            }
            session.SendResponse(new TransfiniteGetScoreRewardResponse
            {
                Code = SuccessCode,
                GotScoreRewardIndex = state.ClaimedScoreRewardIndexes.ToList(),
                RewardGoodsList = rewardGoodsList
            }, packet.Id);
        }

        [RequestPacketHandler("TransfiniteGetRotateSettleInfoRequest")]
        public static void TransfiniteGetRotateSettleInfoRequestHandler(Session session, Packet.Request packet)
        {
            _ = packet.Deserialize<TransfiniteGetRotateSettleInfoRequest>();
            TransfiniteModeState state = EnsureSessionState(session);
            session.SendResponse(new TransfiniteGetRotateSettleInfoResponse
            {
                Code = SuccessCode,
                RewardGoodsList = [],
                MaxStageProgressIndex = state.MaxRotateStageProgressIndex,
                SettleTransfiniteScore = state.Score,
                UnSettleTransfiniteScore = 0
            }, packet.Id);
        }

        private static TransfiniteModeState EnsureSessionState(Session session)
        {
            TransfiniteModeState state = EnsureState(
                session.player,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                out bool cycleReset);
            bool inventoryChanged = SynchronizeScoreItem(session.inventory, state.Score, out _);
            if (cycleReset)
                session.player.Save();
            if (inventoryChanged)
                session.inventory.Save();
            return state;
        }

        private static TransfiniteModeState EnsureState(Player player, long now, out bool cycleReset)
        {
            player.SimulatedBattlefield ??= new SimulatedBattlefieldState();
            player.SimulatedBattlefield.Transfinite ??= new TransfiniteModeState();
            TransfiniteModeState state = player.SimulatedBattlefield.Transfinite;
            RegionDefinition region = ResolveRegion((int)player.PlayerData.Level);
            int cycleSeconds = RequiredPositiveInt("CycleSeconds");
            bool groupBelongsToRegion = region.StageGroupIds.Contains(state.StageGroupId);
            cycleReset = state.ActivityId != RequiredPositiveInt("ActivityId")
                || state.CircleId <= 0
                || state.BeginTime <= 0
                || now < state.BeginTime
                || now >= checked(state.BeginTime + cycleSeconds)
                || state.RegionId != region.Id
                || !groupBelongsToRegion;
            if (cycleReset)
            {
                int nextCircleId = Math.Max(0, state.CircleId) + 1;
                int previousMaxProgress = Math.Max(state.MaxRotateStageProgressIndex, state.StageProgressIndex);
                int groupIndex = (nextCircleId - 1) % region.StageGroupIds.Count;
                state = new TransfiniteModeState
                {
                    ActivityId = RequiredPositiveInt("ActivityId"),
                    BeginTime = Math.Max(1, now - 60),
                    CircleId = nextCircleId,
                    RegionId = region.Id,
                    StageGroupId = region.StageGroupIds[groupIndex],
                    StageProgressIndex = 0,
                    StartStageProgress = 0,
                    MaxRotateStageProgressIndex = previousMaxProgress,
                    Score = 0,
                    StageRecords = [],
                    ClaimedScoreRewardIndexes = []
                };
                player.SimulatedBattlefield.Transfinite = state;
            }

            StageGroupDefinition group = StageGroups.Value[state.StageGroupId];
            state.StageProgressIndex = Math.Clamp(state.StageProgressIndex, 0, group.StageIds.Count);
            state.StartStageProgress = Math.Clamp(state.StartStageProgress, 0, group.StageIds.Count);
            state.MaxRotateStageProgressIndex = Math.Clamp(
                state.MaxRotateStageProgressIndex,
                0,
                group.StageIds.Count);
            state.Score = Math.Clamp(state.Score, 0, RequiredPositiveInt("ScoreLimit"));
            state.StageRecords ??= [];
            state.StageRecords = state.StageRecords
                .Where(record => group.StageIds.Contains(record.StageId))
                .GroupBy(record => record.StageId)
                .Select(records => records.Last())
                .ToList();
            state.ClaimedScoreRewardIndexes ??= [];
            state.ClaimedScoreRewardIndexes = state.ClaimedScoreRewardIndexes
                .Where(index => index >= 0 && index < ScoreRewardThresholds.Value.Count)
                .Distinct()
                .OrderBy(index => index)
                .ToList();
            NormalizeSavedTeam(state);
            if (state.PendingResult is not null && !group.StageIds.Contains(state.PendingResult.LastWinStageId))
                state.PendingResult = null;
            if (state.ConfirmedResult is not null && !group.StageIds.Contains(state.ConfirmedResult.LastWinStageId))
                state.ConfirmedResult = null;
            return state;
        }

        private static void NormalizeSavedTeam(TransfiniteModeState state)
        {
            if (state.Team is null)
                return;
            state.Team.CharacterIds = NormalizeCharacterIds(state.Team.CharacterIds);
            if (state.Team.CharacterIds.All(id => id == 0))
            {
                state.Team = null;
                return;
            }
            state.Team.CharacterResults ??= [];
            Dictionary<int, TransfiniteCharacterResultState> existingResults = state.Team.CharacterResults
                .Where(result => result.CharacterId > 0)
                .GroupBy(result => result.CharacterId)
                .ToDictionary(results => results.Key, results => results.Last());
            state.Team.CharacterResults = state.Team.CharacterIds
                .Where(id => id > 0)
                .Select(id => existingResults.TryGetValue(id, out TransfiniteCharacterResultState? result)
                    ? new TransfiniteCharacterResultState
                    {
                        CharacterId = id,
                        HpPercent = Math.Clamp(result.HpPercent, 0, 100),
                        Energy = Math.Max(0, result.Energy)
                    }
                    : new TransfiniteCharacterResultState { CharacterId = id, HpPercent = 100 })
                .ToList();
            state.Team.CaptainPos = NormalizePosition(state.Team.CaptainPos, state.Team.CharacterIds);
            state.Team.FirstFightPos = NormalizePosition(state.Team.FirstFightPos, state.Team.CharacterIds);
        }

        private static bool SynchronizeScoreItem(Inventory inventory, int score, out Item item)
        {
            int itemId = RequiredPositiveInt("ScoreItemId");
            Item? existing = inventory.Items.FirstOrDefault(candidate => candidate.Id == itemId);
            long existingCount = existing?.Count ?? 0;
            if (existing is not null && existingCount == score)
            {
                item = existing;
                return false;
            }
            item = inventory.Do(itemId, checked(score - (int)existingCount));
            return true;
        }

        private static TransfiniteBattleInfo BuildBattleInfo(TransfiniteModeState state)
        {
            StageGroupDefinition group = StageGroups.Value[state.StageGroupId];
            TransfiniteTeamInfo? teamInfo = state.Team is null
                ? null
                : new TransfiniteTeamInfo
                {
                    CharacterIdList = state.Team.CharacterIds.ToList(),
                    CaptainPos = state.Team.CaptainPos,
                    FirstFightPos = state.Team.FirstFightPos,
                    SelectedGeneralSkill = state.Team.SelectedGeneralSkill,
                    EnterCgIndex = state.Team.EnterCgIndex,
                    SettleCgIndex = state.Team.SettleCgIndex
                };
            TransfiniteBattleResult? result = null;
            if (state.Team is not null)
            {
                result = state.ConfirmedResult is not null
                    ? ToProtocolResult(state.ConfirmedResult)
                    : new TransfiniteBattleResult
                    {
                        LastWinStageId = 0,
                        StageSpendTime = 0,
                        CharacterResultList = state.Team.CharacterResults.Select(ToProtocolCharacterResult).ToList()
                    };
            }

            Dictionary<int, int> stageOrder = group.StageIds
                .Select((stageId, index) => (stageId, index))
                .ToDictionary(pair => pair.stageId, pair => pair.index);
            return new TransfiniteBattleInfo
            {
                StageGroupId = state.StageGroupId,
                TeamInfo = teamInfo,
                StageInfo = state.StageRecords
                    .OrderBy(record => stageOrder.GetValueOrDefault(record.StageId, int.MaxValue))
                    .Select(record => new TransfiniteStageInfo
                    {
                        StageId = record.StageId,
                        IsWin = record.IsWin,
                        SpendTime = record.SpendTime,
                        Score = record.Score
                    })
                    .ToList(),
                StageProgressIndex = state.StageProgressIndex,
                StartStageProgress = state.StartStageProgress,
                Result = result,
                LastResult = state.PendingResult is null ? null : ToProtocolResult(state.PendingResult),
                HistoryResults = []
            };
        }

        private static TransfiniteTeamState BuildTeamState(
            List<int> characterIds,
            int captainPos,
            int firstFightPos,
            int selectedGeneralSkill,
            int enterCgIndex,
            int settleCgIndex,
            TransfiniteTeamState? previousTeam)
        {
            Dictionary<int, TransfiniteCharacterResultState> previousResults = previousTeam?.CharacterResults?
                .Where(result => result.CharacterId > 0)
                .GroupBy(result => result.CharacterId)
                .ToDictionary(results => results.Key, results => results.Last())
                ?? [];
            return new TransfiniteTeamState
            {
                CharacterIds = characterIds,
                CaptainPos = NormalizePosition(captainPos, characterIds),
                FirstFightPos = NormalizePosition(firstFightPos, characterIds),
                SelectedGeneralSkill = Math.Max(0, selectedGeneralSkill),
                EnterCgIndex = Math.Max(0, enterCgIndex),
                SettleCgIndex = Math.Max(0, settleCgIndex),
                CharacterResults = characterIds
                    .Where(id => id > 0)
                    .Select(id => previousResults.TryGetValue(id, out TransfiniteCharacterResultState? result)
                        ? CloneCharacterResult(result)
                        : new TransfiniteCharacterResultState { CharacterId = id, HpPercent = 100 })
                    .ToList()
            };
        }

        private static bool ValidateOwnedTeam(Session session, IReadOnlyList<int> characterIds)
        {
            List<int> positiveIds = characterIds.Where(id => id > 0).ToList();
            if (positiveIds.Count is < 1 or > 3 || positiveIds.Distinct().Count() != positiveIds.Count)
                return false;
            HashSet<uint> ownedIds = session.character.Characters.Select(character => character.Id).ToHashSet();
            return positiveIds.All(id => ownedIds.Contains((uint)id));
        }

        private static List<int> NormalizeCharacterIds(IEnumerable<int>? characterIds)
        {
            List<int> result = (characterIds ?? []).Take(3).Select(id => Math.Max(0, id)).ToList();
            while (result.Count < 3)
                result.Add(0);
            return result;
        }

        private static int NormalizePosition(int position, IReadOnlyList<int> characterIds)
        {
            if (position is >= 1 and <= 3 && characterIds[position - 1] > 0)
                return position;
            int firstOccupiedIndex = characterIds.ToList().FindIndex(id => id > 0);
            return firstOccupiedIndex >= 0 ? firstOccupiedIndex + 1 : 1;
        }

        private static void ResetRun(TransfiniteModeState state, bool clearTeam)
        {
            state.StageProgressIndex = 0;
            state.StartStageProgress = 0;
            state.StageRecords.Clear();
            state.ConfirmedResult = null;
            state.PendingResult = null;
            if (clearTeam)
                state.Team = null;
        }

        private static uint CurrentStageId(TransfiniteModeState state, StageGroupDefinition group)
        {
            int index = Math.Clamp(state.StageProgressIndex, 0, group.StageIds.Count - 1);
            return (uint)group.StageIds[index];
        }

        private static int FindStageIndex(StageGroupDefinition group, int stageId)
        {
            for (int index = 0; index < group.StageIds.Count; index++)
            {
                if (group.StageIds[index] == stageId)
                    return index;
            }
            return -1;
        }

        private static int ResolveStageSpendTime(FightSettleResult result)
        {
            int passTimeLimit = RequiredPositiveInt("PassTimeLimit");
            if (result.LeftTime > 0 && result.LeftTime <= passTimeLimit)
                return Math.Max(1, passTimeLimit - (int)result.LeftTime);

            long activeFrames = Math.Max(
                1,
                result.SettleFrame - result.StartFrame - result.PauseFrame - result.ExSkillPauseFrame);
            return Math.Clamp((int)Math.Ceiling(activeFrames / 60d), 1, passTimeLimit);
        }

        private static int ResolveStageScore(int zeroBasedStageIndex, int spendTime)
        {
            if (zeroBasedStageIndex < 0 || zeroBasedStageIndex >= StageScores.Value.Count)
                return 0;
            int score = StageScores.Value[zeroBasedStageIndex];
            if (ExtraStageIndexes.Value.Contains(zeroBasedStageIndex + 1)
                && spendTime < RequiredPositiveInt("ExtraTimeLimit"))
            {
                score = checked(score + RequiredPositiveInt("ExtraScore"));
            }
            return score;
        }

        private static TransfiniteBattleResult ToProtocolResult(TransfiniteBattleResultState result)
        {
            return new TransfiniteBattleResult
            {
                LastWinStageId = result.LastWinStageId,
                StageSpendTime = result.StageSpendTime,
                CharacterResultList = result.CharacterResultList.Select(ToProtocolCharacterResult).ToList()
            };
        }

        private static TransfiniteCharacterResult ToProtocolCharacterResult(TransfiniteCharacterResultState result)
        {
            return new TransfiniteCharacterResult
            {
                CharacterId = result.CharacterId,
                HpPercent = result.HpPercent,
                Energy = result.Energy
            };
        }

        private static TransfiniteBattleResultState CloneBattleResult(TransfiniteBattleResultState result)
        {
            return new TransfiniteBattleResultState
            {
                LastWinStageId = result.LastWinStageId,
                StageSpendTime = result.StageSpendTime,
                CharacterResultList = result.CharacterResultList.Select(CloneCharacterResult).ToList()
            };
        }

        private static TransfiniteCharacterResultState CloneCharacterResult(TransfiniteCharacterResultState result)
        {
            return new TransfiniteCharacterResultState
            {
                CharacterId = result.CharacterId,
                HpPercent = Math.Clamp(result.HpPercent, 0, 100),
                Energy = Math.Max(0, result.Energy)
            };
        }

        private static RegionDefinition ResolveRegion(int playerLevel)
        {
            return Regions.Value.FirstOrDefault(region => playerLevel >= region.MinLevel && playerLevel <= region.MaxLevel)
                ?? (playerLevel >= Regions.Value.Max(region => region.MinLevel)
                    ? Regions.Value.OrderByDescending(region => region.MaxLevel).First()
                    : Regions.Value.OrderBy(region => region.MinLevel).First());
        }

        private static IReadOnlyList<RegionDefinition> ReadRegions()
        {
            JArray rows = Config.Value["Regions"] as JArray
                ?? throw new InvalidDataException($"{ConfigPath}: Regions must be an array.");
            List<RegionDefinition> result = rows.OfType<JObject>().Select((row, index) => new RegionDefinition
            {
                Id = RequiredPositiveInt(row["Id"], $"Regions[{index}].Id"),
                MinLevel = RequiredPositiveInt(row["MinLevel"], $"Regions[{index}].MinLevel"),
                MaxLevel = RequiredPositiveInt(row["MaxLevel"], $"Regions[{index}].MaxLevel"),
                StageGroupIds = ReadIntArray(row["StageGroupIds"], $"Regions[{index}].StageGroupIds")
            }).ToList();
            if (result.Count == 0 || result.Any(region => region.MaxLevel < region.MinLevel || region.StageGroupIds.Count == 0))
                throw new InvalidDataException($"{ConfigPath}: Regions contains an invalid region definition.");
            return result;
        }

        private static IReadOnlyDictionary<int, IReadOnlyList<ScoreRewardDefinition>> ReadScoreRewards()
        {
            JArray rows = Config.Value["ScoreRewards"] as JArray
                ?? throw new InvalidDataException($"{ConfigPath}: ScoreRewards must be an array.");
            Dictionary<int, IReadOnlyList<ScoreRewardDefinition>> result = [];
            foreach ((JObject row, int rowIndex) in rows.OfType<JObject>().Select((row, index) => (row, index)))
            {
                int regionId = RequiredPositiveInt(row["RegionId"], $"ScoreRewards[{rowIndex}].RegionId");
                JArray goods = row["Goods"] as JArray
                    ?? throw new InvalidDataException($"{ConfigPath}: ScoreRewards[{rowIndex}].Goods must be an array.");
                List<ScoreRewardDefinition> definitions = goods.OfType<JObject>()
                    .Select((reward, rewardIndex) => new ScoreRewardDefinition
                    {
                        Id = RequiredPositiveInt(reward["Id"], $"ScoreRewards[{rowIndex}].Goods[{rewardIndex}].Id"),
                        TemplateId = RequiredPositiveInt(reward["TemplateId"], $"ScoreRewards[{rowIndex}].Goods[{rewardIndex}].TemplateId"),
                        Count = RequiredPositiveInt(reward["Count"], $"ScoreRewards[{rowIndex}].Goods[{rewardIndex}].Count")
                    })
                    .ToList();
                if (definitions.Count != ScoreRewardThresholds.Value.Count)
                    throw new InvalidDataException($"{ConfigPath}: region {regionId} must define exactly {ScoreRewardThresholds.Value.Count} score rewards.");
                if (!result.TryAdd(regionId, definitions))
                    throw new InvalidDataException($"{ConfigPath}: duplicate score-reward region {regionId}.");
            }

            if (Regions.Value.Any(region => !result.ContainsKey(region.Id)))
                throw new InvalidDataException($"{ConfigPath}: every region must define score rewards.");
            return result;
        }

        private static IReadOnlyDictionary<int, StageGroupDefinition> ReadStageGroups()
        {
            JArray rows = Config.Value["StageGroups"] as JArray
                ?? throw new InvalidDataException($"{ConfigPath}: StageGroups must be an array.");
            List<StageGroupDefinition> result = rows.OfType<JObject>().Select((row, index) => new StageGroupDefinition
            {
                Id = RequiredPositiveInt(row["Id"], $"StageGroups[{index}].Id"),
                StageIds = ReadIntArray(row["StageIds"], $"StageGroups[{index}].StageIds"),
                GroupEventIds = ReadIntArray(row["GroupEventIds"], $"StageGroups[{index}].GroupEventIds"),
                NormalEventIds = ReadIntArray(row["NormalEventIds"], $"StageGroups[{index}].NormalEventIds"),
                HiddenEventIds = ReadIntArray(row["HiddenEventIds"], $"StageGroups[{index}].HiddenEventIds"),
                FinalEventIds = ReadIntArray(row["FinalEventIds"], $"StageGroups[{index}].FinalEventIds")
            }).ToList();
            if (result.Count == 0 || result.Any(group => group.StageIds.Count != StageScores.Value.Count))
                throw new InvalidDataException($"{ConfigPath}: every StageGroups entry must contain exactly {StageScores.Value.Count} stages.");
            if (result.SelectMany(group => group.StageIds).Distinct().Count() != result.Sum(group => group.StageIds.Count))
                throw new InvalidDataException($"{ConfigPath}: StageIds must be unique across stage groups.");
            Dictionary<int, StageGroupDefinition> byId = result.ToDictionary(group => group.Id);
            foreach (RegionDefinition region in Regions.Value)
            {
                if (region.StageGroupIds.Any(groupId => !byId.ContainsKey(groupId)))
                    throw new InvalidDataException($"{ConfigPath}: region {region.Id} references an unknown stage group.");
            }
            return byId;
        }

        private static List<int> ReadIntArray(JToken? token, string fieldName, bool requirePositive = true)
        {
            JArray array = token as JArray
                ?? throw new InvalidDataException($"{ConfigPath}: {fieldName} must be an array.");
            List<int> values = array.Select((value, index) =>
            {
                int parsed = value.Value<int>();
                if (requirePositive && parsed <= 0)
                    throw new InvalidDataException($"{ConfigPath}: {fieldName}[{index}] must be positive.");
                if (!requirePositive && parsed < 0)
                    throw new InvalidDataException($"{ConfigPath}: {fieldName}[{index}] must be non-negative.");
                return parsed;
            }).ToList();
            if (values.Count == 0)
                throw new InvalidDataException($"{ConfigPath}: {fieldName} must not be empty.");
            return values;
        }

        private static int RequiredPositiveInt(string fieldName)
        {
            return RequiredPositiveInt(Config.Value[fieldName], fieldName);
        }

        private static int RequiredPositiveInt(JToken? token, string fieldName)
        {
            int value = token?.Value<int>()
                ?? throw new InvalidDataException($"{ConfigPath}: {fieldName} is required.");
            if (value <= 0)
                throw new InvalidDataException($"{ConfigPath}: {fieldName} must be positive.");
            return value;
        }
    }
}
