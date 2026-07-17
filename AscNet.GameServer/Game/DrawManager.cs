using AscNet.Common.Database;
using AscNet.Common.MsgPack;
using AscNet.Common.Util;
using AscNet.GameServer.Handlers;
using AscNet.Logging;
using AscNet.Table.V2.client.draw;
using AscNet.Table.V2.share.character;
using AscNet.Table.V2.share.character.quality;
using AscNet.Table.V2.share.equip;
using AscNet.Table.V2.share.item;

namespace AscNet.GameServer.Game
{
    internal partial class DrawManager
    {
        public static readonly List<DrawSceneTable> drawSceneTables = TableReaderV2.Parse<DrawSceneTable>();
        public static readonly List<DrawPreviewTable> drawPreviewTables = TableReaderV2.Parse<DrawPreviewTable>();
        public static readonly List<CharacterTable> charactersTables = TableReaderV2.Parse<CharacterTable>();
        public static readonly List<CharacterQualityTable> characterQualitiesTables = TableReaderV2.Parse<CharacterQualityTable>();
        private static readonly List<EquipTable> equipTables = TableReaderV2.Parse<EquipTable>();
        private static readonly List<ItemTable> itemTables = TableReaderV2.Parse<ItemTable>();
        private static readonly HashSet<int> drawPreviewIds = drawPreviewTables.Select(x => x.Id).ToHashSet();
        private static readonly HashSet<int> drawWaferShowIds = TableReaderV2.Parse<DrawWaferShowTable>().Select(x => x.Id).ToHashSet();
        private static readonly Logger log = new(typeof(DrawManager), LogLevel.DEBUG, LogLevel.DEBUG);
        private const int MinDrawItemShowQuality = 3;
        private static readonly object stateLock = new();
        private static readonly Dictionary<long, Dictionary<int, DrawProgress>> drawProgressByPlayer = new();
        private static readonly Dictionary<long, Dictionary<int, Dictionary<int, int>>> selectedDrawByPlayerGroup = new();
        private static readonly Dictionary<long, Dictionary<int, int>> switchCountByPlayerGroup = new();
        private static readonly Dictionary<(long PlayerId, int GroupId, int GroupSubType), List<DrawHistoryEntry>> drawHistoryByPlayerGroup = new();

        #region DrawTags
        public const int TagBase = 1;
        public const int TagEvent = 2;
        public const int TagSpecialEvent = 3;
        public const int TagTargetUniframe = 4;
        public const int TagCollab = 5;
        public const int TagEndlessSummerBlue = 6;
        public const int TagCUB = 7;
        #endregion

        #region Groups
        public const int GroupMemberTarget = 1;
        public const int GroupWeaponResearch = 2;
        public const int GroupTargetWeaponResearch = 4;
        public const int GroupDormitoryResearch = 6;
        public const int GroupThemedTargetWeapon = 10;
        public const int GroupThemedEventConstruct = 11;
        public const int GroupArrivalConstruct = 12;
        public const int GroupFateArrivalConstruct = 13;
        public const int GroupArrivalEventConstruct = 14;
        public const int GroupFateThemedConstruct = 15;
        public const int GroupTargetUniframe = 16;
        public const int GroupAnniversary = 17;
        public const int GroupFateAnniversaryLimited = 18;
        public const int GroupCollabTarget = 19;
        public const int GroupFateCollabTarget = 20;
        public const int GroupCollabWeaponTarget = 21;
        public const int GroupCUBTarget = 22;
        public const int GroupWishingTarget = 23;
        public const int GroupFateWishingTarget = 24;
        #endregion

        private sealed record DrawProgress(int TodayCount, int TotalCount);
        private sealed record DrawHistoryEntry(RewardGoods RewardGoods, long DrawTime);

        private sealed record DrawGroupDefinition(
            int Id,
            int Tag,
            int Order,
            int Priority,
            int UseItemId,
            int MaxBottomTimes,
            int Type,
            string Banner,
            Dictionary<int, int> DefaultUseDrawIdDict,
            List<int> OptionalDrawIdList,
            List<int> TagBlackListDrawIds,
            long BannerBeginTime,
            long BannerEndTime,
            int ConditionId,
            int ShowPredictType,
            long StartTime,
            long EndTime);

