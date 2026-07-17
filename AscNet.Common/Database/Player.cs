using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;
using AscNet.Logging;
using AscNet.Common.MsgPack;
using MongoDB.Bson.Serialization.Options;

namespace AscNet.Common.Database
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class BigWorldPlayerState
    {
        [BsonElement("last_world_id")]
        public int LastWorldId { get; set; }

        [BsonElement("last_level_id")]
        public int LastLevelId { get; set; }

        [BsonElement("last_position")]
        public BigWorldVector3? LastPosition { get; set; }

        [BsonElement("last_rotation")]
        public BigWorldVector4? LastRotation { get; set; }

        [BsonElement("last_rotation_y")]
        public double? LastRotationY { get; set; }

        [BsonElement("last_self_runtime_id")]
        public int LastSelfRuntimeId { get; set; }

        [BsonElement("claimed_scene_objects")]
        public List<BigWorldClaimedSceneObject> ClaimedSceneObjects { get; set; } = new();

        [BsonElement("big_world_course_read_element_ids")]
        public List<int> BigWorldCourseReadElementIds { get; set; } = new();

        [BsonElement("big_world_course_task_progress")]
        public int BigWorldCourseTaskProgress { get; set; }
    }

    public class BigWorldVector3
    {
        [BsonElement("x")]
        public double X { get; set; }

        [BsonElement("y")]
        public double Y { get; set; }

        [BsonElement("z")]
        public double Z { get; set; }
    }

    public class BigWorldVector4
    {
        [BsonElement("x")]
        public double X { get; set; }

        [BsonElement("y")]
        public double Y { get; set; }

        [BsonElement("z")]
        public double Z { get; set; }

        [BsonElement("w")]
        public double W { get; set; }
    }

    public class BigWorldClaimedSceneObject
    {
        [BsonElement("level_id")]
        public int LevelId { get; set; }

        [BsonElement("place_id")]
        public int PlaceId { get; set; }

        [BsonElement("uuid")]
        public int Uuid { get; set; }

        [BsonElement("claimed_at")]
        public long ClaimedAt { get; set; }
    }

    public class MissionProgressState
    {
        [BsonElement("condition_counters")]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
        public Dictionary<int, int> ConditionCounters { get; set; } = new();

        [BsonElement("claimed_task_ids")]
        public List<int> ClaimedTaskIds { get; set; } = new();

        [BsonElement("new_player_reward_records")]
        public List<int> NewPlayerRewardRecords { get; set; } = new();

        [BsonElement("newbie_reward_records")]
        public List<int> NewbieRewardRecords { get; set; } = new();

        [BsonElement("newbie_honor_reward")]
        public bool NewbieHonorReward { get; set; }

        [BsonElement("daily_reset_day")]
        public long DailyResetDay { get; set; } = -1;

        [BsonElement("weekly_reset_week")]
        public long WeeklyResetWeek { get; set; } = -1;
    }

    public class BossSingleChallengeBuffChoiceState
    {
        [BsonElement("index")]
        public int Index { get; set; }

        [BsonElement("choice")]
        public int Choice { get; set; }
    }

    public class BossSingleChallengeBuffGroupState
    {
        [BsonElement("buff_group_id")]
        public int BuffGroupId { get; set; }

        [BsonElement("buff_choices")]
        public List<BossSingleChallengeBuffChoiceState> BuffChoices { get; set; } = new();
    }

    public class BossSingleStageRecordState
    {
        [BsonElement("stage_id")]
        public int StageId { get; set; }

        [BsonElement("score")]
        public int Score { get; set; }

        [BsonElement("characters")]
        public List<int> Characters { get; set; } = new();

        [BsonElement("partners")]
        public List<int> Partners { get; set; } = new();

        [BsonElement("buff_group")]
        public int BuffGroup { get; set; }

        [BsonElement("challenge_buff_group")]
        public BossSingleChallengeBuffGroupState? ChallengeBuffGroup { get; set; }
    }

    public class PlayerNameplateState
    {
        [BsonElement("id")]
        public int Id { get; set; }

        [BsonElement("exp")]
        public int Exp { get; set; }

        [BsonElement("end_time")]
        public long EndTime { get; set; }

        [BsonElement("get_time")]
        public long GetTime { get; set; }
    }

    public class PlayerMedalState
    {
        [BsonElement("id")]
        public int Id { get; set; }

        [BsonElement("time")]
        public long Time { get; set; }

        [BsonElement("keep_time")]
        public long KeepTime { get; set; }

        [BsonElement("num")]
        public int Num { get; set; } = 1;
    }

    public class SimulatedBattlefieldState
    {
        [BsonElement("arena_joined")]
        public bool ArenaJoined { get; set; }

        [BsonElement("arena_point")]
        public int ArenaPoint { get; set; }

        [BsonElement("arena_contribute_score")]
        public int ArenaContributeScore { get; set; }

        [BsonElement("boss_level_type")]
        public int BossLevelType { get; set; }

        [BsonElement("boss_cleared_stage_ids")]
        public List<int> BossClearedStageIds { get; set; } = new();

        [BsonElement("boss_special_cleared_stage_ids")]
        public List<int> BossSpecialClearedStageIds { get; set; } = new();

        [BsonElement("boss_claimed_reward_ids")]
        public List<int> BossClaimedRewardIds { get; set; } = new();

        [BsonElement("boss_cycle_index")]
        public long BossCycleIndex { get; set; }

        [BsonElement("boss_challenge_count")]
        public int BossChallengeCount { get; set; }

        [BsonElement("boss_auto_fight_count")]
        public int BossAutoFightCount { get; set; }

        [BsonElement("boss_character_points")]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
        public Dictionary<int, int> BossCharacterPoints { get; set; } = new();

        [BsonElement("boss_challenge_level_type")]
        public int BossChallengeLevelType { get; set; }

        [BsonElement("boss_challenge_section_id")]
        public int BossChallengeSectionId { get; set; }

        [BsonElement("boss_challenge_feature_group_id")]
        public int BossChallengeFeatureGroupId { get; set; }

        [BsonElement("boss_challenge_total_score")]
        public int BossChallengeTotalScore { get; set; }

        [BsonElement("boss_challenge_unlock_score")]
        public int BossChallengeUnlockScore { get; set; }

        [BsonElement("boss_normal_teams")]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
        public Dictionary<int, List<int>> BossNormalTeams { get; set; } = new();

        [BsonElement("boss_current_stage_records")]
        public List<BossSingleStageRecordState> BossCurrentStageRecords { get; set; } = new();

        [BsonElement("boss_best_stage_records")]
        public List<BossSingleStageRecordState> BossBestStageRecords { get; set; } = new();

        [BsonElement("boss_challenge_stage_records")]
        public List<BossSingleStageRecordState> BossChallengeStageRecords { get; set; } = new();

        [BsonElement("repeat_challenge_level")]
        public int RepeatChallengeLevel { get; set; } = 1;

        [BsonElement("repeat_challenge_exp")]
        public int RepeatChallengeExp { get; set; }

        [BsonElement("repeat_challenge_cleared")]
        public bool RepeatChallengeCleared { get; set; }

        [BsonElement("transfinite")]
        public TransfiniteModeState Transfinite { get; set; } = new();
    }

    public class TransfiniteModeState
    {
        [BsonElement("activity_id")]
        public int ActivityId { get; set; }

        [BsonElement("begin_time")]
        public long BeginTime { get; set; }

        [BsonElement("circle_id")]
        public int CircleId { get; set; }

        [BsonElement("region_id")]
        public int RegionId { get; set; }

        [BsonElement("stage_group_id")]
        public int StageGroupId { get; set; }

        [BsonElement("stage_progress_index")]
        public int StageProgressIndex { get; set; }

        [BsonElement("start_stage_progress")]
        public int StartStageProgress { get; set; }

        [BsonElement("max_rotate_stage_progress_index")]
        public int MaxRotateStageProgressIndex { get; set; }

        [BsonElement("score")]
        public int Score { get; set; }

        [BsonElement("team")]
        public TransfiniteTeamState? Team { get; set; }

        [BsonElement("stage_records")]
        public List<TransfiniteStageRecordState> StageRecords { get; set; } = new();

        [BsonElement("confirmed_result")]
        public TransfiniteBattleResultState? ConfirmedResult { get; set; }

        [BsonElement("pending_result")]
        public TransfiniteBattleResultState? PendingResult { get; set; }

        [BsonElement("best_spend_time")]
        public int BestSpendTime { get; set; }

        [BsonElement("claimed_score_reward_indexes")]
        public List<int> ClaimedScoreRewardIndexes { get; set; } = new();
    }

    public class TransfiniteTeamState
    {
        [BsonElement("character_ids")]
        public List<int> CharacterIds { get; set; } = new();

        [BsonElement("captain_pos")]
        public int CaptainPos { get; set; } = 1;

        [BsonElement("first_fight_pos")]
        public int FirstFightPos { get; set; } = 1;

        [BsonElement("selected_general_skill")]
        public int SelectedGeneralSkill { get; set; }

        [BsonElement("enter_cg_index")]
        public int EnterCgIndex { get; set; }

        [BsonElement("settle_cg_index")]
        public int SettleCgIndex { get; set; }

        [BsonElement("character_results")]
        public List<TransfiniteCharacterResultState> CharacterResults { get; set; } = new();
    }

    public class TransfiniteStageRecordState
    {
        [BsonElement("stage_id")]
        public int StageId { get; set; }

        [BsonElement("is_win")]
        public bool IsWin { get; set; }

        [BsonElement("spend_time")]
        public int SpendTime { get; set; }

        [BsonElement("score")]
        public int Score { get; set; }
    }

    public class TransfiniteBattleResultState
    {
        [BsonElement("last_win_stage_id")]
        public int LastWinStageId { get; set; }

        [BsonElement("stage_spend_time")]
        public int StageSpendTime { get; set; }

        [BsonElement("character_result_list")]
        public List<TransfiniteCharacterResultState> CharacterResultList { get; set; } = new();
    }

    public class TransfiniteCharacterResultState
    {
        [BsonElement("character_id")]
        public int CharacterId { get; set; }

        [BsonElement("hp_percent")]
        public int HpPercent { get; set; } = 100;

        [BsonElement("energy")]
        public int Energy { get; set; }
    }

    public class AssignModeState
    {
        [BsonElement("groups")]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
        public Dictionary<int, AssignGroupState> Groups { get; set; } = new();

        [BsonElement("group_teams")]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
        public Dictionary<int, AssignTeamRecordState> GroupTeams { get; set; } = new();

        [BsonElement("chapter_characters")]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
        public Dictionary<int, int> ChapterCharacters { get; set; } = new();

        [BsonElement("claimed_chapter_rewards")]
        public List<int> ClaimedChapterRewards { get; set; } = new();
    }

    public class AssignGroupState
    {
        [BsonElement("fight_count")]
        public int FightCount { get; set; }

        [BsonElement("is_perfect")]
        public bool IsPerfect { get; set; }

        [BsonElement("finish_stage_ids")]
        public List<int> FinishStageIds { get; set; } = new();

        [BsonElement("reboot_count")]
        public int RebootCount { get; set; }
    }

    public class AssignTeamRecordState
    {
        [BsonElement("team_info_list")]
        public List<List<int>> TeamInfoList { get; set; } = new();

        [BsonElement("captain_pos_list")]
        public List<int> CaptainPosList { get; set; } = new();

        [BsonElement("first_fight_pos_list")]
        public List<int> FirstFightPosList { get; set; } = new();
    }

    public class AwarenessModeState
    {
        [BsonElement("chapters")]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
        public Dictionary<int, AwarenessChapterState> Chapters { get; set; } = new();

        [BsonElement("chapter_teams")]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
        public Dictionary<int, AwarenessTeamRecordState> ChapterTeams { get; set; } = new();

        [BsonElement("chapter_characters")]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
        public Dictionary<int, int> ChapterCharacters { get; set; } = new();

        [BsonElement("claimed_chapter_rewards")]
        public List<int> ClaimedChapterRewards { get; set; } = new();
    }

    public class AwarenessChapterState
    {
        [BsonElement("fight_count")]
        public int FightCount { get; set; }

        [BsonElement("finish_stage_ids")]
        public List<int> FinishStageIds { get; set; } = new();
    }

    public class AwarenessTeamRecordState
    {
        [BsonElement("team_info_list")]
        public List<List<int>> TeamInfoList { get; set; } = new();

        [BsonElement("captain_pos_list")]
        public List<int> CaptainPosList { get; set; } = new();

        [BsonElement("first_fight_pos_list")]
        public List<int> FirstFightPosList { get; set; } = new();
    }

    public class StrongholdModeState
    {
        [BsonElement("activity_begin_time")]
        public long ActivityBeginTime { get; set; }

        [BsonElement("level_id")]
        public int LevelId { get; set; } = 1;

        [BsonElement("electric_energy")]
        public int ElectricEnergy { get; set; } = 2500;

        [BsonElement("endurance")]
        public int Endurance { get; set; } = 30;

        [BsonElement("mineral_left")]
        public int MineralLeft { get; set; }

        [BsonElement("total_mineral")]
        public int TotalMineral { get; set; }

        [BsonElement("borrow_count")]
        public int BorrowCount { get; set; }

        [BsonElement("assist_character_id")]
        public int AssistCharacterId { get; set; }

        [BsonElement("electric_character_ids")]
        public List<int> ElectricCharacterIds { get; set; } = new();

        [BsonElement("finished_group_ids")]
        public List<int> FinishedGroupIds { get; set; } = new();

        [BsonElement("group_finish_stage_ids")]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
        public Dictionary<int, List<int>> GroupFinishStageIds { get; set; } = new();

        [BsonElement("teams")]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
        public Dictionary<int, StrongholdTeamState> Teams { get; set; } = new();

        [BsonElement("claimed_reward_ids")]
        public List<int> ClaimedRewardIds { get; set; } = new();

        [BsonElement("stay_days")]
        public List<int> StayDays { get; set; } = new();
    }

    public class StrongholdTeamState
    {
        [BsonElement("id")]
        public int Id { get; set; }

        [BsonElement("captain_pos")]
        public int CaptainPos { get; set; } = 1;

        [BsonElement("first_pos")]
        public int FirstPos { get; set; } = 1;

        [BsonElement("rune_id")]
        public int RuneId { get; set; }

        [BsonElement("sub_rune_id")]
        public int SubRuneId { get; set; }

        [BsonElement("enter_cg_index")]
        public int EnterCgIndex { get; set; }

        [BsonElement("settle_cg_index")]
        public int SettleCgIndex { get; set; }

        [BsonElement("element_id")]
        public int ElementId { get; set; }

        [BsonElement("selected_general_skill")]
        public int SelectedGeneralSkill { get; set; }

        [BsonElement("character_infos")]
        public List<StrongholdCharacterState> CharacterInfos { get; set; } = new();

        [BsonElement("plugin_infos")]
        public List<StrongholdPluginState> PluginInfos { get; set; } = new();
    }

    public class StrongholdCharacterState
    {
        [BsonElement("pos")]
        public int Pos { get; set; }

        [BsonElement("id")]
        public int Id { get; set; }

        [BsonElement("player_id")]
        public long PlayerId { get; set; }

        [BsonElement("robot_id")]
        public int RobotId { get; set; }

        [BsonElement("ability")]
        public int Ability { get; set; }
    }

    public class StrongholdPluginState
    {
        [BsonElement("id")]
        public int Id { get; set; }

        [BsonElement("count")]
        public int Count { get; set; }
    }

    public class Player
    {
        public static IMongoCollection<Player> collection = Common.db.GetCollection<Player>("players");
        private static readonly Logger log = new(typeof(Player), LogLevel.WARN, LogLevel.WARN);

        public static Player FromPlayerId(long id)
        {
            return collection.AsQueryable().FirstOrDefault(x => x.PlayerData.Id == id) ?? Create(id);
        }

        public static Player? TryFromPlayerId(long id)
        {
            try
            {
                return collection.AsQueryable().FirstOrDefault(x => x.PlayerData.Id == id);
            }
            catch (Exception ex)
            {
                log.Warn($"Player lookup failed for id {id}; falling back to minimal player info.", ex);
                return null;
            }
        }

        public static Player? FromToken(string token)
        {
            return collection.AsQueryable().FirstOrDefault(x => x.Token == token);
        }

        private static Player Create(long id)
        {
            Player player = new()
            {
                Token = Guid.NewGuid().ToString(),
                PlayerData = new()
                {
                    Id = id,
                    Name = $"Commandant{id}",
                    Level = 1,
                    Sign = "",
                    DisplayCharId = 1021001,
                    DisplayCharIdList = new() { 1021001 },
                    Birthday = null,
                    Gender = 0,
                    HonorLevel = 1,
                    ServerId = "1",
                    CurrTeamId = 1,
                    CurrHeadPortraitId = 9000003,
                    CurrHeadFrameId = 0,
                    CurrMedalId = 0,
                    CurrentChatBoardId = 25000001,
                    AppearanceSettingInfo = new()
                    {
                        TitleType = 1,
                        CharacterType = 1,
                        FashionType = 1,
                        WeaponFashionType = 1,
                        DormitoryType = 1
                    },
                    CreateTime = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    LastLoginTime = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    ChangeGenderTime = 0,
                    Flags = 1,
                    NewPlayerTaskActiveDay = 0
                },
                HeadPortraits = new(),
                TeamGroups = new()
                {
                    {1, new TeamGroupDatum()
                    {
                        TeamType = 1,
                        TeamId = 1,
                        CaptainPos = 1,
                        FirstFightPos = 1,
                        TeamData = new()
                        {
                            {1, 1021001},
                            {2, 0},
                            {3, 0}
                        },
                        TeamName = null
                    }}
                },
                FubenMainLineData = new(),
            };
            player.AddHead(9000001);
            player.AddHead(9000002);
            player.AddHead(9000003);
            
            collection.InsertOne(player);

            return player;
        }

        public void AddHead(int id)
        {
            TryAddHead(id, out _);
        }

        public bool TryAddHead(int id, out HeadPortraitList head)
        {
            HeadPortraitList? existing = HeadPortraits.FirstOrDefault(candidate => candidate.Id == id);
            if (existing is not null)
            {
                head = existing;
                return false;
            }

            head = new HeadPortraitList
            {
                Id = id,
                LeftCount = 1,
                BeginTime = DateTimeOffset.Now.ToUnixTimeSeconds()
            };
            HeadPortraits.Add(head);
            return true;
        }

        public void UnlockAllProfileContent(ProfileUnlockCatalog catalog, long? now = null)
        {
            ArgumentNullException.ThrowIfNull(catalog);
            long getTime = now ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            HeadPortraits ??= [];
            foreach (int id in catalog.HeadPortraitIds)
                TryAddHead(id, out _);
            HeadPortraits = HeadPortraits
                .GroupBy(head => head.Id)
                .Select(group => group.First())
                .OrderBy(head => head.Id)
                .ToList();

            UnlockedMedals = catalog.MedalIds
                .Select(id => new PlayerMedalState
                {
                    Id = id,
                    Time = getTime,
                    KeepTime = 0,
                    Num = 1
                })
                .ToList();
            UnlockedChatBoards = catalog.ChatBoardIds.ToList();
            UnlockNameplates = catalog.NameplateIds
                .Select(id => new PlayerNameplateState
                {
                    Id = id,
                    Exp = 0,
                    EndTime = 0,
                    GetTime = getTime
                })
                .ToList();

            if (!catalog.HeadPortraitIds.Contains((int)PlayerData.CurrHeadPortraitId))
                PlayerData.CurrHeadPortraitId = catalog.HeadPortraitIds[0];
            if (PlayerData.CurrHeadFrameId != 0
                && !catalog.HeadPortraitIds.Contains((int)PlayerData.CurrHeadFrameId))
                PlayerData.CurrHeadFrameId = 0;
            if (PlayerData.CurrMedalId != 0
                && !catalog.MedalIds.Contains((int)PlayerData.CurrMedalId))
                PlayerData.CurrMedalId = 0;
            if (!catalog.ChatBoardIds.Contains(PlayerData.CurrentChatBoardId))
                PlayerData.CurrentChatBoardId = catalog.ChatBoardIds[0];
            if (CurrentWearNameplate != 0
                && !catalog.NameplateIds.Contains(CurrentWearNameplate))
                CurrentWearNameplate = 0;

            SimulatedBattlefield ??= new();
            SimulatedBattlefield.BossChallengeLevelType = catalog.BossSingleChallengeUnlock.LevelType;
            SimulatedBattlefield.BossChallengeSectionId = catalog.BossSingleChallengeUnlock.SectionConfigId;
            SimulatedBattlefield.BossChallengeFeatureGroupId = catalog.BossSingleChallengeUnlock.FeatureGroupId;
            SimulatedBattlefield.BossChallengeUnlockScore = catalog.BossSingleChallengeUnlock.RequiredTotalScore;
            SimulatedBattlefield.BossChallengeStageRecords ??= [];
            SimulatedBattlefield.BossChallengeTotalScore = SimulatedBattlefield.BossChallengeStageRecords.Sum(record => record.Score);
        }

        public bool AddTreasure(int id)
        {
            if (FubenMainLineData.TreasureData.Contains(id))
            {
                return false;
            }

            FubenMainLineData.TreasureData.Add(id);
            return true;
        }

        public bool AddMainLine2MainTreasure(int mainId, int treasureIdx)
        {
            FubenMainLine2Data ??= new();
            MainLine2MainDatum? mainData = FubenMainLine2Data.MainDatas.FirstOrDefault(x => x.Id == mainId);
            if (mainData is null)
            {
                mainData = new MainLine2MainDatum
                {
                    Id = mainId
                };
                FubenMainLine2Data.MainDatas.Add(mainData);
            }

            if (mainData.MainTreasureIdxs.Contains(treasureIdx))
            {
                return false;
            }

            mainData.MainTreasureIdxs.Add(treasureIdx);
            mainData.MainTreasureIdxs.Sort();
            return true;
        }

        public bool AddGatherReward(int id)
        {
            if (GatherRewards.Contains(id))
            {
                return false;
            }

            GatherRewards.Add(id);
            return true;
        }

        public void Save()
        {
            collection.ReplaceOne(Builders<Player>.Filter.Eq(x => x.Id, Id), this);
        }

        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("token")]
        [BsonRequired]
        public string Token { get; set; }

        [BsonElement("player_data")]
        [BsonRequired]
        public PlayerData PlayerData { get; set; }

        [BsonElement("head_portraits")]
        [BsonRequired]
        public List<HeadPortraitList> HeadPortraits { get; set; }

        [BsonElement("gather_rewards")]
        public List<int> GatherRewards { get; set; } = [5];

        [BsonElement("use_background_id")]
        public int UseBackgroundId { get; set; } = 14000001;

        [BsonElement("last_sign_in_time")]
        public long LastSignInTime { get; set; }

        [BsonElement("sign_in_claim_count")]
        public long SignInClaimCount { get; set; }

        [BsonElement("red_point_records")]
        public RedPointRecords RedPointRecords { get; set; } = new();

        [BsonElement("assist_character_id")]
        public int AssistCharacterId { get; set; }

        [BsonElement("unlock_comics")]
        public List<int> UnlockComics { get; set; } = AscNet.Common.ArchiveDefaults.CreateDefaultUnlockedArchiveComics();

        [BsonElement("life_tree_data")]
        public NotifyLifeTreeData LifeTreeData { get; set; } = new();

        [BsonElement("purchase_buy_times")]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
        public Dictionary<uint, int> PurchaseBuyTimes { get; set; } = new();

        [BsonElement("shop_buy_times")]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
        public Dictionary<uint, int> ShopBuyTimes { get; set; } = new();

        [BsonElement("team_groups")]
        [BsonRequired]
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
        public Dictionary<int, TeamGroupDatum> TeamGroups { get; set; }

        [BsonElement("fuben_main_line_data")]
        public FubenMainLineData FubenMainLineData { get; set; } = new();

        [BsonElement("mission_progress")]
        public MissionProgressState MissionProgress { get; set; } = new();

        [BsonElement("simulated_battlefield")]
        public SimulatedBattlefieldState SimulatedBattlefield { get; set; } = new();

        [BsonElement("current_wear_nameplate")]
        public int CurrentWearNameplate { get; set; }

        [BsonElement("unlock_nameplates")]
        public List<PlayerNameplateState> UnlockNameplates { get; set; } = new();

        [BsonElement("unlocked_medals")]
        public List<PlayerMedalState> UnlockedMedals { get; set; } = new();

        [BsonElement("unlocked_chat_boards")]
        public List<long> UnlockedChatBoards { get; set; } = new();

        [BsonElement("assign_mode")]
        public AssignModeState AssignMode { get; set; } = new();

        [BsonElement("awareness_mode")]
        public AwarenessModeState AwarenessMode { get; set; } = new();

        [BsonElement("stronghold_mode")]
        public StrongholdModeState StrongholdMode { get; set; } = new();

        [BsonElement("big_world_state")]
        public BigWorldPlayerState BigWorldState { get; set; } = new();

        [BsonElement("fuben_main_line2_data")]
        public FubenMainLine2Data FubenMainLine2Data { get; set; } = new();
    }
}
