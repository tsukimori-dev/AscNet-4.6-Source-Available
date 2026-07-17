using AscNet.Common.Database;
using AscNet.Common.MsgPack;
using MessagePack;

namespace AscNet.GameServer.Handlers
{
#pragma warning disable CS8618
    [MessagePackObject(true)]
    public class StrongholdCharacterInfo
    {
        public int Pos { get; set; }
        public int Id { get; set; }
        public long PlayerId { get; set; }
        public int RobotId { get; set; }
        public int Ability { get; set; }
    }

    [MessagePackObject(true)]
    public class StrongholdPluginInfo
    {
        public int Id { get; set; }
        public int Count { get; set; }
    }

    [MessagePackObject(true)]
    public class StrongholdTeamInfo
    {
        public int Id { get; set; }
        public int CaptainPos { get; set; } = 1;
        public int FirstPos { get; set; } = 1;
        public int RuneId { get; set; }
        public int SubRuneId { get; set; }
        public int EnterCgIndex { get; set; }
        public int SettleCgIndex { get; set; }
        public int ElementId { get; set; }
        public int SelectedGeneralSkill { get; set; }
        public List<StrongholdCharacterInfo> CharacterInfos { get; set; } = new();
        public List<StrongholdPluginInfo> PluginInfos { get; set; } = new();
    }

    [MessagePackObject(true)]
    public class SetStrongholdTeamRequest
    {
        public List<StrongholdTeamInfo> TeamInfos { get; set; } = new();
        public bool Own { get; set; }
    }

    [MessagePackObject(true)]
    public class SetStrongholdTeamResponse
    {
        public int Code { get; set; }
    }

    [MessagePackObject(true)]
    public class SetStrongholdFightTeamRequest
    {
        public int Id { get; set; }
        public List<StrongholdTeamInfo> TeamInfos { get; set; } = new();
    }

    [MessagePackObject(true)]
    public class SetStrongholdFightTeamResponse
    {
        public int Code { get; set; }
    }

    [MessagePackObject(true)]
    public class SetStrongholdElectricTeamRequest
    {
        public List<int> CharacterIds { get; set; } = new();
    }

    [MessagePackObject(true)]
    public class SetStrongholdElectricTeamResponse
    {
        public int Code { get; set; }
    }

    [MessagePackObject(true)]
    public class ResetStrongholdGroupRequest
    {
        public int Id { get; set; }
    }

    [MessagePackObject(true)]
    public class ResetStrongholdGroupResponse
    {
        public int Code { get; set; }
    }

    [MessagePackObject(true)]
    public class ResetStrongholdStageRequest
    {
        public int GroupId { get; set; }
        public int StageId { get; set; }
    }

    [MessagePackObject(true)]
    public class ResetStrongholdStageResponse
    {
        public int Code { get; set; }
    }

    [MessagePackObject(true)]
    public class SelectStrongholdLevelRequest
    {
        public int LevelId { get; set; }
    }

    [MessagePackObject(true)]
    public class SelectStrongholdLevelResponse
    {
        public int Code { get; set; }
        public List<NotifyStrongholdLoginData.NotifyStrongholdLoginDataGroupStageData> GroupStageDatas { get; set; } = new();
        public uint ElectricEnergy { get; set; }
        public int Endurance { get; set; }
    }

    [MessagePackObject(true)]
    public class GetStrongholdRewardRequest
    {
        public List<int> Ids { get; set; } = new();
    }

    [MessagePackObject(true)]
    public class GetStrongholdRewardResponse
    {
        public int Code { get; set; }
        public List<int> SuccessIds { get; set; } = new();
        public List<RewardGoods> RewardGoodsList { get; set; } = new();
    }

    [MessagePackObject(true)]
    public class GetStrongholdMineralResponse
    {
        public int Code { get; set; }
        public int MineralCount { get; set; }
    }

    [MessagePackObject(true)]
    public class GetStrongholdAssistCharacterListResponse
    {
        public int Code { get; set; }
        public List<dynamic> CharacterDetails { get; set; } = new();
    }

    [MessagePackObject(true)]
    public class SetStrongholdAssistCharacterRequest
    {
        public int CharacterId { get; set; }
    }

    [MessagePackObject(true)]
    public class SetStrongholdAssistCharacterResponse
    {
        public int Code { get; set; }
    }

    [MessagePackObject(true)]
    public class GetStrongholdLendDetailResponse
    {
        public int Code { get; set; }
        public List<dynamic> LendDayInfos { get; set; } = new();
    }

