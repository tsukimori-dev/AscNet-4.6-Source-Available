using MessagePack;
using System.Collections;
using System.Runtime.CompilerServices;
using AscNet.Common.MsgPack;
using AscNet.Common.Util;
using AscNet.Table.V2.share.character;
using AscNet.Table.V2.share.character.quality;
using AscNet.Table.V2.share.character.skill;
using Newtonsoft.Json.Linq;


namespace AscNet.GameServer.Handlers
{
    #region MsgPackScheme
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [MessagePackObject(true)]
    public class BiancaTheatreSelectDifficultyRequest
    {
        public int Difficulty { get; set; }
    }

    [MessagePackObject(true)]
    public class BiancaTheatreSelectDifficultyResponse
    {
        public int ChapterId { get; set; }
        public int Code { get; set; }
    }

    [MessagePackObject(true)]
    public class BiancaTheatreSelectTeamRequest
    {
        public int TeamId { get; set; }
    }

    [MessagePackObject(true)]
    public class BiancaTheatreSelectTeamResponse
    {
        public int Code { get; set; }
    }

    [MessagePackObject(true)]
    public class BiancaTheatreSelectItemRewardRequest
    {
        public int InnerItemId { get; set; }
    }

    [MessagePackObject(true)]
    public class BiancaTheatreSelectRecruitTickRequest
    {
        public int TickId { get; set; }
    }

    [MessagePackObject(true)]
    public class BiancaTheatreRecruitCharacterRequest
    {
        public int CharacterId { get; set; }
    }

    [MessagePackObject(true)]
    public class BiancaTheatreSelectNodeRequest
    {
        public int NodeId { get; set; }
        public int SlotId { get; set; }
    }

    [MessagePackObject(true)]
    public class BiancaTheatreSetSingleTeamRequest
    {
        public dynamic? TeamData { get; set; }
    }

    [MessagePackObject(true)]
    public class BiancaTheatreRecvFightRewardRequest
    {
        public int Uid { get; set; }
    }

