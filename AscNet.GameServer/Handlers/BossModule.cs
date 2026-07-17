using AscNet.Common.Database;
using AscNet.Common.MsgPack;
using AscNet.Common.Util;
using AscNet.Table.V2.share.reward;
using MessagePack;
using Newtonsoft.Json.Linq;

namespace AscNet.GameServer.Handlers
{
    #region MsgPackScheme
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [MessagePackObject(true)]
    public class BossSingleRankInfoRequest
    {
        public int SectionId { get; set; }
    }

    [MessagePackObject(true)]
    public class BossSingleRankInfoResponse
    {
        public int Code { get; set; }
        public int Rank { get; set; }
        public int TotalRank { get; set; }
    }

    [MessagePackObject(true)]
    public class BossSingleChallengeRankInfoRequest
    {
        public int StageId { get; set; }
    }

    [MessagePackObject(true)]
    public class BossSingleChallengeRankInfoResponse
    {
        public int Code { get; set; }
        public int Rank { get; set; }
        public int TotalRank { get; set; }
    }

    [MessagePackObject(true)]
    public class BossSingleGetChallengeRankRequest
    {
        public int StageId { get; set; }
    }

    [MessagePackObject(true)]
    public class BossSingleGetChallengeRankResponse
    {
        public int Code { get; set; }
        public uint LeftTime { get; set; }
        public int RankNum { get; set; }
        public int Score { get; set; }
        public int HistoryNum { get; set; }
        public int TotalCount { get; set; }
        public List<dynamic> RankList { get; set; } = new();
    }

    [MessagePackObject(true)]
    public class BossSingleGetRankRequest
    {
        public int Level { get; set; }
        public int SectionId { get; set; }
    }

    [MessagePackObject(true)]
    public class BossSingleGetRankResponse
    {
        public int Code { get; set; }
        public uint LeftTime { get; set; }
        public int RankNum { get; set; }
        public int Score { get; set; }
        public int HistoryNum { get; set; }
        public int TotalCount { get; set; }
        public List<dynamic> RankList { get; set; } = new();
    }

    [MessagePackObject(true)]
    public class BossSingleSelectLevelTypeRequest
    {
        public int LevelId { get; set; }
    }

    [MessagePackObject(true)]
    public class BossSingleSelectLevelTypeResponse
    {
        public int Code { get; set; }
        public NotifyFubenBossSingleData.NotifyFubenBossSingleDataFubenBossSingleData FubenBossSingleData { get; set; }
        public Dictionary<int, List<int>> BossListDict { get; set; } = new();
    }

    [MessagePackObject(true)]
    public class BossSingleAutoFightRequest
    {
        public int StageId { get; set; }
    }

    [MessagePackObject(true)]
    public class BossSingleAutoFightResponse
    {
        public int Code { get; set; }
        public int Supply { get; set; }
    }

    [MessagePackObject(true)]
    public class BossSingleSaveScoreRequest
    {
        public int StageId { get; set; }
    }

    [MessagePackObject(true)]
    public class BossSingleSaveScoreResponse
    {
        public int Code { get; set; }
        public int Supply { get; set; }
    }

    [MessagePackObject(true)]
    public class BossSingleResetStageRequest
    {
        public int StageId { get; set; }
    }

    [MessagePackObject(true)]
    public class BossSingleResetStageResponse
    {
        public int Code { get; set; }
        public dynamic? StageRecord { get; set; }
    }

    [MessagePackObject(true)]
    public class BossSingleGetRewardRequest
    {
        public int Id { get; set; }
    }

    [MessagePackObject(true)]
    public class BossSingleGetRewardResponse
    {
        public int Code { get; set; }
        public List<dynamic> RewardGoodsList { get; set; } = new();
    }

    [MessagePackObject(true)]
    public class BossSingleGetAllRewardResponse
    {
        public int Code { get; set; }
        public List<dynamic> RewardGoodsList { get; set; } = new();
    }

    [MessagePackObject(true)]
    public class BossSingleFightResult
    {
        public int StageStatus { get; set; }
        public int MaxTimeScore { get; set; }
        public int TotalScore { get; set; }
        public int MaxBossDamageScore { get; set; }
        public int MaxHpScore { get; set; }
        public int FightTime { get; set; }
        public int BossDamagePer { get; set; }
        public int BossDamageScore { get; set; }
        public int TimeLeft { get; set; }
        public int TimeScore { get; set; }
        public int HpLeftPer { get; set; }
        public int HpScore { get; set; }
    }

    [MessagePackObject(true)]
    public class NotifyBossSingleRankInfo
    {
        public int RankType { get; set; }
        public int Rank { get; set; }
        public int TotalRank { get; set; }
    }

    [MessagePackObject(true)]
    public class GetActivityBossDataRequest
    {
    }