    [MessagePackObject(true)]
    public class SetStrongholdStayResponse
    {
        public int Code { get; set; }
        public List<int> StayDays { get; set; } = new();
    }

    [MessagePackObject(true)]
    public class SweepStrongholdStageRequest
    {
        public int GroupId { get; set; }
    }

    [MessagePackObject(true)]
    public class StrongholdFightResultInfo
    {
        public int GroupId { get; set; }
        public List<RewardGoods> RewardGoodsList { get; set; } = new();
    }

    [MessagePackObject(true)]
    public class StrongholdFightResult
    {
        public bool AllFinished { get; set; }
        public List<StrongholdFightResultInfo> GroupFightResultInfos { get; set; } = new();
    }

    [MessagePackObject(true)]
    public class SweepStrongholdStageResponse
    {
        public int Code { get; set; }
        public bool IsWin { get; set; }
        public int StageId { get; set; }
        public StrongholdFightResult StrongholdFightResult { get; set; } = new();
    }

    [MessagePackObject(true)]
    public class StrongholdGroupProgress
    {
        public int Id { get; set; }
        public List<int> FinishStageIds { get; set; } = new();
    }

    [MessagePackObject(true)]
    public class StrongholdFinishGroupInfo
    {
        public int Id { get; set; }
        public int UsedElectricEnergy { get; set; }
        public int UsedSystemElectricEnergy { get; set; }
    }

    [MessagePackObject(true)]
    public class NotifyUpdateStrongholdGroupData
    {
        public StrongholdGroupProgress GroupInfo { get; set; } = new();
    }

    [MessagePackObject(true)]
    public class NotifyStrongholdFinishGroupId
    {
        public List<int> FinishGroupIds { get; set; } = new();
        public uint ElectricEnergy { get; set; }
        public List<StrongholdFinishGroupInfo> FinishGroupInfos { get; set; } = new();
        public List<StrongholdFinishGroupInfo> HistoryFinishGroupInfos { get; set; } = new();
    }

    [MessagePackObject(true)]
    public class NotifyStrongholdEnduranceData
    {
        public int Endurance { get; set; }
    }

    internal sealed class StrongholdSettleUpdate
    {
        public int GroupId { get; init; }
        public bool GroupCompletedNow { get; init; }
        public StrongholdFightResult FightResult { get; init; } = new();
    }

    internal static class StrongholdModule
    {
        private const int ActivityId = 126;
        private const int SupportedLevelId = 1;
        private const int InitialElectricEnergy = 2500;
        private const int InitialEndurance = 30;
        private const int MineralItemId = 96002;
        private const long ActivityDurationSeconds = 1_189_800;

        private sealed record StrongholdGroupDefinition(
            int GroupId,
            int[] StageIds,
            int SupportId,
            int RewardId,
            int EnduranceCost,
            int[] PrerequisiteGroupIds);

        private static readonly StrongholdGroupDefinition[] GroupDefinitions =
        [
            new(110, [30202034, 30202036, 30202037, 30202038, 30202039], 133, 62372, 1, []),
            new(111, [30202041, 30202043, 30202044, 30202045, 30202046], 133, 62373, 1, []),
            new(112, [30202048, 30202050, 30202051, 30202052, 30202053], 133, 62374, 1, [110, 111]),
            new(116, [30202076, 30202078, 30202079, 30202080, 30202081], 130, 62375, 1, [112]),
            new(117, [30202083, 30202085, 30202086, 30202087, 30202088], 130, 62376, 1, [112]),
            new(118, [30202090, 30202092, 30202093, 30202094, 30202095], 130, 62377, 1, [116, 117]),
            new(122, [30202118, 30202120, 30202121, 30202122, 30202123], 127, 62378, 1, [118]),
            new(123, [30202125, 30202127, 30202128, 30202129, 30202130], 127, 62379, 1, [118]),
            new(124, [30202132, 30202134, 30202135, 30202136, 30202137], 127, 62380, 1, [122, 123]),
            new(128, [30202160, 30202162, 30202163, 30202164, 30202165], 118, 62381, 1, [124]),
            new(129, [30202167, 30202169, 30202170, 30202171, 30202172], 118, 62382, 1, [124]),
            new(130, [30202174, 30202176, 30202177, 30202178, 30202179], 118, 62383, 1, [128, 129])
        ];

        private static readonly Dictionary<int, StrongholdGroupDefinition> GroupsById =
            GroupDefinitions.ToDictionary(group => group.GroupId);