        private sealed record DrawInfoTemplate(
            int Id,
            int GroupId,
            int DrawType,
            int UseItemId,
            int UseItemCount,
            int BaseTodayCount,
            int BaseTotalCount,
            int BottomTimes,
            int MaxBottomTimes,
            long StartTime,
            long EndTime,
            string Banner,
            Dictionary<int, string> Resources,
            Dictionary<int, int> ResourceIds,
            List<int> BtnDrawCount,
            List<int> PurchaseUiType,
            List<int> PurchaseId,
            List<int> ExPurchaseIds,
            int CapacityCheckType,
            int GroupSubType,
            bool IsTriggerSpecified,
            bool IsShowShop,
            bool IsShowBubble,
            int UseTenDrawOnSaleTimes,
            int DailyLimitTimes,
            int ActivityLimitTimes,
            int ShowPriority,
            int UpGoodsId);

        private static readonly DrawGroupDefinition[] GroupDefinitions = BuildActiveDrawGroupDefinitions();
        private static readonly DrawInfoTemplate[] RetailDrawInfoTemplates = BuildActiveRetailDrawInfoTemplates();

        private static readonly Dictionary<int, DrawInfoTemplate> RetailDrawInfoById = RetailDrawInfoTemplates.ToDictionary(x => x.Id);
        private static readonly Dictionary<int, List<DrawInfoTemplate>> RetailDrawInfosByGroup = RetailDrawInfoTemplates
            .GroupBy(x => x.GroupId)
            .ToDictionary(x => x.Key, x => x.OrderBy(info => info.Id).ToList());

        public static List<DrawGroupInfo> GetDrawGroupInfos(long playerId = 0)
        {
            List<DrawGroupInfo> groups = new();
            foreach (DrawGroupDefinition definition in GroupDefinitions)
            {
                if (!RetailDrawInfosByGroup.TryGetValue(definition.Id, out List<DrawInfoTemplate>? infos) || infos.Count == 0)
                    continue;

                Dictionary<int, int> useDrawIdDict = GetUseDrawIdDict(playerId, definition);
                DrawInfo selectedInfo = BuildDrawInfo(GetSelectedTemplate(definition.Id, useDrawIdDict) ?? infos[0], playerId);
                groups.Add(new DrawGroupInfo
                {
                    Id = definition.Id,
                    Tag = definition.Tag,
                    Type = definition.Type,
                    Order = definition.Order,
                    Priority = definition.Priority,
                    UseItemId = definition.UseItemId,
                    BottomTimes = selectedInfo.BottomTimes,
                    MaxBottomTimes = definition.MaxBottomTimes,
                    SwitchDrawIdCount = GetSwitchDrawIdCount(playerId, definition.Id),
                    UseDrawIdDict = useDrawIdDict,
                    OptionalDrawIdList = [.. definition.OptionalDrawIdList],
                    TagBlackListDrawIds = [.. definition.TagBlackListDrawIds],
                    Banner = definition.Banner,
                    StartTime = definition.StartTime,
                    EndTime = definition.EndTime,
                    BannerBeginTime = definition.BannerBeginTime,
                    BannerEndTime = definition.BannerEndTime,
                    ConditionId = definition.ConditionId,
                    ShowPredictType = definition.ShowPredictType,
                });
            }

            return groups;
        }

        public static List<(int DrawGroupId, int Priority)> GetDrawHistoryGroups()
        {
            return GroupDefinitions
                .Where(definition => RetailDrawInfosByGroup.TryGetValue(definition.Id, out List<DrawInfoTemplate>? infos) && infos.Count > 0)
                .Select(definition => (definition.Id, definition.Priority))
                .ToList();
        }