    [MessagePackObject(true)]
    public class GetActivityBossDataResponse
    {
        public int Code { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    #endregion

    internal sealed class BossSinglePendingScore
    {
        public int StageId { get; init; }
        public int SectionId { get; init; }
        public int StageType { get; init; }
        public int Score { get; init; }
        public int StageStatus { get; init; }
        public int BuffGroup { get; init; }
        public BossSingleChallengeBuffGroupData? ChallengeBuffGroup { get; init; }
        public List<int> Characters { get; init; } = new();
        public List<int> Partners { get; init; } = new();
    }

    internal sealed class BossSingleSettleUpdate
    {
        public BossSingleFightResult FightResult { get; init; } = new();
        public bool StateChanged { get; init; }
        public int? CommittedScore { get; init; }
    }

    internal class BossModule
    {
        private const string BattlefieldSnapshotPath = "Configs/simulated_battlefield.json";
        private const int NormalStageType = 1;
        private const int TrialStageType = 2;
        private const int ChallengeStageType = 3;
        private const int BestiaryStageType = 4;
        private const int MaxBossTeamCharacterCount = 3;
        private const int FirstChallengeStatus = 0;
        private const int NonFirstResetStatus = 1;
        private const int NonFirstSameCharactersStatus = 2;
        private const int NonFirstDifferentCharactersStatus = 3;

        private static readonly Lazy<JObject> BattlefieldSnapshot = new(() => JsonSnapshot.LoadObject(BattlefieldSnapshotPath));
        private static readonly Lazy<Dictionary<int, BossStageDefinition>> NormalStages = new(ReadNormalStageDefinitions);
        private static readonly Lazy<BossChallengeDefinition> Challenge = new(ReadChallengeDefinition);

        private sealed class BossStageDefinition
        {
            public int StageId { get; init; }
            public int SectionId { get; init; }
            public int LevelType { get; init; }
            public int BaseScore { get; init; }
            public int BossDamageScore { get; init; }
            public int TimeScore { get; init; }
            public int HpScore { get; init; }
            public bool AutoFight { get; init; }
            public int FightCharCount { get; init; }
            public int MaxScore => checked(BaseScore + BossDamageScore + TimeScore + HpScore);
        }

        private sealed class ScoreRewardDefinition
        {
            public int Id { get; init; }
            public int Score { get; init; }
            public int RewardId { get; init; }
        }

        private sealed class BossChallengeDefinition
        {
            public int SectionConfigId { get; init; }
            public int SectionId { get; init; }
            public int FeatureGroupId { get; init; }
            public int LevelType { get; init; }
            public Dictionary<int, BossChallengeStageDefinition> Stages { get; init; } = new();
            public Dictionary<int, Dictionary<int, List<BossChallengeBuffChoiceDefinition>>> BuffGroups { get; init; } = new();
        }

        private sealed class BossChallengeStageDefinition
        {
            public int StageId { get; init; }
            public int FeatureId { get; init; }
            public int BuffGroupId { get; init; }
            public int BaseScore { get; init; }
            public int BossDamageScore { get; init; }
            public int TimeScore { get; init; }
            public int HpScore { get; init; }
            public int FightCharCount { get; init; }
            public List<int> FightEventIds { get; init; } = new();
            public int MaxScore => checked(BaseScore + BossDamageScore + TimeScore + HpScore);
        }

        private sealed class BossChallengeBuffChoiceDefinition
        {
            public int FeatureId { get; init; }
            public int ScoreRate { get; init; }
            public List<int> FightEventIds { get; init; } = new();
        }

        [RequestPacketHandler("BossSingleRankInfoRequest")]
        public static void BossSingleRankInfoRequestHandler(Session session, Packet.Request packet)
        {
            BossSingleRankInfoRequest request = packet.Deserialize<BossSingleRankInfoRequest>();
            bool knownSection = request.SectionId == 0
                || ReadSectionIds().Contains(request.SectionId)
                || ReadBossListDict().Values.Any(sectionIds => sectionIds.Contains(request.SectionId));
            int score = knownSection ? ResolveCurrentScore(session.player, request.SectionId) : 0;
            session.SendResponse(new BossSingleRankInfoResponse
            {
                Code = knownSection ? 0 : 1,
                Rank = score > 0 ? 1 : 0,
                TotalRank = score > 0 ? 1 : 0
            }, packet.Id);
        }

        [RequestPacketHandler("BossSingleGetRankRequest")]
        public static void BossSingleGetRankRequestHandler(Session session, Packet.Request packet)
        {
            BossSingleGetRankRequest request = packet.Deserialize<BossSingleGetRankRequest>();
            Dictionary<int, List<int>> bossLists = ReadBossListDict();
            bool knownLevel = bossLists.ContainsKey(request.Level);
            bool knownSection = request.SectionId == 0
                || (knownLevel && bossLists[request.Level].Contains(request.SectionId));
            NotifyFubenBossSingleData loginData = BuildLoginData(session.player);
            session.SendResponse(new BossSingleGetRankResponse
            {
                Code = knownLevel && knownSection ? 0 : 1,
                LeftTime = loginData.FubenBossSingleData.RemainTime,
                Score = knownLevel && knownSection
                    ? ResolveCurrentScore(session.player, request.SectionId)
                    : 0,
                RankNum = 0,
                HistoryNum = 0,
                TotalCount = 0,
                RankList = []
            }, packet.Id);
        }

        [RequestPacketHandler("BossSingleChallengeRankInfoRequest")]
        public static void BossSingleChallengeRankInfoRequestHandler(Session session, Packet.Request packet)
        {
            BossSingleChallengeRankInfoRequest request = packet.Deserialize<BossSingleChallengeRankInfoRequest>();
            SimulatedBattlefieldState state = EnsureState(session.player);
            bool unlocked = state.BossChallengeLevelType > 0
                && state.BossChallengeSectionId > 0
                && state.BossChallengeFeatureGroupId > 0;
            bool knownStage = request.StageId == 0 || Challenge.Value.Stages.ContainsKey(request.StageId);
            int score = request.StageId == 0
                ? state.BossChallengeTotalScore
                : state.BossChallengeStageRecords.FirstOrDefault(record => record.StageId == request.StageId)?.Score ?? 0;
            session.SendResponse(new BossSingleChallengeRankInfoResponse
            {
                Code = unlocked && knownStage ? 0 : 1,
                Rank = score > 0 ? 1 : 0,
                TotalRank = score > 0 ? 1 : 0
            }, packet.Id);
        }

        [RequestPacketHandler("BossSingleGetChallengeRankRequest")]
        public static void BossSingleGetChallengeRankRequestHandler(Session session, Packet.Request packet)
        {
            BossSingleGetChallengeRankRequest request = packet.Deserialize<BossSingleGetChallengeRankRequest>();
            SimulatedBattlefieldState state = EnsureState(session.player);
            bool unlocked = IsChallengeUnlocked(state);
            bool knownStage = Challenge.Value.Stages.ContainsKey(request.StageId);
            BossSingleStageRecordState? record = knownStage
                ? state.BossChallengeStageRecords.FirstOrDefault(candidate => candidate.StageId == request.StageId)
                : null;
            List<dynamic> ranks = record is null
                ? []
                : [BuildChallengeRankEntry(session, record)];
            session.SendResponse(new BossSingleGetChallengeRankResponse
            {
                Code = unlocked && knownStage ? 0 : 1,
                LeftTime = BuildLoginData(session.player).FubenBossSingleData.RemainTime,
                RankNum = record is null ? 0 : 1,
                Score = record?.Score ?? 0,
                HistoryNum = record is null ? 0 : 1,
                TotalCount = record is null ? 0 : 1,
                RankList = ranks
            }, packet.Id);
        }

        [RequestPacketHandler("BossSingleSelectLevelTypeRequest")]
        public static void BossSingleSelectLevelTypeRequestHandler(Session session, Packet.Request packet)
        {
            BossSingleSelectLevelTypeRequest request = packet.Deserialize<BossSingleSelectLevelTypeRequest>();
            Dictionary<int, List<int>> bossLists = ReadBossListDict();
            SimulatedBattlefieldState state = EnsureState(session.player);
            bool hasCurrentProgress = state.BossChallengeCount > 0
                || state.BossCurrentStageRecords.Count > 0
                || state.BossCharacterPoints.Count > 0
                || state.BossClaimedRewardIds.Count > 0;
            if (!bossLists.ContainsKey(request.LevelId)
                || (state.BossLevelType != 0 && state.BossLevelType != request.LevelId && hasCurrentProgress))
            {
                session.SendResponse(new BossSingleSelectLevelTypeResponse { Code = 1 }, packet.Id);
                return;
            }

            state.BossLevelType = request.LevelId;
            session.PendingBossSingleScore = null;
            session.player.Save();
            NotifyFubenBossSingleData notification = BuildLoginData(session.player);
            session.SendResponse(new BossSingleSelectLevelTypeResponse
            {
                Code = 0,
                FubenBossSingleData = notification.FubenBossSingleData,
                BossListDict = notification.BossListDict
            }, packet.Id);
        }

        [RequestPacketHandler("BossSingleSaveScoreRequest")]
        public static void BossSingleSaveScoreRequestHandler(Session session, Packet.Request packet)
        {
            BossSingleSaveScoreRequest request = packet.Deserialize<BossSingleSaveScoreRequest>();
            BossSinglePendingScore? pending = session.PendingBossSingleScore;
            if (pending is null || pending.StageId != request.StageId)
            {
                SimulatedBattlefieldState state = EnsureState(session.player);
                bool alreadySaved = state.BossCurrentStageRecords
                        .Any(record => record.StageId == request.StageId && record.Score > 0)
                    || state.BossChallengeStageRecords
                        .Any(record => record.StageId == request.StageId && record.Score > 0);
                session.SendResponse(new BossSingleSaveScoreResponse
                {
                    Code = alreadySaved ? 0 : 1,
                    Supply = 0
                }, packet.Id);
                return;
            }

            List<int> changedChallengeStageIds = [];
            bool committed = pending.StageType == ChallengeStageType
                ? CommitChallengeScore(session, pending, out changedChallengeStageIds)
                : NormalStages.Value.TryGetValue(request.StageId, out BossStageDefinition? stage)
                    && IsActiveNormalStage(session.player, stage)
                    && CommitNormalScore(session, pending);
            if (!committed)
            {
                session.SendResponse(new BossSingleSaveScoreResponse { Code = 1 }, packet.Id);
                return;
            }

            session.PendingBossSingleScore = null;
            session.player.Save();
            session.stage.Save();
            if (pending.StageType == ChallengeStageType)
                SendChallengeStageAndLoginPushes(session, changedChallengeStageIds);
            else
                SendStageAndLoginPushes(session, request.StageId);
            session.SendResponse(new BossSingleSaveScoreResponse
            {
                Code = 0,
                Supply = 0
            }, packet.Id);
        }

        [RequestPacketHandler("BossSingleResetStageRequest")]
        public static void BossSingleResetStageRequestHandler(Session session, Packet.Request packet)
        {
            BossSingleResetStageRequest request = packet.Deserialize<BossSingleResetStageRequest>();
            if (Challenge.Value.Stages.ContainsKey(request.StageId))
            {
                SimulatedBattlefieldState challengeState = EnsureState(session.player);
                BossSingleStageRecordState? challengeRecord = challengeState.BossChallengeStageRecords
                    .FirstOrDefault(record => record.StageId == request.StageId);
                if (challengeRecord is null)
                {
                    session.SendResponse(new BossSingleResetStageResponse { Code = 1 }, packet.Id);
                    return;
                }

                challengeState.BossChallengeStageRecords.Remove(challengeRecord);
                challengeState.BossChallengeTotalScore = challengeState.BossChallengeStageRecords.Sum(record => record.Score);
                if (session.PendingBossSingleScore?.StageId == request.StageId)
                    session.PendingBossSingleScore = null;
                SetStageScore(session, request.StageId, 0);
                session.player.Save();
                session.stage.Save();
                SendChallengeStageAndLoginPushes(session, [request.StageId]);
                session.SendResponse(new BossSingleResetStageResponse
                {
                    Code = 0,
                    StageRecord = BuildRecordPayload(challengeRecord)
                }, packet.Id);
                return;
            }

            if (!NormalStages.Value.TryGetValue(request.StageId, out BossStageDefinition? stage)
                || !IsActiveNormalStage(session.player, stage))
            {
                session.SendResponse(new BossSingleResetStageResponse { Code = 1 }, packet.Id);
                return;
            }

            SimulatedBattlefieldState state = EnsureState(session.player);
            BossSingleStageRecordState? current = state.BossCurrentStageRecords
                .FirstOrDefault(record => record.StageId == request.StageId);
            if (current is null)
            {
                session.SendResponse(new BossSingleResetStageResponse { Code = 1 }, packet.Id);
                return;
            }

            state.BossCurrentStageRecords.Remove(current);
            if (session.PendingBossSingleScore?.StageId == request.StageId)
                session.PendingBossSingleScore = null;
            StageDatum? stageData = null;
            if (session.stage.Stages.TryGetValue(request.StageId, out StageDatum? existingStage))
            {
                existingStage.Score = 0;
                stageData = existingStage;
            }

            session.player.Save();
            session.stage.Save();
            if (stageData is not null)
                session.SendPush(new NotifyStageData { StageList = [stageData] });
            session.SendPush(BuildLoginData(session.player));
            session.SendResponse(new BossSingleResetStageResponse
            {
                Code = 0,
                StageRecord = BuildRecordPayload(current)
            }, packet.Id);
        }

        [RequestPacketHandler("BossSingleAutoFightRequest")]
        public static void BossSingleAutoFightRequestHandler(Session session, Packet.Request packet)
        {
            BossSingleAutoFightRequest request = packet.Deserialize<BossSingleAutoFightRequest>();
            SimulatedBattlefieldState state = EnsureState(session.player);
            if (!NormalStages.Value.TryGetValue(request.StageId, out BossStageDefinition? stage)
                || !stage.AutoFight
                || !IsActiveNormalStage(session.player, stage)
                || state.BossAutoFightCount >= BossConfig.Value<int>("AutoFightCount"))
            {
                session.SendResponse(new BossSingleAutoFightResponse { Code = 1 }, packet.Id);
                return;
            }

            BossSingleStageRecordState? best = state.BossBestStageRecords
                .FirstOrDefault(record => record.StageId == request.StageId);
            if (best is null || best.Score <= 0)
            {
                session.SendResponse(new BossSingleAutoFightResponse { Code = 1 }, packet.Id);
                return;
            }

            int rebate = Math.Clamp(BossConfig.Value<int>("AutoFightRebate"), 0, 100);
            BossSinglePendingScore pending = new()
            {
                StageId = best.StageId,
                SectionId = stage.SectionId,
                StageType = NormalStageType,
                Score = checked((int)((long)best.Score * rebate / 100)),
                StageStatus = DetermineStageStatus(state, stage, best.Characters),
                BuffGroup = best.BuffGroup,
                Characters = best.Characters.ToList(),
                Partners = best.Partners.ToList()
            };
            if (!CommitNormalScore(session, pending))
            {
                session.SendResponse(new BossSingleAutoFightResponse { Code = 1 }, packet.Id);
                return;
            }

            state.BossAutoFightCount++;
            session.player.Save();
            session.stage.Save();
            SendStageAndLoginPushes(session, request.StageId);
            session.SendResponse(new BossSingleAutoFightResponse
            {
                Code = 0,
                Supply = 0
            }, packet.Id);
        }

        [RequestPacketHandler("BossSingleGetRewardRequest")]
        public static void BossSingleGetRewardRequestHandler(Session session, Packet.Request packet)
        {
            BossSingleGetRewardRequest request = packet.Deserialize<BossSingleGetRewardRequest>();
            SimulatedBattlefieldState state = EnsureState(session.player);
            int levelType = ResolveLevelType(state);
            ScoreRewardDefinition? definition = ReadScoreRewards(levelType)
                .FirstOrDefault(reward => reward.Id == request.Id);
            int totalScore = ResolveCurrentScore(session.player, 0);
            if (definition is null
                || state.BossClaimedRewardIds.Contains(request.Id)
                || totalScore < definition.Score)
            {
                session.SendResponse(new BossSingleGetRewardResponse { Code = 1 }, packet.Id);
                return;
            }

            List<dynamic>? rewards = GrantScoreRewards(session, [definition]);
            if (rewards is null)
            {
                session.SendResponse(new BossSingleGetRewardResponse { Code = 1 }, packet.Id);
                return;
            }

            session.SendResponse(new BossSingleGetRewardResponse
            {
                Code = 0,
                RewardGoodsList = rewards
            }, packet.Id);
        }

        [RequestPacketHandler("BossSingleGetAllRewardRequest")]
        public static void BossSingleGetAllRewardRequestHandler(Session session, Packet.Request packet)
        {
            SimulatedBattlefieldState state = EnsureState(session.player);
            int totalScore = ResolveCurrentScore(session.player, 0);
            List<ScoreRewardDefinition> definitions = ReadScoreRewards(ResolveLevelType(state))
                .Where(reward => reward.Score <= totalScore && !state.BossClaimedRewardIds.Contains(reward.Id))
                .OrderBy(reward => reward.Score)
                .ToList();
            if (definitions.Count == 0)
            {
                session.SendResponse(new BossSingleGetAllRewardResponse
                {
                    Code = 0,
                    RewardGoodsList = []
                }, packet.Id);
                return;
            }

            List<dynamic>? rewards = GrantScoreRewards(session, definitions);
            session.SendResponse(new BossSingleGetAllRewardResponse
            {
                Code = rewards is null ? 1 : 0,
                RewardGoodsList = rewards ?? []
            }, packet.Id);
        }

        [RequestPacketHandler("GetActivityBossDataRequest")]
        public static void GetActivityBossDataRequestHandler(Session session, Packet.Request packet)
        {
            session.SendResponse(new GetActivityBossDataResponse { Code = 1 }, packet.Id);
        }

        internal static bool TryPreparePreFight(
            Session session,
            PreFightRequest.PreFightRequestPreFightData preFightData,
            out int errorCode)
        {
            errorCode = 0;
            session.PendingBossSingleScore = null;
            int stageId = checked((int)preFightData.StageId);
            int stageType = ResolveStageType(preFightData.BossSingleStageType, stageId);
            if (stageType == 0)
                return true;
            if (stageType == ChallengeStageType)
            {
                if (!Challenge.Value.Stages.TryGetValue(stageId, out BossChallengeStageDefinition? challengeStage))
                    return RejectPreFight(session, preFightData, "unknown-challenge-stage", out errorCode);
                if (!IsChallengeUnlocked(EnsureState(session.player)))
                    return RejectPreFight(session, preFightData, "challenge-locked", out errorCode);
                if (!TryResolveChallengeBuffs(
                        challengeStage,
                        preFightData.BossSingleChallengeBuffGroup,
                        out _))
                {
                    return RejectPreFight(session, preFightData, "invalid-challenge-buffs", out errorCode);
                }

                List<int> challengeCharacterIds = ReadPreFightCharacterIds(preFightData);
                if (!HasValidOwnedBossTeam(session, challengeCharacterIds))
                    return RejectPreFight(session, preFightData, "invalid-challenge-team", out errorCode);
                return true;
            }
            if (stageType is TrialStageType or BestiaryStageType)
                return true;
            if (stageType != NormalStageType
                || !NormalStages.Value.TryGetValue(stageId, out BossStageDefinition? stage)
                || !IsActiveNormalStage(session.player, stage))
            {
                return RejectPreFight(session, preFightData, "inactive-normal-stage", out errorCode);
            }

            List<int> characterIds = ReadPreFightCharacterIds(preFightData);
            if (!HasValidOwnedBossTeam(session, characterIds))
                return RejectPreFight(session, preFightData, "invalid-normal-team", out errorCode);

            SimulatedBattlefieldState state = EnsureState(session.player);
            int stageStatus = DetermineStageStatus(state, stage, characterIds);
            if (!CanConsumeAttempt(state, stageStatus, characterIds))
                return RejectPreFight(session, preFightData, "attempt-limit", out errorCode);

            if (!state.BossNormalTeams.TryGetValue(stage.SectionId, out List<int>? existingTeam)
                || !existingTeam.SequenceEqual(characterIds))
            {
                state.BossNormalTeams[stage.SectionId] = characterIds;
                session.player.Save();
            }
            return true;
        }

        internal static bool IsFightStage(PreFightRequest.PreFightRequestPreFightData preFightData)
        {
            return ResolveStageType(
                preFightData.BossSingleStageType,
                checked((int)preFightData.StageId)) != 0;
        }

        internal static bool IsFightStage(Session session, uint stageId)
        {
            int requestedStageType = session.fight?.PreFight.PreFightData.BossSingleStageType ?? 0;
            return ResolveStageType(requestedStageType, checked((int)stageId)) != 0;
        }

        internal static void ApplyPreFightData(
            PreFightRequest.PreFightRequestPreFightData preFightData,
            PreFightResponse response)
        {
            if (!IsFightStage(preFightData))
                return;

            if (response.FightData.PassTimeLimit <= 0)
                response.FightData.PassTimeLimit = BossConfig.Value<int>("FightTimeLimit");

            int stageId = checked((int)preFightData.StageId);
            if (ResolveStageType(preFightData.BossSingleStageType, stageId) != ChallengeStageType
                || !Challenge.Value.Stages.TryGetValue(stageId, out BossChallengeStageDefinition? stage))
            {
                return;
            }

            foreach (int eventId in stage.FightEventIds)
                AddFightEvent(response, eventId);
            if (TryResolveChallengeBuffs(stage, preFightData.BossSingleChallengeBuffGroup, out List<BossChallengeBuffChoiceDefinition> selectedBuffs))
            {
                foreach (int eventId in selectedBuffs.SelectMany(buff => buff.FightEventIds))
                    AddFightEvent(response, eventId);
            }
        }

        internal static BossSingleSettleUpdate? RecordFightSettle(Session session, FightSettleRequest request)
        {
            int stageId = checked((int)request.Result.StageId);
            PreFightRequest.PreFightRequestPreFightData? preFightData = session.fight?.PreFight.PreFightData;
            int stageType = ResolveStageType(preFightData?.BossSingleStageType ?? 0, stageId);
            if (stageType == 0)
                return null;
            if (stageType == ChallengeStageType)
                return RecordChallengeFightSettle(session, request, preFightData, stageId);

            int fightTimeLimit = BossConfig.Value<int>("FightTimeLimit");
            int timeLeft = Math.Clamp(checked((int)request.Result.LeftTime), 0, fightTimeLimit);
            if (stageType is TrialStageType or BestiaryStageType)
            {
                RecordStageClear(session.player, stageId);
                return new BossSingleSettleUpdate
                {
                    StateChanged = true,
                    FightResult = new BossSingleFightResult
                    {
                        StageStatus = FirstChallengeStatus,
                        TotalScore = BossConfig.Value<int>("ClearedStageScore"),
                        FightTime = fightTimeLimit - timeLeft,
                        BossDamagePer = 100,
                        TimeLeft = timeLeft,
                        HpLeftPer = 100
                    }
                };
            }

            if (!NormalStages.Value.TryGetValue(stageId, out BossStageDefinition? stage))
                return null;

            List<int> characterIds = (preFightData?.CardIds ?? [])
                .Where(id => id > 0)
                .Select(id => checked((int)id))
                .Distinct()
                .ToList();
            if (characterIds.Count == 0)
            {
                SimulatedBattlefieldState existingState = EnsureState(session.player);
                characterIds = existingState.BossNormalTeams.TryGetValue(stage.SectionId, out List<int>? team)
                    ? team.ToList()
                    : [];
            }

            SimulatedBattlefieldState state = EnsureState(session.player);
            int stageStatus = DetermineStageStatus(state, stage, characterIds);
            int timeScore = stage.TimeScore == 0 || fightTimeLimit <= 0
                ? 0
                : checked((int)Math.Round(
                    (double)stage.TimeScore * timeLeft / fightTimeLimit,
                    MidpointRounding.AwayFromZero));
            int totalScore = checked(stage.BaseScore + stage.BossDamageScore + timeScore + stage.HpScore);
            List<int> partnerIds = session.character.Partners
                .Where(partner => characterIds.Contains(partner.CharacterId))
                .Select(partner => partner.Id)
                .Distinct()
                .ToList();
            BossSinglePendingScore pending = new()
            {
                StageId = stageId,
                SectionId = stage.SectionId,
                StageType = NormalStageType,
                Score = totalScore,
                StageStatus = stageStatus,
                BuffGroup = 0,
                Characters = characterIds,
                Partners = partnerIds
            };
            session.PendingBossSingleScore = pending;

            return new BossSingleSettleUpdate
            {
                StateChanged = false,
                FightResult = new BossSingleFightResult
                {
                    StageStatus = stageStatus,
                    MaxTimeScore = stage.TimeScore,
                    TotalScore = totalScore,
                    MaxBossDamageScore = stage.BossDamageScore,
                    MaxHpScore = stage.HpScore,
                    FightTime = fightTimeLimit - timeLeft,
                    BossDamagePer = 100,
                    BossDamageScore = stage.BossDamageScore,
                    TimeLeft = timeLeft,
                    TimeScore = timeScore,
                    HpLeftPer = 100,
                    HpScore = stage.HpScore
                }
            };
        }

        private static BossSingleSettleUpdate? RecordChallengeFightSettle(
            Session session,
            FightSettleRequest request,
            PreFightRequest.PreFightRequestPreFightData? preFightData,
            int stageId)
        {
            if (preFightData is null
                || !Challenge.Value.Stages.TryGetValue(stageId, out BossChallengeStageDefinition? stage)
                || !TryResolveChallengeBuffs(stage, preFightData.BossSingleChallengeBuffGroup, out List<BossChallengeBuffChoiceDefinition> selectedBuffs))
            {
                return null;
            }

            List<int> characterIds = ReadPreFightCharacterIds(preFightData);
            if (!HasValidOwnedBossTeam(session, characterIds))
                return null;

            int fightTimeLimit = BossConfig.Value<int>("FightTimeLimit");
            int timeLeft = Math.Clamp(checked((int)request.Result.LeftTime), 0, fightTimeLimit);
            int timeScore = stage.TimeScore == 0 || fightTimeLimit <= 0
                ? 0
                : checked((int)Math.Round(
                    (double)stage.TimeScore * timeLeft / fightTimeLimit,
                    MidpointRounding.AwayFromZero));
            int rawScore = checked(stage.BaseScore + stage.BossDamageScore + timeScore + stage.HpScore);
            int scoreRate = checked(selectedBuffs.Sum(buff => buff.ScoreRate));
            int totalScore = checked((int)Math.Round(
                (double)rawScore * (10_000 + scoreRate) / 10_000,
                MidpointRounding.AwayFromZero));
            List<int> partnerIds = session.character.Partners
                .Where(partner => characterIds.Contains(partner.CharacterId))
                .Select(partner => partner.Id)
                .Distinct()
                .ToList();

            session.PendingBossSingleScore = new BossSinglePendingScore
            {
                StageId = stageId,
                SectionId = Challenge.Value.SectionConfigId,
                StageType = ChallengeStageType,
                Score = totalScore,
                StageStatus = FirstChallengeStatus,
                ChallengeBuffGroup = CloneChallengeBuffGroup(preFightData.BossSingleChallengeBuffGroup),
                Characters = characterIds,
                Partners = partnerIds
            };

            return new BossSingleSettleUpdate
            {
                StateChanged = false,
                FightResult = new BossSingleFightResult
                {
                    StageStatus = FirstChallengeStatus,
                    MaxTimeScore = stage.TimeScore,
                    TotalScore = totalScore,
                    MaxBossDamageScore = stage.BossDamageScore,
                    MaxHpScore = stage.HpScore,
                    FightTime = fightTimeLimit - timeLeft,
                    BossDamagePer = 100,
                    BossDamageScore = stage.BossDamageScore,
                    TimeLeft = timeLeft,
                    TimeScore = timeScore,
                    HpLeftPer = 100,
                    HpScore = stage.HpScore
                }
            };
        }

        internal static bool RecordStageClear(Player player, int stageId)
        {
            bool knownSpecialStage = ReadTrialStageIds().Contains(stageId)
                || ReadBestiaryStageIds().Contains(stageId);
            if (!knownSpecialStage)
                return false;

            SimulatedBattlefieldState state = EnsureState(player);
            if (!state.BossSpecialClearedStageIds.Contains(stageId))
                state.BossSpecialClearedStageIds.Add(stageId);
            return true;
        }

        internal static NotifyFubenBossSingleData BuildLoginData(Player player, long? now = null)
        {
            JObject config = BossConfig;
            SimulatedBattlefieldState state = EnsureState(player, now);
            Dictionary<int, List<int>> bossLists = ReadBossListDict();
            int configuredLevelType = config.Value<int>("LevelType");
            int levelType = ResolveLevelType(state);
            if (!bossLists.TryGetValue(levelType, out List<int>? activeSections) || activeSections.Count == 0)
            {
                levelType = configuredLevelType;
                if (!bossLists.TryGetValue(levelType, out activeSections) || activeSections.Count == 0)
                {
                    throw new InvalidDataException(
                        $"{BattlefieldSnapshotPath}: BossSingle.BossListDict must contain a non-empty section list for LevelType {levelType}.");
                }
            }

            HashSet<int> activeSectionSet = activeSections.ToHashSet();
            List<BossSingleStageRecordState> currentRecords = state.BossCurrentStageRecords
                .Where(record => NormalStages.Value.TryGetValue(record.StageId, out BossStageDefinition? stage)
                    && activeSectionSet.Contains(stage.SectionId))
                .OrderBy(record => record.StageId)
                .ToList();
            List<BossSingleStageRecordState> bestRecords = state.BossBestStageRecords
                .Where(record => NormalStages.Value.TryGetValue(record.StageId, out BossStageDefinition? stage)
                    && activeSectionSet.Contains(stage.SectionId))
                .OrderBy(record => record.StageId)
                .ToList();

            uint resultTime = ArenaModule.BuildLoginData(player, now).ResultTime;
            long currentTime = now ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            uint remainTime = resultTime > currentTime
                ? checked((uint)(resultTime - currentTime))
                : 0;
            List<dynamic> trialStages = ReadTrialStageIds()
                .Select(stageId => BuildStageInfo(player, stageId))
                .Cast<dynamic>()
                .ToList();
            List<dynamic> bestiaryStages = ReadBestiaryStageIds()
                .Select(stageId => BuildStageInfo(player, stageId))
                .Cast<dynamic>()
                .ToList();
            List<dynamic> teams = activeSections
                .Select(sectionId => (dynamic)new Dictionary<string, object>
                {
                    ["SectionId"] = sectionId,
                    ["CharacterIds"] = state.BossNormalTeams.TryGetValue(sectionId, out List<int>? team)
                        ? team.ToArray()
                        : Array.Empty<int>()
                })
                .ToList();
            List<ScoreRewardDefinition> scoreRewards = ReadScoreRewards(levelType);
            HashSet<int> validRewardIds = scoreRewards.Select(reward => reward.Id).ToHashSet();
            int totalScore = Math.Max(
                bestRecords.Sum(record => record.Score),
                state.BossChallengeUnlockScore);
            int currentTotalScore = Math.Max(
                currentRecords.Sum(record => record.Score),
                state.BossChallengeUnlockScore);

            return new NotifyFubenBossSingleData
            {
                FubenBossSingleData = new()
                {
                    ActivityNo = config.Value<int>("ActivityNo"),
                    TotalScore = totalScore,
                    MaxScore = NormalStages.Value.Values
                        .Where(stage => activeSectionSet.Contains(stage.SectionId))
                        .Sum(stage => stage.MaxScore),
                    OldLevelType = configuredLevelType,
                    LevelType = levelType,
                    ChallengeCount = state.BossChallengeCount,
                    RemainTime = remainTime,
                    AutoFightCount = state.BossAutoFightCount,
                    CharacterPoints = new Dictionary<int, int>(state.BossCharacterPoints),
                    HistoryList = bestRecords.Select(BuildRecordPayload).Cast<dynamic>().ToList(),
                    RewardIds = state.BossClaimedRewardIds
                        .Where(validRewardIds.Contains)
                        .Cast<dynamic>()
                        .ToList(),
                    RewardGroupId = config.Value<int>("RewardGroupId"),
                    RankPlatform = config.Value<int>("RankPlatform"),
                    BossList = activeSections.ToList(),
                    AfreshId = config.Value<int>("AfreshId"),
                    IsResetOpen = config.Value<bool>("IsResetOpen"),
                    TrialStageInfoList = trialStages,
                    BestiraryStageInfoList = bestiaryStages,
                    StageRecordList = currentRecords.Select(BuildRecordPayload).Cast<dynamic>().ToList(),
                    CurTotalScore = currentTotalScore,
                    ChallengeSectionId = state.BossChallengeSectionId,
                    ChallengeFeatureGroupId = state.BossChallengeFeatureGroupId,
                    ChallengeLevelType = state.BossChallengeLevelType,
                    ChallengeTotalScore = state.BossChallengeTotalScore,
                    ChallengeDeleteRecordTime = 0,
                    ChallengeStageHistoryList = state.BossChallengeStageRecords
                        .OrderBy(record => record.StageId)
                        .Select(BuildRecordPayload)
                        .Cast<dynamic>()
                        .ToList(),
                    NormalStageTeamInfos = teams
                },
                BossListDict = bossLists
            };
        }

        private static List<dynamic>? GrantScoreRewards(
            Session session,
            IReadOnlyCollection<ScoreRewardDefinition> definitions)
        {
            List<(ScoreRewardDefinition Definition, List<RewardGoodsTable> Goods)> rewardRows = definitions
                .Select(definition => (definition, RewardHandler.GetRewardGoods(definition.RewardId)))
                .ToList();
            if (rewardRows.Any(row => row.Item2.Count == 0))
                return null;

            List<dynamic> rewardGoods = RewardHandler.GiveRewards(
                    rewardRows.SelectMany(row => row.Goods),
                    session)
                .Cast<dynamic>()
                .ToList();
            SimulatedBattlefieldState state = EnsureState(session.player);
            foreach (ScoreRewardDefinition definition in definitions)
            {
                if (!state.BossClaimedRewardIds.Contains(definition.Id))
                    state.BossClaimedRewardIds.Add(definition.Id);
            }
            state.BossClaimedRewardIds.Sort();
            session.player.Save();
            session.inventory.Save();
            session.character.Save();
            session.SendPush(BuildLoginData(session.player));
            return rewardGoods;
        }

        private static bool CommitNormalScore(Session session, BossSinglePendingScore pending)
        {
            if (!NormalStages.Value.TryGetValue(pending.StageId, out BossStageDefinition? stage))
                return false;

            SimulatedBattlefieldState state = EnsureState(session.player);
            int stageStatus = DetermineStageStatus(state, stage, pending.Characters);
            if (!CanConsumeAttempt(state, stageStatus, pending.Characters))
                return false;

            bool sectionPreviouslyCleared = IsSectionCleared(state, stage.SectionId);
            if (stageStatus == FirstChallengeStatus && !sectionPreviouslyCleared)
                state.BossChallengeCount++;
            if (stageStatus != NonFirstSameCharactersStatus)
            {
                foreach (int characterId in pending.Characters)
                    state.BossCharacterPoints[characterId] = state.BossCharacterPoints.GetValueOrDefault(characterId) + 1;
            }

            if (!state.BossClearedStageIds.Contains(pending.StageId))
                state.BossClearedStageIds.Add(pending.StageId);
            state.BossClearedStageIds.Sort();
            state.BossNormalTeams[stage.SectionId] = pending.Characters.ToList();

            BossSingleStageRecordState currentRecord = new()
            {
                StageId = pending.StageId,
                Score = pending.Score,
                Characters = pending.Characters.ToList(),
                Partners = pending.Partners.ToList(),
                BuffGroup = pending.BuffGroup
            };
            state.BossCurrentStageRecords.RemoveAll(record => record.StageId == pending.StageId);
            state.BossCurrentStageRecords.Add(currentRecord);
            state.BossCurrentStageRecords = state.BossCurrentStageRecords
                .OrderBy(record => record.StageId)
                .ToList();

            BossSingleStageRecordState? bestRecord = state.BossBestStageRecords
                .FirstOrDefault(record => record.StageId == pending.StageId);
            if (bestRecord is null || pending.Score > bestRecord.Score)
            {
                state.BossBestStageRecords.RemoveAll(record => record.StageId == pending.StageId);
                state.BossBestStageRecords.Add(CloneRecord(currentRecord));
                state.BossBestStageRecords = state.BossBestStageRecords
                    .OrderBy(record => record.StageId)
                    .ToList();
            }

            UpdateStageScore(session, pending.StageId, pending.Score, pending.Characters);
            return true;
        }

        private static bool CommitChallengeScore(
            Session session,
            BossSinglePendingScore pending,
            out List<int> changedStageIds)
        {
            changedStageIds = [];
            if (!Challenge.Value.Stages.TryGetValue(pending.StageId, out BossChallengeStageDefinition? stage)
                || pending.ChallengeBuffGroup is not null
                    && !TryResolveChallengeBuffs(stage, pending.ChallengeBuffGroup, out _))
            {
                return false;
            }

            SimulatedBattlefieldState state = EnsureState(session.player);
            if (!IsChallengeUnlocked(state))
                return false;

            HashSet<int> characterIds = pending.Characters.ToHashSet();
            List<BossSingleStageRecordState> conflictingRecords = state.BossChallengeStageRecords
                .Where(record => record.StageId != pending.StageId && record.Characters.Any(characterIds.Contains))
                .ToList();
            foreach (BossSingleStageRecordState conflict in conflictingRecords)
            {
                state.BossChallengeStageRecords.Remove(conflict);
                SetStageScore(session, conflict.StageId, 0);
                changedStageIds.Add(conflict.StageId);
            }

            state.BossChallengeStageRecords.RemoveAll(record => record.StageId == pending.StageId);
            state.BossChallengeStageRecords.Add(new BossSingleStageRecordState
            {
                StageId = pending.StageId,
                Score = pending.Score,
                Characters = pending.Characters.ToList(),
                Partners = pending.Partners.ToList(),
                ChallengeBuffGroup = ToChallengeBuffGroupState(pending.ChallengeBuffGroup)
            });
            state.BossChallengeStageRecords = state.BossChallengeStageRecords
                .OrderBy(record => record.StageId)
                .ToList();
            state.BossChallengeTotalScore = state.BossChallengeStageRecords.Sum(record => record.Score);
            UpdateStageScore(session, pending.StageId, pending.Score, pending.Characters);
            changedStageIds.Add(pending.StageId);
            changedStageIds = changedStageIds.Distinct().Order().ToList();
            return true;
        }

        private static bool CanConsumeAttempt(
            SimulatedBattlefieldState state,
            int stageStatus,
            IReadOnlyCollection<int> characterIds)
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (stageStatus == FirstChallengeStatus
                && state.BossChallengeCount >= ResolveChallengeLimit(now))
            {
                return false;
            }

            if (stageStatus == NonFirstSameCharactersStatus)
                return true;

            int staminaCount = BossConfig.Value<int>("StaminaCount");
            return characterIds.Count > 0
                && characterIds.All(characterId => state.BossCharacterPoints.GetValueOrDefault(characterId) < staminaCount);
        }

        private static int DetermineStageStatus(
            SimulatedBattlefieldState state,
            BossStageDefinition stage,
            IReadOnlyCollection<int> characterIds)
        {
            BossSingleStageRecordState? current = state.BossCurrentStageRecords
                .FirstOrDefault(record => record.StageId == stage.StageId);
            if (current is not null)
            {
                return SameCharacters(current.Characters, characterIds)
                    ? NonFirstSameCharactersStatus
                    : NonFirstDifferentCharactersStatus;
            }

            return IsSectionCleared(state, stage.SectionId)
                ? NonFirstResetStatus
                : FirstChallengeStatus;
        }

        private static bool IsSectionCleared(SimulatedBattlefieldState state, int sectionId)
        {
            return state.BossClearedStageIds.Any(stageId =>
                NormalStages.Value.TryGetValue(stageId, out BossStageDefinition? stage)
                && stage.SectionId == sectionId);
        }

        private static bool SameCharacters(
            IEnumerable<int> left,
            IEnumerable<int> right)
        {
            return left.Order().SequenceEqual(right.Order());
        }

        private static int ResolveStageType(int requestedStageType, int stageId)
        {
            if (requestedStageType is NormalStageType or TrialStageType or ChallengeStageType or BestiaryStageType)
                return requestedStageType;
            if (NormalStages.Value.ContainsKey(stageId))
                return NormalStageType;
            if (Challenge.Value.Stages.ContainsKey(stageId))
                return ChallengeStageType;
            if (ReadTrialStageIds().Contains(stageId))
                return TrialStageType;
            if (ReadBestiaryStageIds().Contains(stageId))
                return BestiaryStageType;
            return 0;
        }

        private static bool IsActiveNormalStage(Player player, BossStageDefinition stage)
        {
            SimulatedBattlefieldState state = EnsureState(player);
            int levelType = ResolveLevelType(state);
            return stage.LevelType == levelType
                && ReadBossListDict().TryGetValue(levelType, out List<int>? sections)
                && sections.Contains(stage.SectionId);
        }

        private static int ResolveLevelType(SimulatedBattlefieldState state)
        {
            Dictionary<int, List<int>> bossLists = ReadBossListDict();
            int levelType = state.BossLevelType;
            if (!bossLists.ContainsKey(levelType))
                levelType = BossConfig.Value<int>("LevelType");
            return levelType;
        }

        private static int ResolveCurrentScore(Player player, int sectionId)
        {
            SimulatedBattlefieldState state = EnsureState(player);
            IEnumerable<BossSingleStageRecordState> records = state.BossCurrentStageRecords;
            if (sectionId != 0)
            {
                records = records.Where(record =>
                    NormalStages.Value.TryGetValue(record.StageId, out BossStageDefinition? stage)
                    && stage.SectionId == sectionId);
            }
            return records.Sum(record => record.Score);
        }

        private static int ResolveChallengeLimit(long now)
        {
            DayOfWeek day = DateTimeOffset.FromUnixTimeSeconds(now).ToLocalTime().DayOfWeek;
            return day is DayOfWeek.Saturday or DayOfWeek.Sunday
                ? BossConfig.Value<int>("WeekChallengeCount")
                : BossConfig.Value<int>("ChallengeCount");
        }

        private static StageDatum UpdateStageScore(
            Session session,
            int stageId,
            int score,
            IReadOnlyCollection<int> characterIds)
        {
            session.stage.Stages ??= new();
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (!session.stage.Stages.TryGetValue(stageId, out StageDatum? stageData))
            {
                stageData = new StageDatum
                {
                    StageId = stageId,
                    StarsMark = 7,
                    Passed = true,
                    PassTimesTotal = 1,
                    CreateTime = now,
                    LastPassTime = now,
                    RefreshTime = now,
                    BestCardIds = characterIds.Select(id => (long)id).ToList(),
                    LastCardIds = characterIds.Select(id => (long)id).ToList()
                };
                session.stage.AddStage(stageData);
            }

            stageData.Passed = true;
            stageData.Score = score;
            stageData.BestCardIds = characterIds.Select(id => (long)id).ToList();
            stageData.LastCardIds = characterIds.Select(id => (long)id).ToList();
            return stageData;
        }

        private static void SetStageScore(Session session, int stageId, int score)
        {
            if (session.stage.Stages.TryGetValue(stageId, out StageDatum? stageData))
                stageData.Score = score;
        }

        private static void SendStageAndLoginPushes(Session session, int stageId)
        {
            if (session.stage.Stages.TryGetValue(stageId, out StageDatum? stageData))
                session.SendPush(new NotifyStageData { StageList = [stageData] });
            session.SendPush(BuildLoginData(session.player));
        }

        private static void SendChallengeStageAndLoginPushes(Session session, IEnumerable<int> stageIds)
        {
            List<StageDatum> stageData = stageIds
                .Distinct()
                .Select(stageId => session.stage.Stages.TryGetValue(stageId, out StageDatum? value) ? value : null)
                .OfType<StageDatum>()
                .ToList();
            if (stageData.Count > 0)
                session.SendPush(new NotifyStageData { StageList = stageData });
            session.SendPush(BuildLoginData(session.player));
        }

        private static Dictionary<string, object> BuildRecordPayload(BossSingleStageRecordState record)
        {
            return new Dictionary<string, object>
            {
                ["StageId"] = record.StageId,
                ["Score"] = record.Score,
                ["Characters"] = record.Characters.ToArray(),
                ["Partners"] = record.Partners.ToArray(),
                ["BuffGroup"] = record.ChallengeBuffGroup is null
                    ? record.BuffGroup
                    : ToChallengeBuffGroupData(record.ChallengeBuffGroup)
            };
        }

        private static BossSingleStageRecordState CloneRecord(BossSingleStageRecordState record)
        {
            return new BossSingleStageRecordState
            {
                StageId = record.StageId,
                Score = record.Score,
                Characters = record.Characters.ToList(),
                Partners = record.Partners.ToList(),
                BuffGroup = record.BuffGroup,
                ChallengeBuffGroup = CloneChallengeBuffGroupState(record.ChallengeBuffGroup)
            };
        }

        private static List<int> ReadPreFightCharacterIds(PreFightRequest.PreFightRequestPreFightData preFightData)
        {
            return (preFightData.CardIds ?? [])
                .Where(id => id > 0)
                .Select(id => checked((int)id))
                .Distinct()
                .ToList();
        }

        private static bool HasValidOwnedBossTeam(Session session, IReadOnlyCollection<int> characterIds)
        {
            if (characterIds.Count is < 1 or > MaxBossTeamCharacterCount)
                return false;

            HashSet<int> ownedCharacterIds = session.character.Characters
                .Select(character => checked((int)character.Id))
                .ToHashSet();
            return characterIds.All(ownedCharacterIds.Contains);
        }

        private static bool RejectPreFight(
            Session session,
            PreFightRequest.PreFightRequestPreFightData preFightData,
            string reason,
            out int errorCode)
        {
            errorCode = 1;
            string characters = string.Join(',', ReadPreFightCharacterIds(preFightData));
            BossSingleChallengeBuffGroupData? buffGroup = preFightData.BossSingleChallengeBuffGroup;
            string buffs = buffGroup is null
                ? "<null>"
                : $"{buffGroup.BuffGroupId}:[{string.Join(',', (buffGroup.BuffChoices ?? []).OrderBy(pair => pair.Key).Select(pair => $"{pair.Key}={pair.Value}"))}]";
            session.log.Warn(
                $"[BOSS] PreFightRejected reason={reason} stageId={preFightData.StageId} stageType={preFightData.BossSingleStageType} characters=[{characters}] buffs={buffs}");
            return false;
        }

        private static void AddFightEvent(PreFightResponse response, int eventId)
        {
            if (eventId > 0 && !response.FightData.EventIds.Any(value => Convert.ToInt32(value) == eventId))
                response.FightData.EventIds.Add(eventId);
        }

        private static bool IsChallengeUnlocked(SimulatedBattlefieldState state)
        {
            BossChallengeDefinition challenge = Challenge.Value;
            return state.BossChallengeLevelType == challenge.LevelType
                && state.BossChallengeSectionId == challenge.SectionConfigId
                && state.BossChallengeFeatureGroupId == challenge.FeatureGroupId
                && state.BossChallengeUnlockScore > 0;
        }

        private static bool TryResolveChallengeBuffs(
            BossChallengeStageDefinition stage,
            BossSingleChallengeBuffGroupData? buffGroup,
            out List<BossChallengeBuffChoiceDefinition> selectedBuffs)
        {
            selectedBuffs = [];
            if (buffGroup is null || buffGroup.BuffGroupId == 0)
                return true;
            if (buffGroup.BuffGroupId != stage.BuffGroupId
                || !Challenge.Value.BuffGroups.TryGetValue(stage.BuffGroupId, out Dictionary<int, List<BossChallengeBuffChoiceDefinition>>? indexes))
            {
                return false;
            }

            Dictionary<int, int> choices = buffGroup.BuffChoices ?? [];
            if (choices.Keys.Any(index => !indexes.ContainsKey(index)))
                return false;
            foreach ((int index, int choice) in choices.OrderBy(pair => pair.Key))
            {
                if (choice == 0)
                    continue;
                if (!indexes.TryGetValue(index, out List<BossChallengeBuffChoiceDefinition>? options)
                    || choice < 1
                    || choice > options.Count)
                {
                    return false;
                }
                selectedBuffs.Add(options[choice - 1]);
            }
            return true;
        }

        private static BossSingleChallengeBuffGroupData? CloneChallengeBuffGroup(BossSingleChallengeBuffGroupData? value)
        {
            return value is null
                ? null
                : new BossSingleChallengeBuffGroupData
                {
                    BuffGroupId = value.BuffGroupId,
                    BuffChoices = new Dictionary<int, int>(value.BuffChoices ?? [])
                };
        }

        private static BossSingleChallengeBuffGroupState? ToChallengeBuffGroupState(BossSingleChallengeBuffGroupData? value)
        {
            return value is null
                ? null
                : new BossSingleChallengeBuffGroupState
                {
                    BuffGroupId = value.BuffGroupId,
                    BuffChoices = (value.BuffChoices ?? [])
                        .OrderBy(pair => pair.Key)
                        .Select(pair => new BossSingleChallengeBuffChoiceState
                        {
                            Index = pair.Key,
                            Choice = pair.Value
                        })
                        .ToList()
                };
        }

        private static BossSingleChallengeBuffGroupState? CloneChallengeBuffGroupState(BossSingleChallengeBuffGroupState? value)
        {
            return value is null
                ? null
                : new BossSingleChallengeBuffGroupState
                {
                    BuffGroupId = value.BuffGroupId,
                    BuffChoices = value.BuffChoices
                        .Select(choice => new BossSingleChallengeBuffChoiceState
                        {
                            Index = choice.Index,
                            Choice = choice.Choice
                        })
                        .ToList()
                };
        }

        private static BossSingleChallengeBuffGroupData ToChallengeBuffGroupData(BossSingleChallengeBuffGroupState value)
        {
            return new BossSingleChallengeBuffGroupData
            {
                BuffGroupId = value.BuffGroupId,
                BuffChoices = value.BuffChoices.ToDictionary(choice => choice.Index, choice => choice.Choice)
            };
        }

        private static Dictionary<string, object> BuildChallengeRankEntry(
            Session session,
            BossSingleStageRecordState record)
        {
            List<dynamic> characters = record.Characters
                .Select(characterId => session.character.Characters.FirstOrDefault(character => character.Id == characterId))
                .Where(character => character is not null)
                .Select(character => (dynamic)new Dictionary<string, object>
                {
                    ["Id"] = character!.Id,
                    ["LiberateLv"] = character.LiberateLv,
                    ["CharacterHeadInfo"] = new Dictionary<string, object>
                    {
                        ["HeadFashionId"] = character.CharacterHeadInfo?.HeadFashionId ?? 0,
                        ["HeadFashionType"] = character.CharacterHeadInfo?.HeadFashionType ?? 0
                    }
                })
                .ToList();
            return new Dictionary<string, object>
            {
                ["Id"] = session.player.PlayerData.Id,
                ["Name"] = session.player.PlayerData.Name,
                ["HeadPortraitId"] = session.player.PlayerData.CurrHeadPortraitId,
                ["HeadFrameId"] = session.player.PlayerData.CurrHeadFrameId,
                ["RankNum"] = 1,
                ["Score"] = record.Score,
                ["CharacterList"] = characters
            };
        }

        private static Dictionary<string, object> BuildStageInfo(Player player, int stageId)
        {
            SimulatedBattlefieldState state = EnsureState(player);
            bool cleared = state.BossSpecialClearedStageIds.Contains(stageId)
                || (!NormalStages.Value.ContainsKey(stageId) && state.BossClearedStageIds.Contains(stageId));
            return new Dictionary<string, object>
            {
                ["StageId"] = stageId,
                ["Score"] = cleared ? BossConfig.Value<int>("ClearedStageScore") : 0
            };
        }

        private static SimulatedBattlefieldState EnsureState(Player player, long? now = null)
        {
            player.SimulatedBattlefield ??= new();
            SimulatedBattlefieldState state = player.SimulatedBattlefield;
            BossChallengeDefinition challenge = Challenge.Value;
            if (state.BossChallengeLevelType == challenge.LevelType
                && state.BossChallengeSectionId == challenge.SectionId
                && state.BossChallengeFeatureGroupId == challenge.FeatureGroupId)
            {
                // Older unlock data stored the business SectionId here. The client indexes
                // BossSingleSection by the row configuration Id carried by this field.
                state.BossChallengeSectionId = challenge.SectionConfigId;
            }
            state.BossClearedStageIds ??= [];
            state.BossSpecialClearedStageIds ??= [];
            state.BossClaimedRewardIds ??= [];
            state.BossCharacterPoints ??= [];
            state.BossNormalTeams ??= [];
            state.BossCurrentStageRecords ??= [];
            state.BossBestStageRecords ??= [];
            state.BossChallengeStageRecords ??= [];
            state.BossChallengeStageRecords = state.BossChallengeStageRecords
                .Where(record => Challenge.Value.Stages.ContainsKey(record.StageId))
                .OrderBy(record => record.StageId)
                .ToList();
            state.BossChallengeTotalScore = state.BossChallengeStageRecords.Sum(record => record.Score);

            long currentTime = now ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long cycleSeconds = BattlefieldSnapshot.Value.Value<long>("CycleSeconds");
            long baseTime = BattlefieldSnapshot.Value["ArenaActivity"]?.Value<long>("TeamTime")
                ?? throw new InvalidDataException($"{BattlefieldSnapshotPath}: ArenaActivity.TeamTime must be configured.");
            long cycleIndex = currentTime > baseTime ? (currentTime - baseTime) / cycleSeconds : 0;
            if (state.BossCycleIndex == 0)
            {
                state.BossCycleIndex = cycleIndex;
            }
            else if (state.BossCycleIndex != cycleIndex)
            {
                foreach (BossSingleStageRecordState current in state.BossCurrentStageRecords)
                {
                    BossSingleStageRecordState? best = state.BossBestStageRecords
                        .FirstOrDefault(record => record.StageId == current.StageId);
                    if (best is null || current.Score > best.Score)
                    {
                        state.BossBestStageRecords.RemoveAll(record => record.StageId == current.StageId);
                        state.BossBestStageRecords.Add(CloneRecord(current));
                    }
                }

                state.BossCycleIndex = cycleIndex;
                state.BossChallengeCount = 0;
                state.BossAutoFightCount = 0;
                state.BossCharacterPoints.Clear();
                state.BossNormalTeams.Clear();
                state.BossCurrentStageRecords.Clear();
                state.BossChallengeStageRecords.Clear();
                state.BossChallengeTotalScore = 0;
                state.BossClearedStageIds.Clear();
                state.BossSpecialClearedStageIds.Clear();
                state.BossClaimedRewardIds.Clear();
            }
            return state;
        }

        private static BossChallengeDefinition ReadChallengeDefinition()
        {
            if (BossConfig["Challenge"] is not JObject challenge
                || challenge["Stages"] is not JArray stageRows
                || challenge["BuffGroups"] is not JObject buffGroupRows)
            {
                throw new InvalidDataException(
                    $"{BattlefieldSnapshotPath}: BossSingle.Challenge, Stages, and BuffGroups must be configured.");
            }

            Dictionary<int, BossChallengeStageDefinition> stages = new();
            foreach (JObject row in stageRows.OfType<JObject>())
            {
                BossChallengeStageDefinition stage = new()
                {
                    StageId = row.Value<int>("StageId"),
                    FeatureId = row.Value<int>("FeatureId"),
                    BuffGroupId = row.Value<int>("BuffGroupId"),
                    BaseScore = row.Value<int>("BaseScore"),
                    BossDamageScore = row.Value<int>("BossDamageScore"),
                    TimeScore = row.Value<int>("TimeScore"),
                    HpScore = row.Value<int>("HpScore"),
                    FightCharCount = row.Value<int?>("FightCharCount") ?? 1,
                    FightEventIds = ReadPositiveIntArray(
                        row["FightEventIds"],
                        "BossSingle.Challenge.Stages[].FightEventIds")
                };
                if (stage.StageId <= 0
                    || stage.FeatureId <= 0
                    || stage.BuffGroupId <= 0
                    || stage.FightCharCount <= 0
                    || stage.MaxScore <= 0
                    || !stages.TryAdd(stage.StageId, stage))
                {
                    throw new InvalidDataException(
                        $"{BattlefieldSnapshotPath}: BossSingle.Challenge.Stages contains an invalid or duplicate stage.");
                }
            }
            if (stages.Count == 0)
                throw new InvalidDataException($"{BattlefieldSnapshotPath}: BossSingle.Challenge.Stages must not be empty.");

            Dictionary<int, Dictionary<int, List<BossChallengeBuffChoiceDefinition>>> buffGroups = new();
            foreach (JProperty groupProperty in buffGroupRows.Properties())
            {
                if (!int.TryParse(groupProperty.Name, out int buffGroupId)
                    || buffGroupId <= 0
                    || groupProperty.Value is not JArray indexRows)
                {
                    throw new InvalidDataException(
                        $"{BattlefieldSnapshotPath}: BossSingle.Challenge.BuffGroups contains an invalid group.");
                }

                Dictionary<int, List<BossChallengeBuffChoiceDefinition>> indexes = new();
                foreach (JObject indexRow in indexRows.OfType<JObject>())
                {
                    int index = indexRow.Value<int>("Index");
                    if (index <= 0
                        || indexRow["Choices"] is not JArray choiceRows
                        || choiceRows.Count == 0)
                    {
                        throw new InvalidDataException(
                            $"{BattlefieldSnapshotPath}: BossSingle.Challenge.BuffGroups.{buffGroupId} contains an invalid index.");
                    }

                    List<BossChallengeBuffChoiceDefinition> choices = choiceRows
                        .OfType<JObject>()
                        .Select(choice => new BossChallengeBuffChoiceDefinition
                        {
                            FeatureId = choice.Value<int>("FeatureId"),
                            ScoreRate = choice.Value<int>("ScoreRate"),
                            FightEventIds = ReadPositiveIntArray(
                                choice["FightEventIds"],
                                $"BossSingle.Challenge.BuffGroups.{buffGroupId}[{index}].FightEventIds")
                        })
                        .ToList();
                    if (choices.Count != choiceRows.Count
                        || choices.Any(choice => choice.FeatureId <= 0 || choice.ScoreRate < 0)
                        || !indexes.TryAdd(index, choices))
                    {
                        throw new InvalidDataException(
                            $"{BattlefieldSnapshotPath}: BossSingle.Challenge.BuffGroups.{buffGroupId} contains invalid choices.");
                    }
                }
                if (indexes.Count == 0 || !buffGroups.TryAdd(buffGroupId, indexes))
                {
                    throw new InvalidDataException(
                        $"{BattlefieldSnapshotPath}: BossSingle.Challenge.BuffGroups contains an empty or duplicate group.");
                }
            }

            BossChallengeDefinition result = new()
            {
                SectionConfigId = challenge.Value<int>("SectionConfigId"),
                SectionId = challenge.Value<int>("SectionId"),
                FeatureGroupId = challenge.Value<int>("FeatureGroupId"),
                LevelType = challenge.Value<int>("LevelType"),
                Stages = stages,
                BuffGroups = buffGroups
            };
            if (result.SectionConfigId <= 0
                || result.SectionId <= 0
                || result.FeatureGroupId <= 0
                || result.LevelType <= 0
                || result.Stages.Values.Any(stage => !result.BuffGroups.ContainsKey(stage.BuffGroupId)))
            {
                throw new InvalidDataException(
                    $"{BattlefieldSnapshotPath}: BossSingle.Challenge identifiers or stage buff-group references are invalid.");
            }
            return result;
        }

        private static List<int> ReadPositiveIntArray(JToken? token, string fieldName)
        {
            if (token is not JArray values)
                throw new InvalidDataException($"{BattlefieldSnapshotPath}: {fieldName} must be an array.");
            List<int> result = values.Values<int>().ToList();
            if (result.Count == 0 || result.Any(value => value <= 0))
                throw new InvalidDataException($"{BattlefieldSnapshotPath}: {fieldName} must contain positive IDs.");
            return result;
        }

        private static Dictionary<int, BossStageDefinition> ReadNormalStageDefinitions()
        {
            if (BossConfig["NormalSections"] is not JObject sections)
                throw new InvalidDataException($"{BattlefieldSnapshotPath}: BossSingle.NormalSections must be an object.");

            Dictionary<int, List<int>> bossLists = ReadBossListDict();
            Dictionary<int, BossStageDefinition> result = new();
            foreach (JProperty sectionProperty in sections.Properties())
            {
                if (!int.TryParse(sectionProperty.Name, out int sectionId)
                    || sectionProperty.Value is not JArray stages)
                {
                    throw new InvalidDataException(
                        $"{BattlefieldSnapshotPath}: BossSingle.NormalSections contains an invalid section entry.");
                }

                int[] matchingLevels = bossLists
                    .Where(pair => pair.Value.Contains(sectionId))
                    .Select(pair => pair.Key)
                    .ToArray();
                if (matchingLevels.Length != 1)
                {
                    throw new InvalidDataException(
                        $"{BattlefieldSnapshotPath}: section {sectionId} must belong to exactly one BossListDict level.");
                }

                foreach (JObject stage in stages.OfType<JObject>())
                {
                    BossStageDefinition definition = new()
                    {
                        StageId = stage.Value<int>("StageId"),
                        SectionId = sectionId,
                        LevelType = matchingLevels[0],
                        BaseScore = stage.Value<int>("BaseScore"),
                        BossDamageScore = stage.Value<int>("BossDamageScore"),
                        TimeScore = stage.Value<int>("TimeScore"),
                        HpScore = stage.Value<int>("HpScore"),
                        AutoFight = stage.Value<bool>("AutoFight"),
                        FightCharCount = stage.Value<int?>("FightCharCount") ?? 1
                    };
                    if (definition.StageId <= 0
                        || definition.MaxScore <= 0
                        || !result.TryAdd(definition.StageId, definition))
                    {
                        throw new InvalidDataException(
                            $"{BattlefieldSnapshotPath}: BossSingle.NormalSections contains an invalid or duplicate stage.");
                    }
                }
            }
            return result;
        }

        private static List<ScoreRewardDefinition> ReadScoreRewards(int levelType)
        {
            if (BossConfig["ScoreRewards"] is not JObject rewards
                || rewards[levelType.ToString()] is not JArray rows)
            {
                throw new InvalidDataException(
                    $"{BattlefieldSnapshotPath}: BossSingle.ScoreRewards.{levelType} must be an array.");
            }

            return rows.OfType<JObject>()
                .Select(row => new ScoreRewardDefinition
                {
                    Id = row.Value<int>("Id"),
                    Score = row.Value<int>("Score"),
                    RewardId = row.Value<int>("RewardId")
                })
                .OrderBy(row => row.Score)
                .ToList();
        }

        private static IReadOnlyList<int> ReadTrialStageIds()
        {
            return ReadIntArray("TrialStageIds");
        }

        private static IReadOnlyList<int> ReadBestiaryStageIds()
        {
            return ReadIntArray("BestiaryStageIds");
        }

        private static IReadOnlyList<int> ReadSectionIds()
        {
            return ReadIntArray("SectionIds");
        }

        private static IReadOnlyList<int> ReadIntArray(string fieldName)
        {
            return BossConfig[fieldName] is JArray values
                ? values.Select(value => value.Value<int>()).ToArray()
                : throw new InvalidDataException($"{BattlefieldSnapshotPath}: BossSingle.{fieldName} must be an array.");
        }

        private static Dictionary<int, List<int>> ReadBossListDict()
        {
            if (BossConfig["BossListDict"] is not JObject values)
                throw new InvalidDataException($"{BattlefieldSnapshotPath}: BossSingle.BossListDict must be an object.");

            Dictionary<int, List<int>> result = new();
            foreach (JProperty property in values.Properties())
            {
                if (int.TryParse(property.Name, out int levelType) && property.Value is JArray sections)
                    result[levelType] = sections.Values<int>().ToList();
            }
            return result;
        }

        private static JObject BossConfig => BattlefieldSnapshot.Value["BossSingle"] as JObject
            ?? throw new InvalidDataException($"{BattlefieldSnapshotPath}: BossSingle must be an object.");
    }
}