        private static readonly Dictionary<int, StrongholdGroupDefinition> GroupsByStageId =
            GroupDefinitions
                .SelectMany(group => group.StageIds.Select(stageId => (stageId, group)))
                .ToDictionary(pair => pair.stageId, pair => pair.group);

        private static readonly Dictionary<int, (int RewardTableId, int RequiredGroupId)> RewardDefinitions = new()
        {
            [16] = (62360, 112),
            [17] = (62361, 118),
            [18] = (62362, 124),
            [19] = (62363, 130),
            [20] = (62364, 112),
            [21] = (62365, 112),
            [22] = (62366, 118),
            [23] = (62367, 118),
            [24] = (62368, 124),
            [25] = (62369, 124),
            [26] = (62370, 130),
            [27] = (62371, 130)
        };

        public static NotifyStrongholdLoginData BuildLoginData(Player player, long? nowOverride = null)
        {
            long now = nowOverride ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            StrongholdModeState state = NormalizeState(player, now);
            long activityBeginTime = state.ActivityBeginTime;
            int currentDay = (int)Math.Clamp(((now - activityBeginTime) / 86_400) + 1, 1, 13);

            return new NotifyStrongholdLoginData
            {
                Id = ActivityId,
                BeginTime = (uint)Math.Clamp(activityBeginTime, 0L, (long)uint.MaxValue),
                FightBeginTime = (int)Math.Clamp(activityBeginTime, (long)int.MinValue, (long)int.MaxValue),
                CurDay = currentDay,
                AssistCharacterId = state.AssistCharacterId,
                SetAssistCharacterTime = 0,
                BorrowCount = state.BorrowCount,
                ElectricEnergy = (uint)Math.Max(0, state.ElectricEnergy),
                Endurance = state.Endurance,
                MineralLeft = state.MineralLeft,
                TotalMineral = state.TotalMineral,
                ElectricCharacterIds = state.ElectricCharacterIds.Select(id => (dynamic)id).ToList(),
                FinishGroupIds = state.FinishedGroupIds.Select(id => (dynamic)id).ToList(),
                FinishGroupInfos = BuildFinishGroupInfos(state).Select(info => (dynamic)ToDictionary(info)).ToList(),
                HistoryFinishGroupInfos = [],
                GroupInfos = BuildGroupProgress(state).Select(info => (dynamic)ToDictionary(info)).ToList(),
                TeamInfos = state.Teams.Values
                    .OrderBy(team => team.Id)
                    .Select(team => (dynamic)ToDictionary(ToTeamInfo(team)))
                    .ToList(),
                GroupStageDatas = BuildGroupStageDatas(),
                RuneList = [],
                RewardIds = state.ClaimedRewardIds.Select(id => (dynamic)id).ToList(),
                LastResultRecord = new NotifyStrongholdLoginData.NotifyStrongholdLoginDataLastResultRecord(),
                MineRecords = [],
                LevelId = SupportedLevelId,
                StayDays = state.StayDays.Select(day => (dynamic)day).ToList()
            };
        }

        public static bool IsStage(uint stageId)
        {
            return stageId <= int.MaxValue && GroupsByStageId.ContainsKey((int)stageId);
        }

        public static StrongholdSettleUpdate? RecordStageClear(Session session, int stageId)
        {
            if (!GroupsByStageId.TryGetValue(stageId, out StrongholdGroupDefinition? definition))
                return null;

            StrongholdModeState state = NormalizeState(session.player);
            if (!IsGroupUnlocked(state, definition) || state.Endurance < definition.EnduranceCost)
                return null;

            List<int> progress = GetOrCreateGroupProgress(state, definition.GroupId);
            bool wasFinished = definition.StageIds.All(progress.Contains);
            bool added = false;
            if (!progress.Contains(stageId))
            {
                progress.Add(stageId);
                progress.Sort();
                added = true;
            }

            bool allFinished = definition.StageIds.All(progress.Contains);
            bool completedNow = added && !wasFinished && allFinished;
            List<RewardGoods> rewards = completedNow
                ? CompleteGroup(session, state, definition)
                : [];

            return new StrongholdSettleUpdate
            {
                GroupId = definition.GroupId,
                GroupCompletedNow = completedNow,
                FightResult = BuildFightResult(definition.GroupId, allFinished, rewards)
            };
        }

        public static void SendSettlementPushes(Session session, StrongholdSettleUpdate update)
        {
            StrongholdModeState state = NormalizeState(session.player);
            session.SendPush(new NotifyUpdateStrongholdGroupData
            {
                GroupInfo = BuildGroupProgress(state, update.GroupId)
            });

            if (!update.GroupCompletedNow)
                return;

            session.SendPush(BuildFinishGroupPush(state));
            session.SendPush(new NotifyStrongholdEnduranceData { Endurance = state.Endurance });
        }