        public static (int BottomTimes, int MaxBottomTimes) GetDrawHistoryStatus(long playerId, int groupId, int groupSubType)
        {
            if (!RetailDrawInfosByGroup.TryGetValue(groupId, out List<DrawInfoTemplate>? infos) || infos.Count == 0)
                return (0, 0);

            DrawInfoTemplate template = infos.FirstOrDefault(info => info.GroupSubType == groupSubType)
                ?? GetSelectedTemplate(groupId, GetUseDrawIdDict(playerId, GroupDefinitions.First(definition => definition.Id == groupId)))
                ?? infos[0];
            DrawInfo drawInfo = BuildDrawInfo(template, playerId);
            return (drawInfo.BottomTimes, drawInfo.MaxBottomTimes);
        }

        public static void RecordDrawHistory(long playerId, int drawId, IEnumerable<RewardGoods> rewards)
        {
            if (!RetailDrawInfoById.TryGetValue(drawId, out DrawInfoTemplate? template))
                return;

            long drawTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            (long PlayerId, int GroupId, int GroupSubType) key = (playerId, template.GroupId, template.GroupSubType);

            lock (stateLock)
            {
                if (!drawHistoryByPlayerGroup.TryGetValue(key, out List<DrawHistoryEntry>? history))
                {
                    history = [];
                    drawHistoryByPlayerGroup[key] = history;
                }

                history.AddRange(rewards.Select(reward => new DrawHistoryEntry(CloneRewardGoods(reward), drawTime)));
                if (history.Count > 100)
                    history.RemoveRange(0, history.Count - 100);
            }
        }

        public static List<(RewardGoods RewardGoods, long DrawTime)> GetDrawHistory(long playerId, int groupId, int groupSubType)
        {
            lock (stateLock)
            {
                return drawHistoryByPlayerGroup.TryGetValue((playerId, groupId, groupSubType), out List<DrawHistoryEntry>? history)
                    ? history.Select(entry => (CloneRewardGoods(entry.RewardGoods), entry.DrawTime)).ToList()
                    : [];
            }
        }

        private static RewardGoods CloneRewardGoods(RewardGoods reward)
        {
            return new RewardGoods
            {
                RewardType = reward.RewardType,
                TemplateId = reward.TemplateId,
                Count = reward.Count,
                Level = reward.Level,
                Quality = reward.Quality,
                Grade = reward.Grade,
                Breakthrough = reward.Breakthrough,
                ConvertFrom = reward.ConvertFrom,
                ShowQuality = reward.ShowQuality,
                Id = reward.Id,
                IsGift = reward.IsGift,
                RewardMulti = reward.RewardMulti
            };
        }

        public static List<DrawAdjustActivityInfo> GetDrawAdjustActivityInfos()
        {
            return BuildGeneratedDrawAdjustActivityInfos();
        }

        public static List<DrawInfo> GetDrawInfosByGroup(int groupId, long playerId = 0)
        {
            return RetailDrawInfosByGroup.TryGetValue(groupId, out List<DrawInfoTemplate>? infos)
                ? infos.Select(info => BuildDrawInfo(info, playerId)).ToList()
                : [];
        }

        public static DrawInfo? GetDrawInfoById(int drawId, long playerId = 0)
        {
            return RetailDrawInfoById.TryGetValue(drawId, out DrawInfoTemplate? template)
                ? BuildDrawInfo(template, playerId)
                : null;
        }

        public static int SetUseDrawId(long playerId, int drawId)
        {
            if (!RetailDrawInfoById.TryGetValue(drawId, out DrawInfoTemplate? template))
                return 0;

            int selectionSlot = GetSelectionSlot(template);
            lock (stateLock)
            {
                if (!selectedDrawByPlayerGroup.TryGetValue(playerId, out Dictionary<int, Dictionary<int, int>>? selectedByGroup))
                {
                    selectedByGroup = new();
                    selectedDrawByPlayerGroup[playerId] = selectedByGroup;
                }

                if (!selectedByGroup.TryGetValue(template.GroupId, out Dictionary<int, int>? selectedSlots))
                {
                    DrawGroupDefinition definition = GroupDefinitions.First(x => x.Id == template.GroupId);
                    selectedSlots = new(definition.DefaultUseDrawIdDict);
                    selectedByGroup[template.GroupId] = selectedSlots;
                }

                selectedSlots[selectionSlot] = drawId;

                if (!switchCountByPlayerGroup.TryGetValue(playerId, out Dictionary<int, int>? switchCountByGroup))
                {
                    switchCountByGroup = new();
                    switchCountByPlayerGroup[playerId] = switchCountByGroup;
                }
                switchCountByGroup[template.GroupId] = switchCountByGroup.GetValueOrDefault(template.GroupId) + 1;
                return switchCountByGroup[template.GroupId];
            }
        }