    [MessagePackObject(true)]
    public class BiancaTheatreStrengthenRequest
    {
        public int Id { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    #endregion

    internal class BiancaTheatreModule
    {
        private const int CapturedStartChapterId = 1;
        private const int CapturedSelectedItemUid = 75;

        private const string InitialRewardStepJson = """{"ChapterId":1,"Step":{"Uid":340,"StepType":1,"RootUid":0,"Overdue":0,"ItemIds":[26,10,23],"IsExtraReward":1,"SelectedItemId":0,"TickIds":[],"TickId":0,"RefreshCharacterIds":[],"FloorIndexes":[],"RecruitCharacterIds":[],"RefreshCount":0,"RecruitCount":0,"CurRefreshCount":0,"CurRecruitCount":0,"NodeData":null,"FightRewards":[]}}""";
        private const string InitialItemDataAJson = """{"ItemDataList":[{"Id":96119,"Count":3,"BuyTimes":0,"TotalBuyTimes":0,"LastBuyTime":0,"RefreshTime":1713643079,"CreateTime":1713643079},{"Id":96120,"Count":1,"BuyTimes":0,"TotalBuyTimes":0,"LastBuyTime":0,"RefreshTime":1713643079,"CreateTime":1713643079}],"ItemRecycleDict":{}}""";
        private const string InitialItemDataBJson = """{"ItemDataList":[{"Id":96119,"Count":8,"BuyTimes":0,"TotalBuyTimes":0,"LastBuyTime":0,"RefreshTime":1713643079,"CreateTime":1713643079}],"ItemRecycleDict":{}}""";
        private const string InitialItemDataCJson = """{"ItemDataList":[{"Id":96120,"Count":4,"BuyTimes":0,"TotalBuyTimes":0,"LastBuyTime":0,"RefreshTime":1713643079,"CreateTime":1713643079}],"ItemRecycleDict":{}}""";
        private const string PostItemRewardStepJson = """{"ChapterId":1,"Step":{"Uid":341,"StepType":3,"RootUid":0,"Overdue":0,"ItemIds":[],"IsExtraReward":0,"SelectedItemId":0,"TickIds":[1002,2001,3001,4001,5001],"TickId":0,"RefreshCharacterIds":[],"FloorIndexes":[],"RecruitCharacterIds":[],"RefreshCount":0,"RecruitCount":0,"CurRefreshCount":0,"CurRecruitCount":0,"NodeData":null,"FightRewards":[]}}""";
        private const string RecruitCompleteStepJson = """{"ChapterId":1,"Step":{"Uid":343,"StepType":5,"RootUid":0,"Overdue":0,"ItemIds":[],"IsExtraReward":0,"SelectedItemId":0,"TickIds":[],"TickId":0,"RefreshCharacterIds":[],"FloorIndexes":[],"RecruitCharacterIds":[],"RefreshCount":0,"RecruitCount":0,"CurRefreshCount":0,"CurRecruitCount":0,"NodeData":{"NodeId":1,"Slots":[{"SlotId":1,"SlotType":1,"Selected":0,"FightId":10012,"FightTemplateId":11121,"NodeRewards":[{"Uid":1,"RewardType":2,"ConfigId":2003,"Count":0,"Received":0,"TagType":0}],"PassedStageIds":[],"EventId":0,"CurStepId":0,"PassedStepId":[],"ShopId":0,"ShopItems":[]}]},"FightRewards":[]}}""";
        private const string FightRewardStepJson = """{"ChapterId":1,"Step":{"Uid":344,"StepType":6,"RootUid":343,"Overdue":0,"ItemIds":[],"IsExtraReward":0,"SelectedItemId":0,"TickIds":[],"TickId":0,"RefreshCharacterIds":[],"FloorIndexes":[],"RecruitCharacterIds":[],"RefreshCount":0,"RecruitCount":0,"CurRefreshCount":0,"CurRecruitCount":0,"NodeData":null,"FightRewards":[{"Uid":1,"RewardType":2,"ConfigId":2003,"Count":0,"Received":0,"TagType":0},{"Uid":2,"RewardType":3,"ConfigId":16,"Count":4,"Received":0,"TagType":0}]}}""";
        private const string FightRewardItemDataJson = """{"ItemDataList":[{"Id":96119,"Count":12,"BuyTimes":0,"TotalBuyTimes":0,"LastBuyTime":0,"RefreshTime":1713643079,"CreateTime":1713643079}],"ItemRecycleDict":{}}""";
        private const string SecondFightRewardStepJson = """{"ChapterId":1,"Step":{"Uid":345,"StepType":4,"RootUid":344,"Overdue":0,"ItemIds":[],"IsExtraReward":0,"SelectedItemId":0,"TickIds":[],"TickId":2003,"RefreshCharacterIds":[],"FloorIndexes":[],"RecruitCharacterIds":[],"RefreshCount":2,"RecruitCount":1,"CurRefreshCount":0,"CurRecruitCount":0,"NodeData":null,"FightRewards":[]}}""";
        private const string EndRewardStepJson = """{"ChapterId":1,"Step":{"Uid":346,"StepType":5,"RootUid":0,"Overdue":0,"ItemIds":[],"IsExtraReward":0,"SelectedItemId":0,"TickIds":[],"TickId":0,"RefreshCharacterIds":[],"FloorIndexes":[],"RecruitCharacterIds":[],"RefreshCount":0,"RecruitCount":0,"CurRefreshCount":0,"CurRecruitCount":0,"NodeData":{"NodeId":2,"Slots":[{"SlotId":1,"SlotType":1,"Selected":0,"FightId":10011,"FightTemplateId":11111,"NodeRewards":[{"Uid":1,"RewardType":2,"ConfigId":4003,"Count":0,"Received":0,"TagType":0}],"PassedStageIds":[],"EventId":0,"CurStepId":0,"PassedStepId":[],"ShopId":0,"ShopItems":[]},{"SlotId":2,"SlotType":1,"Selected":0,"FightId":10012,"FightTemplateId":11123,"NodeRewards":[{"Uid":1,"RewardType":2,"ConfigId":2003,"Count":0,"Received":0,"TagType":0}],"PassedStageIds":[],"EventId":0,"CurStepId":0,"PassedStepId":[],"ShopId":0,"ShopItems":[]}]},"FightRewards":[]}}""";
        private const string TotalExpJson = """{"TotalExp":5576}""";
        private const string SettleAdventureResponseJson = """{"Code":0,"SettleData":{"NodeCount":1,"FightNodeCount":1,"TotalCharacterLevel":2,"TotalItemCount":1,"ChapterCount":0,"NodeCountScore":3,"FightNodeCountScore":6,"TotalCharacterLevelScore":4,"TotalItemCountScore":1,"ChapterCountScore":0,"TotalScore":14,"EndId":1,"EndFactor":"1.0","DifficultyFactor":"1.0","TotalExp":14,"OutItemCount":14,"TeamId":3,"Characters":[{"CharacterId":1011002,"Level":2,"IsDecay":0}],"Items":[{"Uid":75,"ItemId":26}],"UnlockItemId":[3,21,26,27,31,33,35,40,45,55,58,64,65,67,73,74,75,91,93,99,100,104,105,106,122,123,124,125,126,127,128,129,130,131,132,133,134,135,136,137,138,139,143,77,78,94,95,96,97,98,101,102,103,113,22,28,79,80],"UnlockTeamId":[3,5,4,6],"UnlockDifficultyId":[2,3],"TeamRecords":[{"TeamId":2,"EndRecords":[2]},{"TeamId":3,"EndRecords":[3,5]}],"PassChapterIds":[7,8,9,10,1,2,3,4,5,13],"HistoryTotalItemCount":75,"HistoryTotalPassFightNodeCount":45,"HistoryItemObtainRecords":{"3":26,"4":35,"5":14}}}""";
        private const string StrengthenResponseJson = """{"Code":20176035}""";

        private static readonly byte[] InitialRewardStepPayload = SerializeJsonPayload(InitialRewardStepJson);
        private static readonly byte[] InitialItemDataAPayload = SerializeJsonPayload(InitialItemDataAJson);
        private static readonly byte[] InitialItemDataBPayload = SerializeJsonPayload(InitialItemDataBJson);
        private static readonly byte[] InitialItemDataCPayload = SerializeJsonPayload(InitialItemDataCJson);
        private static readonly byte[] PostItemRewardStepPayload = SerializeJsonPayload(PostItemRewardStepJson);
        private static readonly byte[] FightRewardStepPayload = SerializeJsonPayload(FightRewardStepJson);
        private static readonly byte[] FightRewardItemDataPayload = SerializeJsonPayload(FightRewardItemDataJson);
        private static readonly byte[] TotalExpPayload = SerializeJsonPayload(TotalExpJson);
        private static readonly byte[] StrengthenResponsePayload = SerializeJsonPayload(StrengthenResponseJson);

        private sealed class TheatreSessionState
        {
            public int TeamId { get; set; }
            public int? SelectedItemId { get; set; }
            public int SelectedItemUid { get; set; } = CapturedSelectedItemUid;
            public int? RecruitTickId { get; set; }
            public int[] RefreshCharacterIds { get; set; } = [];
            public List<int> RecruitedCharacterIds { get; } = [];
            public List<int> TeamCharacterIds { get; } = [];
            public List<int> TeamRobotIds { get; } = [];
            public HashSet<uint> FightStageIds { get; } = [];
            public uint? ActiveFightStageId { get; set; }
            public int FightNodeCount { get; set; }
            public bool PostFightRecruitPending { get; set; }
        }

        private static readonly ConditionalWeakTable<Session, TheatreSessionState> SessionStates = new();
        private static readonly JObject FightStageSnapshots = JsonSnapshot.LoadObject("Configs/bianca_theatre_fight_stages.json");

        [RequestPacketHandler("BiancaTheatreSelectDifficultyRequest")]
        public static void BiancaTheatreSelectDifficultyRequestHandler(Session session, Packet.Request packet)
        {
            _ = packet.Deserialize<BiancaTheatreSelectDifficultyRequest>();
            session.SendResponse(new BiancaTheatreSelectDifficultyResponse
            {
                ChapterId = CapturedStartChapterId,
                Code = 0
            }, packet.Id);
        }

        [RequestPacketHandler("BiancaTheatreSelectTeamRequest")]
        public static void BiancaTheatreSelectTeamRequestHandler(Session session, Packet.Request packet)
        {
            BiancaTheatreSelectTeamRequest request = packet.Deserialize<BiancaTheatreSelectTeamRequest>();
            SessionStates.Remove(session);
            GetState(session).TeamId = request.TeamId;

            session.SendPush("NotifyBiancaTheatreAddStep", InitialRewardStepPayload);
            session.SendPush("NotifyItemDataList", InitialItemDataAPayload);
            session.SendPush("NotifyItemDataList", InitialItemDataBPayload);
            session.SendPush("NotifyItemDataList", InitialItemDataCPayload);
            session.SendResponse(new BiancaTheatreSelectTeamResponse
            {
                Code = 0
            }, packet.Id);
        }

        [RequestPacketHandler("BiancaTheatreSelectItemRewardRequest")]
        public static void BiancaTheatreSelectItemRewardRequestHandler(Session session, Packet.Request packet)
        {
            BiancaTheatreSelectItemRewardRequest request = packet.Deserialize<BiancaTheatreSelectItemRewardRequest>();
            TheatreSessionState state = GetState(session);
            state.SelectedItemId = request.InnerItemId;

            session.SendPush("NotifyBiancaTheatreAddItem", SerializePayload(new Dictionary<string, object?>
            {
                ["BiancaTheatreItems"] = new object[]
                {
                    new Dictionary<string, object?>
                    {
                        ["Uid"] = state.SelectedItemUid,
                        ["ItemId"] = request.InnerItemId
                    }
                }
            }));
            session.SendPush("NotifyBiancaTheatreAddStep", PostItemRewardStepPayload);
            session.SendResponse("BiancaTheatreSelectItemRewardResponse", SerializePayload(new Dictionary<string, object?>
            {
                ["InnerItemId"] = request.InnerItemId,
                ["Code"] = 0
            }), packet.Id);
        }

        [RequestPacketHandler("BiancaTheatreSelectRecruitTickRequest")]
        public static void BiancaTheatreSelectRecruitTickRequestHandler(Session session, Packet.Request packet)
        {
            BiancaTheatreSelectRecruitTickRequest request = packet.Deserialize<BiancaTheatreSelectRecruitTickRequest>();
            TheatreSessionState state = GetState(session);
            state.RecruitTickId = request.TickId;
            state.RefreshCharacterIds = BuildRecruitChoices(request.TickId);

            session.SendResponse("BiancaTheatreSelectRecruitTickResponse", SerializePayload(new Dictionary<string, object?>
            {
                ["Step"] = new Dictionary<string, object?>
                {
                    ["Uid"] = 342,
                    ["StepType"] = 4,
                    ["RootUid"] = 341,
                    ["Overdue"] = 0,
                    ["ItemIds"] = Array.Empty<object>(),
                    ["IsExtraReward"] = 0,
                    ["SelectedItemId"] = 0,
                    ["TickIds"] = Array.Empty<object>(),
                    ["TickId"] = request.TickId,
                    ["RefreshCharacterIds"] = state.RefreshCharacterIds,
                    ["FloorIndexes"] = Enumerable.Repeat(0, state.RefreshCharacterIds.Length).ToArray(),
                    ["RecruitCharacterIds"] = state.RecruitedCharacterIds.ToArray(),
                    ["RefreshCount"] = 2,
                    ["RecruitCount"] = 1,
                    ["CurRefreshCount"] = 0,
                    ["CurRecruitCount"] = state.RecruitedCharacterIds.Count,
                    ["NodeData"] = null,
                    ["FightRewards"] = Array.Empty<object>()
                },
                ["Code"] = 0
            }), packet.Id);
        }

        [RequestPacketHandler("BiancaTheatreRecruitCharacterRequest")]
        public static void BiancaTheatreRecruitCharacterRequestHandler(Session session, Packet.Request packet)
        {
            BiancaTheatreRecruitCharacterRequest request = packet.Deserialize<BiancaTheatreRecruitCharacterRequest>();
            TheatreSessionState state = GetState(session);
            bool validCharacter = TableReaderV2.Parse<CharacterTable>().Any(character => character.Id == request.CharacterId);
            if (validCharacter && !state.RecruitedCharacterIds.Contains(request.CharacterId))
                state.RecruitedCharacterIds.Add(request.CharacterId);

            session.SendResponse("BiancaTheatreRecruitCharacterResponse", BuildCodeResponse(validCharacter ? 0 : 20009021), packet.Id);
        }

        [RequestPacketHandler("BiancaTheatreEndRecruitRequest")]
        public static void BiancaTheatreEndRecruitRequestHandler(Session session, Packet.Request packet)
        {
            TheatreSessionState state = GetState(session);
            if (state.PostFightRecruitPending)
            {
                state.PostFightRecruitPending = false;
            }
            else
            {
                Dictionary<string, object?> payload = BuildRecruitCompleteStepPayload(state);
                RecordFightStageIds(state, payload);
                session.SendPush("NotifyBiancaTheatreAddStep", SerializePayload(payload));
            }

            session.SendResponse("BiancaTheatreEndRecruitResponse", SerializePayload(new Dictionary<string, object?>
            {
                ["BiancaTheatreItems"] = Array.Empty<object>(),
                ["Code"] = 0
            }), packet.Id);
        }

        [RequestPacketHandler("BiancaTheatreSelectNodeRequest")]
        public static void BiancaTheatreSelectNodeRequestHandler(Session session, Packet.Request packet)
        {
            _ = packet.Deserialize<BiancaTheatreSelectNodeRequest>();
            session.SendResponse("BiancaTheatreSelectNodeResponse", BuildCodeResponse(0), packet.Id);
        }

        [RequestPacketHandler("BiancaTheatreSetSingleTeamRequest")]
        public static void BiancaTheatreSetSingleTeamRequestHandler(Session session, Packet.Request packet)
        {
            BiancaTheatreSetSingleTeamRequest request = packet.Deserialize<BiancaTheatreSetSingleTeamRequest>();
            TheatreSessionState state = GetState(session);
            int[] requestedCardIds = ReadIntegerList(GetMapValue((object?)request.TeamData, "CardIds"))
                .Where(characterId => characterId > 0)
                .ToArray();
            int[] requestedRobotIds = ReadIntegerList(GetMapValue((object?)request.TeamData, "RobotIds"))
                .Where(robotId => robotId > 0)
                .ToArray();
            HashSet<int> knownCharacterIds = TableReaderV2.Parse<CharacterTable>()
                .Select(character => character.Id)
                .ToHashSet();
            HashSet<int> knownRobotIds = TableReaderV2.Parse<AscNet.Table.V2.share.robot.RobotTable>()
                .Where(robot => robot.CharacterId > 0)
                .Select(robot => robot.Id)
                .ToHashSet();

            state.TeamCharacterIds.Clear();
            state.TeamCharacterIds.AddRange(requestedCardIds.Where(knownCharacterIds.Contains).Distinct());
            state.TeamRobotIds.Clear();
            state.TeamRobotIds.AddRange(requestedRobotIds.Where(knownRobotIds.Contains).Distinct());

            if (state.TeamCharacterIds.Count == 0 && state.TeamRobotIds.Count == 0)
            {
                foreach (int characterId in state.RecruitedCharacterIds)
                {
                    AscNet.Table.V2.share.robot.RobotTable? robot = TableReaderV2.Parse<AscNet.Table.V2.share.robot.RobotTable>()
                        .Where(row => row.CharacterId == characterId)
                        .OrderBy(row => row.Id)
                        .FirstOrDefault();
                    if (robot is not null)
                        state.TeamRobotIds.Add(robot.Id);
                }
            }

            session.SendResponse("BiancaTheatreSetSingleTeamResponse", BuildCodeResponse(0), packet.Id);
        }

        [RequestPacketHandler("BiancaTheatreRecvFightRewardRequest")]
        public static void BiancaTheatreRecvFightRewardRequestHandler(Session session, Packet.Request packet)
        {
            BiancaTheatreRecvFightRewardRequest request = packet.Deserialize<BiancaTheatreRecvFightRewardRequest>();
            if (request.Uid == 2)
            {
                session.SendPush("NotifyItemDataList", FightRewardItemDataPayload);
                session.SendResponse("BiancaTheatreRecvFightRewardResponse", SerializePayload(new Dictionary<string, object?>
                {
                    ["RewardGoodsList"] = new object[]
                    {
                        new Dictionary<string, object?>
                        {
                            ["RewardType"] = 1,
                            ["TemplateId"] = 96119,
                            ["Count"] = 4,
                            ["Level"] = 0,
                            ["Quality"] = 0,
                            ["Grade"] = 0,
                            ["Breakthrough"] = 0,
                            ["ConvertFrom"] = 0,
                            ["IsGift"] = false,
                            ["RewardMulti"] = 0,
                            ["Id"] = 0
                        }
                    },
                    ["Code"] = 0
                }), packet.Id);
                return;
            }

            if (request.Uid == 1)
                session.SendPush("NotifyBiancaTheatreAddStep", BuildSecondRecruitStepPayload(GetState(session)));

            session.SendResponse("BiancaTheatreRecvFightRewardResponse", SerializePayload(new Dictionary<string, object?>
            {
                ["RewardGoodsList"] = Array.Empty<object>(),
                ["Code"] = 0
            }), packet.Id);
        }

        [RequestPacketHandler("BiancaTheatreEndRecvFightRewardRequest")]
        public static void BiancaTheatreEndRecvFightRewardRequestHandler(Session session, Packet.Request packet)
        {
            TheatreSessionState state = GetState(session);
            Dictionary<string, object?> payload = MessagePackPayloads.PayloadFromJson(EndRewardStepJson);
            RecordFightStageIds(state, payload);
            session.SendPush("NotifyBiancaTheatreAddStep", SerializePayload(payload));
            session.SendResponse("BiancaTheatreEndRecvFightRewardResponse", BuildCodeResponse(0), packet.Id);
        }

        [RequestPacketHandler("BiancaTheatreSettleAdventureRequest")]
        public static void BiancaTheatreSettleAdventureRequestHandler(Session session, Packet.Request packet)
        {
            session.SendPush("NotifyBiancaTheatreTotalExp", TotalExpPayload);
            session.SendResponse("BiancaTheatreSettleAdventureResponse", SerializePayload(BuildSettleAdventurePayload(GetState(session))), packet.Id);
        }

        [RequestPacketHandler("BiancaTheatreStrengthenRequest")]
        public static void BiancaTheatreStrengthenRequestHandler(Session session, Packet.Request packet)
        {
            _ = packet.Deserialize<BiancaTheatreStrengthenRequest>();
            session.SendResponse("BiancaTheatreStrengthenResponse", StrengthenResponsePayload, packet.Id);
        }

        internal static bool TryGetTheatreFightDeployment(
            Session session,
            uint stageId,
            IReadOnlyList<uint>? requestedCardIds,
            IReadOnlyList<int>? requestedRobotIds,
            out IReadOnlyList<uint> cardIds,
            out IReadOnlyList<int> robotIds)
        {
            cardIds = Array.Empty<uint>();
            robotIds = Array.Empty<int>();
            if (!SessionStates.TryGetValue(session, out TheatreSessionState? state))
                return false;

            bool requestMatchesTeam = state.TeamCharacterIds.Any(characterId => requestedCardIds?.Contains((uint)characterId) == true)
                || state.TeamRobotIds.Any(robotId => requestedRobotIds?.Contains(robotId) == true);
            if (!requestMatchesTeam && !state.FightStageIds.Contains(stageId))
                return false;

            cardIds = state.TeamCharacterIds.Select(characterId => (uint)characterId).ToArray();
            robotIds = state.TeamRobotIds.ToArray();
            if (cardIds.Count == 0 && robotIds.Count == 0)
                return false;

            state.ActiveFightStageId = stageId;
            return true;
        }

        internal static bool ApplyTheatreFightStageData(Session session, uint stageId, PreFightResponse response)
        {
            if (!SessionStates.TryGetValue(session, out TheatreSessionState? state) || state.ActiveFightStageId != stageId)
                return false;
            JObject? stageSnapshot = FightStageSnapshots[stageId.ToString()] as JObject
                ?? FightStageSnapshots.Properties().Select(property => property.Value).OfType<JObject>().FirstOrDefault();
            if (stageSnapshot is null)
                return false;

            response.FightData.EventIds = JsonSnapshot.ReadDynamicList(stageSnapshot["EventIds"]);
            response.FightData.NormalEventIds = JsonSnapshot.ReadDynamicList(stageSnapshot["NormalEventIds"]);
            response.FightData.NpcGroupList = JsonSnapshot.ReadDynamic(stageSnapshot["NpcGroupList"]);
            response.FightData.Restartable = stageSnapshot["Restartable"]?.Value<bool>() ?? false;
            response.FightData.StageParams = JsonSnapshot.ReadDynamic(stageSnapshot["StageParams"]);
            return true;
        }

        internal static bool TrySendTheatreRetreatSettle(Session session, uint stageId)
        {
            if (!SessionStates.TryGetValue(session, out TheatreSessionState? state) || state.ActiveFightStageId != stageId)
                return false;

            Dictionary<string, object?> settleResponse = BuildSettleAdventurePayload(state);
            session.SendPush("NotifyBiancaTheatreAdventureSettle", SerializePayload(new Dictionary<string, object?>
            {
                ["SettleData"] = settleResponse["SettleData"]
            }));
            state.ActiveFightStageId = null;
            return true;
        }
        internal static bool TrySendTheatreFightClearProgress(Session session, uint stageId)
        {
            if (!SessionStates.TryGetValue(session, out TheatreSessionState? state) || state.ActiveFightStageId != stageId)
                return false;

            state.FightNodeCount++;
            session.SendPush("NotifyBiancaTheatreFightNodeCountChange", SerializePayload(new Dictionary<string, object?>
            {
                ["FightNodeCount"] = state.FightNodeCount
            }));
            session.SendPush("NotifyBiancaTheatreAddStep", FightRewardStepPayload);
            session.SendPush(new NotifyArchiveMonsterRecord
            {
                Monsters =
                [
                    new() { Id = 90110, Killed = 318 },
                    new() { Id = 90100, Killed = 90 }
                ]
            });
            state.ActiveFightStageId = null;
            return true;
        }

        internal static bool TryBuildTheatreCharacterData(Session session, uint characterId, out CharacterData characterData, out IReadOnlyList<EquipData> equips)
        {
            characterData = null!;
            equips = Array.Empty<EquipData>();
            if (!SessionStates.TryGetValue(session, out TheatreSessionState? state)
                || (!state.TeamCharacterIds.Contains((int)characterId) && !state.RecruitedCharacterIds.Contains((int)characterId)))
                return false;

            CharacterTable? character = TableReaderV2.Parse<CharacterTable>().FirstOrDefault(row => row.Id == characterId);
            CharacterSkillTable? characterSkill = TableReaderV2.Parse<CharacterSkillTable>().FirstOrDefault(row => row.CharacterId == characterId);
            CharacterQualityTable? characterQuality = TableReaderV2.Parse<CharacterQualityTable>()
                .Where(row => row.CharacterId == characterId)
                .OrderBy(row => row.Quality)
                .FirstOrDefault();
            if (character is null || characterSkill is null || characterQuality is null)
                return false;

            characterData = new CharacterData
            {
                Id = characterId,
                Level = 2,
                Quality = characterQuality.Quality,
                InitQuality = characterQuality.Quality,
                Grade = 1,
                FashionId = (uint)character.DefaultNpcFashtionId,
                TrustLv = 1,
                LiberateLv = 1,
                CharacterType = character.Type,
                CharacterHeadInfo = new CharacterData.CharacterHead
                {
                    HeadFashionId = (uint)character.DefaultNpcFashtionId,
                    HeadFashionType = 0
                },
                SkillList = characterSkill.SkillGroupId
                    .Where(skillGroupId => skillGroupId > 0)
                    .Select(CharacterSkillIdFromGroupId)
                    .Distinct()
                    .Select(skillId => new CharacterSkill { Id = skillId, Level = 1 })
                    .ToList()
            };

            if (character.EquipId > 0)
            {
                equips = new[]
                {
                    new EquipData
                    {
                        TemplateId = (uint)character.EquipId,
                        CharacterId = character.Id,
                        Level = 1
                    }
                };
            }

            return true;
        }

        private static TheatreSessionState GetState(Session session)
        {
            return SessionStates.GetValue(session, _ => new TheatreSessionState());
        }

        private static int[] BuildRecruitChoices(int tickId)
        {
            int? element = ResolveInvitationElement(tickId);
            IEnumerable<CharacterTable> candidates = TableReaderV2.Parse<CharacterTable>()
                .Where(character => character.Priority > 0 && character.DefaultNpcFashtionId > 0);
            if (element.HasValue)
                candidates = candidates.Where(character => character.Element == element.Value);

            return candidates
                .OrderBy(character => character.Priority)
                .ThenBy(character => character.Id)
                .Select(character => character.Id)
                .Distinct()
                .Take(3)
                .ToArray();
        }

        private static int? ResolveInvitationElement(int tickId)
        {
            return (tickId / 1000) switch
            {
                1 => 2,
                3 => 3,
                4 => 4,
                5 => 5,
                _ => null
            };
        }

        private static Dictionary<string, object?> BuildRecruitCompleteStepPayload(TheatreSessionState state)
        {
            Dictionary<string, object?> payload = MessagePackPayloads.PayloadFromJson(RecruitCompleteStepJson);
            Dictionary<string, object?> step = (Dictionary<string, object?>)payload["Step"]!;
            step["RecruitCharacterIds"] = state.RecruitedCharacterIds.ToArray();
            return payload;
        }

        private static byte[] BuildSecondRecruitStepPayload(TheatreSessionState state)
        {
            Dictionary<string, object?> payload = MessagePackPayloads.PayloadFromJson(SecondFightRewardStepJson);
            state.PostFightRecruitPending = true;
            Dictionary<string, object?> step = (Dictionary<string, object?>)payload["Step"]!;
            int tickId = Convert.ToInt32(step["TickId"]);
            state.RecruitTickId = tickId;
            state.RefreshCharacterIds = BuildRecruitChoices(tickId);
            step["RefreshCharacterIds"] = state.RefreshCharacterIds;
            step["FloorIndexes"] = Enumerable.Repeat(0, state.RefreshCharacterIds.Length).ToArray();
            step["RecruitCharacterIds"] = state.RecruitedCharacterIds.ToArray();
            step["CurRecruitCount"] = state.RecruitedCharacterIds.Count;
            return SerializePayload(payload);
        }

        private static Dictionary<string, object?> BuildSettleAdventurePayload(TheatreSessionState state)
        {
            Dictionary<string, object?> payload = MessagePackPayloads.PayloadFromJson(SettleAdventureResponseJson);
            Dictionary<string, object?> settleData = (Dictionary<string, object?>)payload["SettleData"]!;
            IReadOnlyList<int> characterIds = state.TeamCharacterIds.Count > 0
                ? state.TeamCharacterIds
                : state.RecruitedCharacterIds;

            settleData["TeamId"] = state.TeamId;
            settleData["Characters"] = characterIds.Select(characterId => new Dictionary<string, object?>
            {
                ["CharacterId"] = characterId,
                ["Level"] = 2,
                ["IsDecay"] = 0
            }).ToArray();
            settleData["Items"] = state.SelectedItemId.HasValue
                ? new object[]
                {
                    new Dictionary<string, object?>
                    {
                        ["Uid"] = state.SelectedItemUid,
                        ["ItemId"] = state.SelectedItemId.Value
                    }
                }
                : Array.Empty<object>();
            settleData["TotalCharacterLevel"] = characterIds.Count * 2;
            settleData["TotalItemCount"] = state.SelectedItemId.HasValue ? 1 : 0;
            return payload;
        }

        private static void RecordFightStageIds(TheatreSessionState state, Dictionary<string, object?> payload)
        {
            if (payload.TryGetValue("Step", out object? stepValue) && stepValue is Dictionary<string, object?> step
                && step.TryGetValue("NodeData", out object? nodeValue) && nodeValue is Dictionary<string, object?> node
                && node.TryGetValue("Slots", out object? slotsValue) && slotsValue is IEnumerable slots)
            {
                foreach (object? slotValue in slots)
                {
                    if (slotValue is not Dictionary<string, object?> slot)
                        continue;

                    AddPositiveStageId(state, slot.GetValueOrDefault("FightId"));
                    AddPositiveStageId(state, slot.GetValueOrDefault("FightTemplateId"));
                }
            }
        }

        private static void AddPositiveStageId(TheatreSessionState state, object? value)
        {
            if (value is not null)
            {
                uint stageId = Convert.ToUInt32(value);
                if (stageId > 0)
                    state.FightStageIds.Add(stageId);
            }
        }

        private static object? GetMapValue(object? mapValue, string key)
        {
            if (mapValue is not IDictionary map)
                return null;

            foreach (DictionaryEntry entry in map)
            {
                if (string.Equals(Convert.ToString(entry.Key), key, StringComparison.Ordinal))
                    return entry.Value;
            }

            return null;
        }

        private static int[] ReadIntegerList(object? listValue)
        {
            if (listValue is not IEnumerable values)
                return [];

            List<int> result = [];
            foreach (object? value in values)
            {
                if (value is not null)
                    result.Add(Convert.ToInt32(value));
            }

            return result.ToArray();
        }

        private static uint CharacterSkillIdFromGroupId(int skillGroupId)
        {
            string text = skillGroupId.ToString();
            return uint.Parse(text[..Math.Min(6, text.Length)]);
        }

        private static byte[] BuildCodeResponse(int code)
        {
            return SerializePayload(new Dictionary<string, object?>
            {
                ["Code"] = code
            });
        }

        private static byte[] SerializeJsonPayload(string json)
        {
            return MessagePackPayloads.FromJson(json);
        }

        private static byte[] SerializePayload(Dictionary<string, object?> payload)
        {
            return MessagePackPayloads.Serialize(payload);
        }
    }
}