        [RequestPacketHandler("SetStrongholdTeamRequest")]
        public static void SetStrongholdTeamRequestHandler(Session session, Packet.Request packet)
        {
            SetStrongholdTeamRequest request = packet.Deserialize<SetStrongholdTeamRequest>();
            if (!ValidateTeamInfos(session, request.TeamInfos))
            {
                session.SendResponse(new SetStrongholdTeamResponse { Code = 1 }, packet.Id);
                return;
            }

            SaveTeams(session.player, request.TeamInfos);
            session.player.Save();
            session.SendResponse(new SetStrongholdTeamResponse { Code = 0 }, packet.Id);
        }

        [RequestPacketHandler("SetStrongholdFightTeamRequest")]
        public static void SetStrongholdFightTeamRequestHandler(Session session, Packet.Request packet)
        {
            SetStrongholdFightTeamRequest request = packet.Deserialize<SetStrongholdFightTeamRequest>();
            StrongholdModeState state = NormalizeState(session.player);
            if (!GroupsById.TryGetValue(request.Id, out StrongholdGroupDefinition? definition)
                || !IsGroupUnlocked(state, definition)
                || state.Endurance < definition.EnduranceCost
                || !ValidateTeamInfos(session, request.TeamInfos))
            {
                session.SendResponse(new SetStrongholdFightTeamResponse { Code = 1 }, packet.Id);
                return;
            }

            SaveTeams(session.player, request.TeamInfos);
            session.player.Save();
            session.SendResponse(new SetStrongholdFightTeamResponse { Code = 0 }, packet.Id);
        }

        [RequestPacketHandler("SetStrongholdElectricTeamRequest")]
        public static void SetStrongholdElectricTeamRequestHandler(Session session, Packet.Request packet)
        {
            SetStrongholdElectricTeamRequest request = packet.Deserialize<SetStrongholdElectricTeamRequest>();
            HashSet<int> owned = session.character.Characters.Select(character => (int)character.Id).ToHashSet();
            List<int> selected = request.CharacterIds.Where(id => id > 0).Distinct().ToList();
            if (selected.Count > 3 || selected.Any(id => !owned.Contains(id)))
            {
                session.SendResponse(new SetStrongholdElectricTeamResponse { Code = 1 }, packet.Id);
                return;
            }

            StrongholdModeState state = NormalizeState(session.player);
            state.ElectricCharacterIds = selected;
            session.player.Save();
            session.SendResponse(new SetStrongholdElectricTeamResponse { Code = 0 }, packet.Id);
        }

        [RequestPacketHandler("ResetStrongholdGroupRequest")]
        public static void ResetStrongholdGroupRequestHandler(Session session, Packet.Request packet)
        {
            ResetStrongholdGroupRequest request = packet.Deserialize<ResetStrongholdGroupRequest>();
            if (!GroupsById.ContainsKey(request.Id))
            {
                session.SendResponse(new ResetStrongholdGroupResponse { Code = 1 }, packet.Id);
                return;
            }

            StrongholdModeState state = NormalizeState(session.player);
            state.GroupFinishStageIds[request.Id] = [];
            session.player.Save();
            session.SendResponse(new ResetStrongholdGroupResponse { Code = 0 }, packet.Id);
        }

        [RequestPacketHandler("ResetStrongholdStageRequest")]
        public static void ResetStrongholdStageRequestHandler(Session session, Packet.Request packet)
        {
            ResetStrongholdStageRequest request = packet.Deserialize<ResetStrongholdStageRequest>();
            if (!GroupsById.TryGetValue(request.GroupId, out StrongholdGroupDefinition? definition)
                || !definition.StageIds.Contains(request.StageId))
            {
                session.SendResponse(new ResetStrongholdStageResponse { Code = 1 }, packet.Id);
                return;
            }

            StrongholdModeState state = NormalizeState(session.player);
            GetOrCreateGroupProgress(state, request.GroupId).Remove(request.StageId);
            session.player.Save();
            session.SendResponse(new ResetStrongholdStageResponse { Code = 0 }, packet.Id);
        }