        public static DrawInfo? ApplyDrawProgress(long playerId, int drawId, int count)
        {
            if (!RetailDrawInfoById.ContainsKey(drawId))
                return null;

            lock (stateLock)
            {
                if (!drawProgressByPlayer.TryGetValue(playerId, out Dictionary<int, DrawProgress>? playerProgress))
                {
                    playerProgress = new();
                    drawProgressByPlayer[playerId] = playerProgress;
                }

                DrawProgress current = playerProgress.GetValueOrDefault(drawId, new DrawProgress(0, 0));
                playerProgress[drawId] = current with
                {
                    TodayCount = current.TodayCount + count,
                    TotalCount = current.TotalCount + count
                };
            }

            return GetDrawInfoById(drawId, playerId);
        }

        public static int GetGroupByDrawId(int drawId)
        {
            return RetailDrawInfoById.TryGetValue(drawId, out DrawInfoTemplate? template) ? template.GroupId : 0;
        }

        public static List<RewardGoods> DrawDraw(long playerId, int drawId, int pullOffset = 0)
        {
            List<RewardGoods> rewards = new();
            if (!RetailDrawInfoById.TryGetValue(drawId, out DrawInfoTemplate? template))
            {
                log.Error($"Invalid draw id {drawId}");
                return rewards;
            }

            DrawProgress progress = GetDrawProgress(playerId, drawId);
            int bottomTimesBeforePull = GetBottomTimes(template.MaxBottomTimes, template.BottomTimes, progress.TotalCount + pullOffset);
            bool forceRare = IsClient46HelentineFirstPull(template.Id, progress.TotalCount + pullOffset)
                || template.MaxBottomTimes > 0 && bottomTimesBeforePull == 1;

            RewardGoods? reward = DrawRetailReward(template, forceRare);
            if (reward is not null)
                rewards.Add(reward);

            return rewards;
        }

        private static RewardGoods? DrawRetailReward(DrawInfoTemplate template, bool forceRare)
        {
            return template.GroupId switch
            {
                GroupWeaponResearch or GroupTargetWeaponResearch => DrawEquipReward(template, forceRare),
                GroupFateArrivalConstruct or GroupFateThemedConstruct or GroupFateAnniversaryLimited or GroupFateCollabTarget or GroupFateWishingTarget => DrawLegacyCharacterReward(template, forceRare),
                GroupCUBTarget => DrawFallbackItemReward(),
                _ => DrawCharacterReward(template, forceRare)
            };
        }

        private static DrawInfo BuildDrawInfo(DrawInfoTemplate template, long playerId)
        {
            DrawProgress progress = GetDrawProgress(playerId, template.Id);
            return new DrawInfo
            {
                Id = template.Id,
                GroupId = template.GroupId,
                DrawType = template.DrawType,
                UseItemId = template.UseItemId,
                UseItemCount = template.UseItemCount,
                TodayCount = template.BaseTodayCount + progress.TodayCount,
                TotalCount = template.BaseTotalCount + progress.TotalCount,
                BottomTimes = GetBottomTimes(template.MaxBottomTimes, template.BottomTimes, progress.TotalCount),
                MaxBottomTimes = template.MaxBottomTimes,
                StartTime = template.StartTime,
                EndTime = template.EndTime,
                Banner = template.Banner,
                Resources = new(template.Resources),
                ResourceIds = new(template.ResourceIds),
                BtnDrawCount = [.. template.BtnDrawCount],
                PurchaseUiType = [.. template.PurchaseUiType],
                PurchaseId = [.. template.PurchaseId],
                ExPurchaseIds = [.. template.ExPurchaseIds],
                CapacityCheckType = template.CapacityCheckType,
                UpGoodsId = template.UpGoodsId,
                IsTriggerSpecified = template.IsTriggerSpecified,
                IsShowShop = template.IsShowShop,
                IsShowBubble = template.IsShowBubble,
                UseTenDrawOnSaleTimes = template.UseTenDrawOnSaleTimes,
                DailyLimitTimes = template.DailyLimitTimes,
                ActivityLimitTimes = template.ActivityLimitTimes,
                GroupSubType = template.GroupSubType,
                ShowPriority = template.ShowPriority
            };
        }

        private static DrawProgress GetDrawProgress(long playerId, int drawId)
        {
            lock (stateLock)
            {
                if (drawProgressByPlayer.TryGetValue(playerId, out Dictionary<int, DrawProgress>? playerProgress)
                    && playerProgress.TryGetValue(drawId, out DrawProgress? progress)
                    && progress is not null)
                    return progress;
            }

            return new DrawProgress(0, 0);
        }

        private static int GetBottomTimes(int maxBottomTimes, int templateBottomTimes, int progressCount)
        {
            if (maxBottomTimes <= 0)
                return 0;

            int consumed = maxBottomTimes - templateBottomTimes;
            consumed = (consumed + progressCount) % maxBottomTimes;
            return consumed == 0 ? maxBottomTimes : maxBottomTimes - consumed;
        }

        private static Dictionary<int, int> GetUseDrawIdDict(long playerId, DrawGroupDefinition definition)
        {
            lock (stateLock)
            {
                if (selectedDrawByPlayerGroup.TryGetValue(playerId, out Dictionary<int, Dictionary<int, int>>? selectedByGroup)
                    && selectedByGroup.TryGetValue(definition.Id, out Dictionary<int, int>? selectedSlots))
                    return new(selectedSlots);
            }

            return new(definition.DefaultUseDrawIdDict);
        }

        private static DrawInfoTemplate? GetSelectedTemplate(int groupId, Dictionary<int, int> useDrawIdDict)
        {
            foreach (int drawId in useDrawIdDict.OrderByDescending(x => x.Key).Select(x => x.Value))
            {
                if (drawId > 0 && RetailDrawInfoById.TryGetValue(drawId, out DrawInfoTemplate? template))
                    return template;
            }

            return RetailDrawInfosByGroup.TryGetValue(groupId, out List<DrawInfoTemplate>? infos) ? infos.FirstOrDefault() : null;
        }

        private static int GetSelectionSlot(DrawInfoTemplate template)
        {
            DrawGroupDefinition? definition = GroupDefinitions.FirstOrDefault(x => x.Id == template.GroupId);
            if (definition is null)
                return 0;

            foreach ((int slot, int drawId) in definition.DefaultUseDrawIdDict)
            {
                if (drawId == template.Id)
                    return slot;
            }

            if (definition.TagBlackListDrawIds.Contains(template.Id))
                return template.GroupId switch
                {
                    GroupTargetWeaponResearch => 3,
                    GroupCUBTarget => 4,
                    GroupThemedEventConstruct or GroupFateThemedConstruct => 5,
                    _ => 0
                };

            return 0;
        }

        private static int GetSwitchDrawIdCount(long playerId, int groupId)
        {
            lock (stateLock)
            {
                return switchCountByPlayerGroup.TryGetValue(playerId, out Dictionary<int, int>? switchCountByGroup)
                    ? switchCountByGroup.GetValueOrDefault(groupId)
                    : 0;
            }
        }
        private static RewardGoods? DrawCharacterReward(DrawInfoTemplate template, bool forceRare)
        {
            if (forceRare && TryCreateTargetCharacterReward(template, out RewardGoods? targetReward))
                return targetReward;

            int roll = Random.Shared.Next(9860);
            if (roll < 50 && TryCreateTargetCharacterReward(template, out targetReward))
                return targetReward;

            if (roll < 1445)
                return DrawCharacterShardReward(template);

            if (roll < 3656)
                return DrawCharacterShardReward(template);

            if (roll < 6495)
                return DrawMemoryReward();

            if (roll < 7937)
                return DrawOverclockMaterialReward();

            if (roll < 8418)
                return DrawExpMaterialReward();

            return DrawCogBoxReward();
        }