        [RequestPacketHandler("SelectStrongholdLevelRequest")]
        public static void SelectStrongholdLevelRequestHandler(Session session, Packet.Request packet)
        {
            SelectStrongholdLevelRequest request = packet.Deserialize<SelectStrongholdLevelRequest>();
            if (request.LevelId != SupportedLevelId)
            {
                session.SendResponse(new SelectStrongholdLevelResponse { Code = 1 }, packet.Id);
                return;
            }

            StrongholdModeState state = NormalizeState(session.player);
            state.LevelId = SupportedLevelId;
            session.player.Save();
            session.SendResponse(new SelectStrongholdLevelResponse
            {
                Code = 0,
                GroupStageDatas = BuildGroupStageDatas(),
                ElectricEnergy = (uint)Math.Max(0, state.ElectricEnergy),
                Endurance = state.Endurance
            }, packet.Id);
        }

        [RequestPacketHandler("GetStrongholdRewardRequest")]
        public static void GetStrongholdRewardRequestHandler(Session session, Packet.Request packet)
        {
            GetStrongholdRewardRequest request = packet.Deserialize<GetStrongholdRewardRequest>();
            StrongholdModeState state = NormalizeState(session.player);
            List<int> requestedIds = request.Ids.Distinct().OrderBy(id => id).ToList();

            bool invalid = requestedIds.Count == 0
                || requestedIds.Any(id => !RewardDefinitions.TryGetValue(id, out var definition)
                    || !state.FinishedGroupIds.Contains(definition.RequiredGroupId)
                    || state.ClaimedRewardIds.Contains(id));
            if (invalid)
            {
                session.SendResponse(new GetStrongholdRewardResponse { Code = 1 }, packet.Id);
                return;
            }

            List<AscNet.Table.V2.share.reward.RewardGoodsTable> rewardGoods = requestedIds
                .SelectMany(id => RewardHandler.GetRewardGoods(RewardDefinitions[id].RewardTableId))
                .ToList();
            if (rewardGoods.Count == 0)
            {
                session.SendResponse(new GetStrongholdRewardResponse { Code = 1 }, packet.Id);
                return;
            }

            state.ClaimedRewardIds.AddRange(requestedIds);
            state.ClaimedRewardIds = state.ClaimedRewardIds.Distinct().OrderBy(id => id).ToList();
            List<RewardGoods> rewards = RewardHandler.GiveRewards(rewardGoods, session);
            session.player.Save();
            session.inventory.Save();
            session.character.Save();
            session.SendResponse(new GetStrongholdRewardResponse
            {
                Code = 0,
                SuccessIds = requestedIds,
                RewardGoodsList = rewards
            }, packet.Id);
        }

        [RequestPacketHandler("GetStrongholdMineralRequest")]
        public static void GetStrongholdMineralRequestHandler(Session session, Packet.Request packet)
        {
            StrongholdModeState state = NormalizeState(session.player);
            int count = Math.Max(0, state.MineralLeft);
            state.MineralLeft = 0;
            if (count > 0)
            {
                RewardHandler.GiveRewards(
                    [new Reward { Id = MineralItemId, Count = count, Type = RewardType.Item }],
                    session);
            }
            session.player.Save();
            session.inventory.Save();
            session.SendResponse(new GetStrongholdMineralResponse { Code = 0, MineralCount = count }, packet.Id);
        }

        [RequestPacketHandler("GetStrongholdAssistCharacterListRequest")]
        public static void GetStrongholdAssistCharacterListRequestHandler(Session session, Packet.Request packet)
        {
            session.SendResponse(new GetStrongholdAssistCharacterListResponse { Code = 0 }, packet.Id);
        }

        [RequestPacketHandler("SetStrongholdAssistCharacterRequest")]
        public static void SetStrongholdAssistCharacterRequestHandler(Session session, Packet.Request packet)
        {
            SetStrongholdAssistCharacterRequest request = packet.Deserialize<SetStrongholdAssistCharacterRequest>();
            if (request.CharacterId != 0
                && !session.character.Characters.Any(character => character.Id == request.CharacterId))
            {
                session.SendResponse(new SetStrongholdAssistCharacterResponse { Code = 1 }, packet.Id);
                return;
            }

            StrongholdModeState state = NormalizeState(session.player);
            state.AssistCharacterId = request.CharacterId;
            session.player.Save();
            session.SendResponse(new SetStrongholdAssistCharacterResponse { Code = 0 }, packet.Id);
        }

        [RequestPacketHandler("GetStrongholdLendDetailRequest")]
        public static void GetStrongholdLendDetailRequestHandler(Session session, Packet.Request packet)
        {
            session.SendResponse(new GetStrongholdLendDetailResponse { Code = 0 }, packet.Id);
        }