        private static RewardGoods? DrawLegacyCharacterReward(DrawInfoTemplate template, bool forceRare)
        {
            if (forceRare && TryCreateTargetCharacterReward(template, out RewardGoods? targetReward))
                return targetReward;

            double roll = Random.Shared.NextDouble();
            if (roll < 0.015 && TryCreateTargetCharacterReward(template, out targetReward))
                return targetReward;

            if (roll < 0.25)
                return DrawCharacterShardReward(template);

            if (roll < 0.58)
                return DrawMemoryReward();

            return DrawFallbackItemReward();
        }

        private static RewardGoods? DrawEquipReward(DrawInfoTemplate template, bool forceRare)
        {
            if (forceRare && TryCreateTargetEquipReward(template, out RewardGoods? targetReward))
                return targetReward;

            int roll = Random.Shared.Next(10000);
            if (roll < 400 && TryCreateTargetEquipReward(template, out targetReward))
                return targetReward;

            if (roll < 450)
                return DrawRandomWeaponReward(quality: 6, excludeEquipId: template.ResourceIds.GetValueOrDefault(1));

            if (roll < 600)
                return DrawPreviewEquipReward(template) ?? DrawRandomWeaponReward(quality: 5);

            if (roll < 750)
                return DrawRandomWeaponReward(quality: 5, excludeEquipId: template.ResourceIds.GetValueOrDefault(1));

            if (roll < 4090)
                return DrawRandomWeaponReward(quality: 4);

            if (roll < 6880)
                return DrawRandomWeaponReward(quality: 3);
            if (roll < 7815)
                return DrawCogBoxReward();

            if (roll < 8750)
                return DrawOverclockMaterialReward();

            return DrawExpMaterialReward();
        }

        private static bool TryCreateTargetCharacterReward(DrawInfoTemplate template, out RewardGoods? reward)
        {
            reward = null;
            int characterId = template.ResourceIds.GetValueOrDefault(1);
            if (characterId <= 0 || !IsCharacterId(characterId))
                return false;

            reward = CreateRewardGoods(RewardType.Character, characterId, 1, level: 1);
            return true;
        }

        private static bool TryCreateTargetEquipReward(DrawInfoTemplate template, out RewardGoods? reward)
        {
            reward = null;
            int equipId = template.ResourceIds.GetValueOrDefault(1);
            if (equipId <= 0 || !IsEquipId(equipId))
                return false;

            reward = CreateRewardGoods(RewardType.Equip, equipId, 1, level: 1);
            return true;
        }

        private static RewardGoods? DrawCharacterShardReward(DrawInfoTemplate template)
        {
            List<int> shardIds = drawPreviewTables
                .FirstOrDefault(x => x.Id == template.Id)
                ?.GoodsId
                .Select(characterId => charactersTables.FirstOrDefault(character => character.Id == characterId)?.ItemId ?? 0)
                .Where(Inventory.IsValidClientItemId)
                .Distinct()
                .ToList() ?? [];

            if (shardIds.Count == 0)
            {
                int characterId = template.ResourceIds.GetValueOrDefault(1);
                CharacterTable? targetCharacter = charactersTables.FirstOrDefault(x => x.Id == characterId);
                if (targetCharacter is not null && Inventory.IsValidClientItemId(targetCharacter.ItemId))
                    shardIds.Add(targetCharacter.ItemId);
            }

            int shardId = PickRandomId(shardIds);
            if (shardId <= 0)
                return DrawFallbackItemReward();

            int count = Random.Shared.Next(100) switch
            {
                < 10 => 18,
                < 35 => 6,
                _ => 2
            };
            return CreateRewardGoods(RewardType.Item, shardId, count);
        }

        private static RewardGoods? DrawMemoryReward()
        {
            List<EquipTable> memories = equipTables
                .Where(equip => equip.Type == 0
                    && equip.Quality == 4
                    && Character.IsOwnableEquipTemplate(equip)
                    && drawWaferShowIds.Contains(equip.Id))
                .ToList();
            if (memories.Count == 0)
                return DrawFallbackItemReward();

            EquipTable memory = memories[Random.Shared.Next(memories.Count)];
            return CreateRewardGoods(RewardType.Equip, memory.Id, 1, level: 1);
        }