        [RequestPacketHandler("SetStrongholdStayRequest")]
        public static void SetStrongholdStayRequestHandler(Session session, Packet.Request packet)
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            StrongholdModeState state = NormalizeState(session.player, now);
            int currentDay = (int)Math.Clamp(((now - state.ActivityBeginTime) / 86_400) + 1, 1, 13);
            if (!state.StayDays.Contains(currentDay))
            {
                state.StayDays.Add(currentDay);
                state.StayDays.Sort();
            }
            session.player.Save();
            session.SendResponse(new SetStrongholdStayResponse
            {
                Code = 0,
                StayDays = state.StayDays.ToList()
            }, packet.Id);
        }

        [RequestPacketHandler("SweepStrongholdStageRequest")]
        public static void SweepStrongholdStageRequestHandler(Session session, Packet.Request packet)
        {
            SweepStrongholdStageRequest request = packet.Deserialize<SweepStrongholdStageRequest>();
            StrongholdModeState state = NormalizeState(session.player);
            if (!GroupsById.TryGetValue(request.GroupId, out StrongholdGroupDefinition? definition)
                || !IsGroupUnlocked(state, definition)
                || state.Endurance < definition.EnduranceCost)
            {
                session.SendResponse(new SweepStrongholdStageResponse { Code = 1 }, packet.Id);
                return;
            }

            List<int> progress = GetOrCreateGroupProgress(state, definition.GroupId);
            bool wasFinished = definition.StageIds.All(progress.Contains);
            if (wasFinished)
            {
                session.SendResponse(new SweepStrongholdStageResponse { Code = 1 }, packet.Id);
                return;
            }

            progress.Clear();
            progress.AddRange(definition.StageIds);
            List<RewardGoods> rewards = CompleteGroup(session, state, definition);
            StrongholdSettleUpdate update = new()
            {
                GroupId = definition.GroupId,
                GroupCompletedNow = true,
                FightResult = BuildFightResult(definition.GroupId, true, rewards)
            };

            session.player.Save();
            session.inventory.Save();
            session.character.Save();
            SendSettlementPushes(session, update);
            session.SendResponse(new SweepStrongholdStageResponse
            {
                Code = 0,
                IsWin = true,
                StageId = definition.StageIds[^1],
                StrongholdFightResult = update.FightResult
            }, packet.Id);
        }

        private static StrongholdModeState NormalizeState(Player player, long? nowOverride = null)
        {
            player.StrongholdMode ??= new();
            StrongholdModeState state = player.StrongholdMode;
            state.LevelId = SupportedLevelId;
            if (state.ActivityBeginTime <= 0)
                state.ActivityBeginTime = (nowOverride ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds()) - 86_400;
            if (state.ElectricEnergy <= 0)
                state.ElectricEnergy = InitialElectricEnergy;
            if (state.Endurance < 0)
                state.Endurance = 0;
            state.ElectricCharacterIds ??= new();
            state.FinishedGroupIds ??= new();
            state.GroupFinishStageIds ??= new();
            state.Teams ??= new();
            state.ClaimedRewardIds ??= new();
            state.StayDays ??= new();
            foreach (int groupId in state.GroupFinishStageIds.Keys.ToArray())
                state.GroupFinishStageIds[groupId] ??= new();
            foreach (StrongholdTeamState team in state.Teams.Values)
            {
                team.CharacterInfos ??= new();
                team.PluginInfos ??= new();
            }
            return state;
        }

        private static List<NotifyStrongholdLoginData.NotifyStrongholdLoginDataGroupStageData> BuildGroupStageDatas()
        {
            return GroupDefinitions.Select(definition => new NotifyStrongholdLoginData.NotifyStrongholdLoginDataGroupStageData
            {
                Id = definition.GroupId,
                StageIds = definition.StageIds.Select(stageId => (uint)stageId).ToList(),
                StageBuffId = new Dictionary<dynamic, dynamic>(),
                SupportId = definition.SupportId
            }).ToList();
        }

        private static List<StrongholdGroupProgress> BuildGroupProgress(StrongholdModeState state)
        {
            return state.GroupFinishStageIds
                .Where(pair => GroupsById.ContainsKey(pair.Key) && pair.Value.Count > 0)
                .OrderBy(pair => pair.Key)
                .Select(pair => new StrongholdGroupProgress
                {
                    Id = pair.Key,
                    FinishStageIds = pair.Value.Distinct().OrderBy(id => id).ToList()
                })
                .ToList();
        }