        private static RewardGoods? DrawPreviewEquipReward(DrawInfoTemplate template)
        {
            DrawPreviewTable? preview = drawPreviewTables.FirstOrDefault(x => x.Id == template.Id);
            List<int> previewEquipIds = preview?.GoodsId
                .Where(IsEquipId)
                .ToList() ?? [];
            if (previewEquipIds.Count == 0)
                return null;

            int equipId = previewEquipIds[Random.Shared.Next(previewEquipIds.Count)];
            return CreateRewardGoods(RewardType.Equip, equipId, 1, level: 1);
        }

        private static RewardGoods? DrawRandomWeaponReward(int quality, int excludeEquipId = 0)
        {
            List<EquipTable> weapons = equipTables
                .Where(equip => equip.Type > 0
                    && equip.Quality == quality
                    && equip.Id != excludeEquipId
                    && Character.IsOwnableEquipTemplate(equip))
                .ToList();
            if (weapons.Count == 0)
                return DrawFallbackItemReward();

            EquipTable weapon = weapons[Random.Shared.Next(weapons.Count)];
            return CreateRewardGoods(RewardType.Equip, weapon.Id, 1, level: 1);
        }


        // Retail draw 1488 grants the overclock materials directly (captured 40110/40113, Count=1);
        // the unopened 60001/60002 boxes never appear in retail DrawDrawCardResponse payloads.
        private static RewardGoods? DrawOverclockMaterialReward()
        {
            return DrawItemRewardByIds([40110, 40111, 40112, 40113, 40114], fallbackCount: 1);
        }

        private static RewardGoods? DrawExpMaterialReward()
        {
            return DrawItemRewardByIds([30011, 30012, 30013, 30014, 31101, 31102, 31103, 31104, 31201, 31202, 31203, 31204], fallbackCount: 3);
        }

        private static RewardGoods? DrawCogBoxReward()
        {
            return DrawItemRewardByIds([90012, 90013, 90014, 90015], fallbackCount: 1);
        }

        private static RewardGoods? DrawFallbackItemReward()
        {
            return DrawOverclockMaterialReward() ?? DrawCogBoxReward();
        }

        private static RewardGoods? DrawItemRewardByIds(int[] ids, int fallbackCount)
        {
            HashSet<int> allowedIds = [.. ids];
            return DrawItemReward(item => allowedIds.Contains(item.Id) && item.Quality >= MinDrawItemShowQuality, fallbackCount);
        }

        private static RewardGoods? DrawItemReward(Func<ItemTable, bool> predicate, int fallbackCount)
        {
            List<ItemTable> pool = itemTables
                .Where(predicate)
                .ToList();
            if (pool.Count == 0)
                return null;

            ItemTable item = pool[Random.Shared.Next(pool.Count)];
            int count = item.Id switch
            {
                90014 or 90015 => 1,
                _ => fallbackCount
            };
            return CreateRewardGoods(RewardType.Item, item.Id, count);
        }

        private static int PickRandomId(List<int> ids)
        {
            return ids.Count == 0 ? 0 : ids[Random.Shared.Next(ids.Count)];
        }

        private static int GetFirstQuality(int characterId)
        {
            return characterQualitiesTables
                .Where(x => x.CharacterId == characterId)
                .OrderBy(x => x.Quality)
                .FirstOrDefault()?.Quality ?? 0;
        }

        private static RewardGoods CreateRewardGoods(RewardType type, int templateId, int count, int level = 0, int quality = 0)
        {
            return new RewardGoods
            {
                RewardType = (int)type,
                TemplateId = templateId,
                Count = count,
                Level = level,
                Quality = quality,
                IsGift = false,
                RewardMulti = 0
            };
        }


        private static bool IsCharacterId(int templateId)
        {
            return charactersTables.Any(x => x.Id == templateId);
        }

        private static bool IsEquipId(int templateId)
        {
            return equipTables.Any(x => x.Id == templateId && Character.IsOwnableEquipTemplate(x));
        }
    }
}