        private static StrongholdGroupProgress BuildGroupProgress(StrongholdModeState state, int groupId)
        {
            return new StrongholdGroupProgress
            {
                Id = groupId,
                FinishStageIds = GetOrCreateGroupProgress(state, groupId).Distinct().OrderBy(id => id).ToList()
            };
        }

        private static List<StrongholdFinishGroupInfo> BuildFinishGroupInfos(StrongholdModeState state)
        {
            return state.FinishedGroupIds
                .Where(GroupsById.ContainsKey)
                .Distinct()
                .OrderBy(id => id)
                .Select(id => new StrongholdFinishGroupInfo { Id = id })
                .ToList();
        }

        private static NotifyStrongholdFinishGroupId BuildFinishGroupPush(StrongholdModeState state)
        {
            return new NotifyStrongholdFinishGroupId
            {
                FinishGroupIds = state.FinishedGroupIds.Distinct().OrderBy(id => id).ToList(),
                ElectricEnergy = (uint)Math.Max(0, state.ElectricEnergy),
                FinishGroupInfos = BuildFinishGroupInfos(state),
                HistoryFinishGroupInfos = []
            };
        }

        private static List<int> GetOrCreateGroupProgress(StrongholdModeState state, int groupId)
        {
            if (!state.GroupFinishStageIds.TryGetValue(groupId, out List<int>? progress))
            {
                progress = new();
                state.GroupFinishStageIds[groupId] = progress;
            }
            return progress;
        }

        private static bool IsGroupUnlocked(StrongholdModeState state, StrongholdGroupDefinition definition)
        {
            return definition.PrerequisiteGroupIds.All(state.FinishedGroupIds.Contains);
        }

        private static List<RewardGoods> CompleteGroup(
            Session session,
            StrongholdModeState state,
            StrongholdGroupDefinition definition)
        {
            if (!state.FinishedGroupIds.Contains(definition.GroupId))
            {
                state.FinishedGroupIds.Add(definition.GroupId);
                state.FinishedGroupIds.Sort();
            }
            state.Endurance = Math.Max(0, state.Endurance - definition.EnduranceCost);
            List<AscNet.Table.V2.share.reward.RewardGoodsTable> rewardGoods = RewardHandler.GetRewardGoods(definition.RewardId);
            return rewardGoods.Count > 0 ? RewardHandler.GiveRewards(rewardGoods, session) : [];
        }

        private static StrongholdFightResult BuildFightResult(int groupId, bool allFinished, List<RewardGoods> rewards)
        {
            return new StrongholdFightResult
            {
                AllFinished = allFinished,
                GroupFightResultInfos = allFinished
                    ? [new StrongholdFightResultInfo { GroupId = groupId, RewardGoodsList = rewards }]
                    : []
            };
        }

        private static bool ValidateTeamInfos(Session session, List<StrongholdTeamInfo> teamInfos)
        {
            if (teamInfos.Count is < 1 or > 5 || teamInfos.Select(team => team.Id).Distinct().Count() != teamInfos.Count)
                return false;

            HashSet<int> ownedCharacterIds = session.character.Characters.Select(character => (int)character.Id).ToHashSet();
            HashSet<int> usedCharacterIds = new();
            foreach (StrongholdTeamInfo team in teamInfos)
            {
                if (team.Id is < 1 or > 5
                    || team.CaptainPos is < 1 or > 3
                    || team.FirstPos is < 1 or > 3
                    || team.CharacterInfos.Count > 3
                    || team.CharacterInfos.Select(member => member.Pos).Distinct().Count() != team.CharacterInfos.Count
                    || team.PluginInfos.Any(plugin => plugin.Id < 0 || plugin.Count < 0))
                    return false;

                HashSet<int> occupiedPositions = team.CharacterInfos
                    .Where(member => member.Id > 0 || member.RobotId > 0)
                    .Select(member => member.Pos)
                    .ToHashSet();
                if (!occupiedPositions.Contains(team.CaptainPos) || !occupiedPositions.Contains(team.FirstPos))
                    return false;

                foreach (StrongholdCharacterInfo member in team.CharacterInfos)
                {
                    if (member.Pos is < 1 or > 3 || member.Id < 0 || member.RobotId < 0)
                        return false;
                    if (member.Id == 0 || member.RobotId > 0)
                        continue;
                    if (member.PlayerId > 0 && member.PlayerId != session.player.PlayerData.Id)
                        return false;
                    if (!ownedCharacterIds.Contains(member.Id) || !usedCharacterIds.Add(member.Id))
                        return false;
                }
            }
            return true;
        }

        private static void SaveTeams(Player player, List<StrongholdTeamInfo> teamInfos)
        {
            StrongholdModeState state = NormalizeState(player);
            state.Teams = teamInfos.ToDictionary(team => team.Id, ToTeamState);
        }

        private static StrongholdTeamState ToTeamState(StrongholdTeamInfo team)
        {
            return new StrongholdTeamState
            {
                Id = team.Id,
                CaptainPos = team.CaptainPos,
                FirstPos = team.FirstPos,
                RuneId = team.RuneId,
                SubRuneId = team.SubRuneId,
                EnterCgIndex = team.EnterCgIndex,
                SettleCgIndex = team.SettleCgIndex,
                ElementId = team.ElementId,
                SelectedGeneralSkill = team.SelectedGeneralSkill,
                CharacterInfos = team.CharacterInfos.Select(member => new StrongholdCharacterState
                {
                    Pos = member.Pos,
                    Id = member.Id,
                    PlayerId = member.PlayerId,
                    RobotId = member.RobotId,
                    Ability = member.Ability
                }).ToList(),
                PluginInfos = team.PluginInfos.Select(plugin => new StrongholdPluginState
                {
                    Id = plugin.Id,
                    Count = plugin.Count
                }).ToList()
            };
        }

        private static StrongholdTeamInfo ToTeamInfo(StrongholdTeamState team)
        {
            return new StrongholdTeamInfo
            {
                Id = team.Id,
                CaptainPos = team.CaptainPos,
                FirstPos = team.FirstPos,
                RuneId = team.RuneId,
                SubRuneId = team.SubRuneId,
                EnterCgIndex = team.EnterCgIndex,
                SettleCgIndex = team.SettleCgIndex,
                ElementId = team.ElementId,
                SelectedGeneralSkill = team.SelectedGeneralSkill,
                CharacterInfos = team.CharacterInfos.Select(member => new StrongholdCharacterInfo
                {
                    Pos = member.Pos,
                    Id = member.Id,
                    PlayerId = member.PlayerId,
                    RobotId = member.RobotId,
                    Ability = member.Ability
                }).ToList(),
                PluginInfos = team.PluginInfos.Select(plugin => new StrongholdPluginInfo
                {
                    Id = plugin.Id,
                    Count = plugin.Count
                }).ToList()
            };
        }

        private static Dictionary<string, object> ToDictionary(StrongholdTeamInfo team)
        {
            return new Dictionary<string, object>
            {
                [nameof(team.Id)] = team.Id,
                [nameof(team.CaptainPos)] = team.CaptainPos,
                [nameof(team.FirstPos)] = team.FirstPos,
                [nameof(team.RuneId)] = team.RuneId,
                [nameof(team.SubRuneId)] = team.SubRuneId,
                [nameof(team.EnterCgIndex)] = team.EnterCgIndex,
                [nameof(team.SettleCgIndex)] = team.SettleCgIndex,
                [nameof(team.ElementId)] = team.ElementId,
                [nameof(team.SelectedGeneralSkill)] = team.SelectedGeneralSkill,
                [nameof(team.CharacterInfos)] = team.CharacterInfos.Select(member => new Dictionary<string, object>
                {
                    [nameof(member.Pos)] = member.Pos,
                    [nameof(member.Id)] = member.Id,
                    [nameof(member.PlayerId)] = member.PlayerId,
                    [nameof(member.RobotId)] = member.RobotId,
                    [nameof(member.Ability)] = member.Ability
                }).ToArray(),
                [nameof(team.PluginInfos)] = team.PluginInfos.Select(plugin => new Dictionary<string, object>
                {
                    [nameof(plugin.Id)] = plugin.Id,
                    [nameof(plugin.Count)] = plugin.Count
                }).ToArray()
            };
        }

        private static Dictionary<string, object> ToDictionary(StrongholdGroupProgress progress)
        {
            return new Dictionary<string, object>
            {
                [nameof(progress.Id)] = progress.Id,
                [nameof(progress.FinishStageIds)] = progress.FinishStageIds.ToArray()
            };
        }

        private static Dictionary<string, object> ToDictionary(StrongholdFinishGroupInfo info)
        {
            return new Dictionary<string, object>
            {
                [nameof(info.Id)] = info.Id,
                [nameof(info.UsedElectricEnergy)] = info.UsedElectricEnergy,
                [nameof(info.UsedSystemElectricEnergy)] = info.UsedSystemElectricEnergy
            };
        }
    }
}
