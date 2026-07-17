using AscNet.Common;
using AscNet.Common.MsgPack;
using AscNet.Common.Database;
using AscNet.Common.Util;
using AscNet.Table.V2.share.item;
using Newtonsoft.Json;

namespace AscNet.GameServer.Commands
{
    [CommandName("item")]
    internal class ItemCommand : Command
    {
        private static readonly HashSet<int> MaxBatchResearchTicketIds = [50000, 50003, 50005, 50009];

        public ItemCommand(Session session, string[] args, bool validate = true) : base(session, args, validate) { }

        public override string Help => "管理当前账号的道具。";

        [Argument(0, @"^add$|^clear$|^reset$", "要执行的操作（add、clear、reset）", ArgumentFlags.IgnoreCase)]
        string Op { get; set; } = string.Empty;

        [Argument(1, @"^[0-9]+$|^all$", "目标道具，填写道具 ID 或 all", ArgumentFlags.Optional)]
        string Target { get; set; } = string.Empty;

        [Argument(2, @"^[0-9]+$|^max$", "目标数量，填写数字或 max（非 max 时受游戏上限限制）", ArgumentFlags.Optional)]
        string Amount { get; set; } = string.Empty;

        public override void Execute()
        {
            switch (Op.ToLowerInvariant())
            {
                case "add":
                    AddItems();
                    break;
                case "clear":
                    ClearItems();
                    break;
                case "reset":
                    ResetItems();
                    break;
                default:
                    throw new InvalidOperationException("无效的操作！");
            }
        }

        private void AddItems()
        {
            NotifyItemDataList notifyItemData = new();

            if (Target == "all")
            {
                if (string.IsNullOrEmpty(Amount))
                    throw new ArgumentException("请指定目标数量，填写数字或 max！");

                bool useMaxCount = Amount.Equals("max", StringComparison.OrdinalIgnoreCase);
                int amount = useMaxCount ? 0 : Miscs.ParseIntOr(Amount);

                foreach (ItemTable itemTable in TableReaderV2.Parse<ItemTable>())
                {
                    Item? existingItem = session.inventory.Items.FirstOrDefault(x => x.Id == itemTable.Id);
                    long currentCount = existingItem?.Count ?? 0;
                    long targetCount = GetBatchAddMaxTargetCount(itemTable);
                    long grantAmount = useMaxCount
                        ? targetCount - currentCount
                        : Math.Min(amount, targetCount - currentCount);
                    notifyItemData.ItemDataList.Add(
                        session.inventory.Do(itemTable.Id, (int)Math.Clamp(grantAmount, int.MinValue, int.MaxValue)));
                }
            }
            else
            {
                if (string.IsNullOrEmpty(Target) || string.IsNullOrEmpty(Amount))
                    throw new ArgumentException("目标或数量无效！");

                int itemId = Miscs.ParseIntOr(Target);
                if (!TableReaderV2.Parse<ItemTable>().Any(x => x.Id == itemId))
                    throw new ArgumentException("目标道具 ID 无效！");

                notifyItemData.ItemDataList.Add(session.inventory.Do(itemId, Miscs.ParseIntOr(Amount)));
            }

            session.inventory.Save();
            session.SendPush(notifyItemData);
        }

        private void ClearItems()
        {
            NotifyItemDataList notifyItemData = new();

            if (Target == "all")
            {
                foreach (Item item in Inventory.FilterClientItems(session.inventory.Items))
                {
                    item.Count = 0;
                    item.RefreshTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                    notifyItemData.ItemDataList.Add(item);
                }

                session.inventory.Items.RemoveAll(item => Inventory.IsValidClientItemId(item.Id));
            }
            else
            {
                if (string.IsNullOrEmpty(Target))
                    throw new ArgumentException("请指定要清空的道具 ID 或 all！");

                int itemId = Miscs.ParseIntOr(Target);
                if (!TableReaderV2.Parse<ItemTable>().Any(x => x.Id == itemId))
                    throw new ArgumentException("目标道具 ID 无效！");

                Item clearedItem = session.inventory.Do(itemId, int.MinValue);
                notifyItemData.ItemDataList.Add(clearedItem);
                session.inventory.Items.RemoveAll(item => item.Id == itemId);
            }

            session.inventory.Save();
            session.SendPush(notifyItemData);
        }

        private void ResetItems()
        {
            List<ItemConfig> defaultItems =
                JsonConvert.DeserializeObject<List<ItemConfig>>(File.ReadAllText("./Configs/default_items.json")) ?? new();
            Dictionary<int, ItemTable> itemTables = TableReaderV2.Parse<ItemTable>().ToDictionary(x => x.Id);
            Dictionary<int, long> defaultItemMap = defaultItems.ToDictionary(
                x => x.Id,
                x => Math.Min(x.Count, Inventory.GetMaxCount(itemTables.GetValueOrDefault(x.Id))));
            NotifyItemDataList notifyItemData = new();

            if (Target == "all")
            {
                List<Item> clientItems = Inventory.FilterClientItems(session.inventory.Items);
                foreach (Item item in clientItems)
                {
                    item.Count = defaultItemMap.GetValueOrDefault(item.Id, 0);
                    item.RefreshTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                    notifyItemData.ItemDataList.Add(item);
                }

                session.inventory.Items.RemoveAll(item => Inventory.IsValidClientItemId(item.Id));

                foreach (ItemConfig defaultItem in defaultItems.Where(item => itemTables.ContainsKey(item.Id)))
                {
                    Item newItem = new()
                    {
                        Id = defaultItem.Id,
                        Count = defaultItemMap[defaultItem.Id],
                        RefreshTime = DateTimeOffset.Now.ToUnixTimeSeconds(),
                        CreateTime = DateTimeOffset.Now.ToUnixTimeSeconds()
                    };
                    session.inventory.Items.Add(newItem);

                    if (!clientItems.Any(item => item.Id == defaultItem.Id))
                        notifyItemData.ItemDataList.Add(newItem);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(Target))
                    throw new ArgumentException("请指定要重置的道具 ID 或 all！");

                int itemId = Miscs.ParseIntOr(Target);
                if (!itemTables.ContainsKey(itemId))
                    throw new ArgumentException("目标道具 ID 无效！");

                long targetCount = defaultItemMap.GetValueOrDefault(itemId, 0);
                Item? existingItem = session.inventory.Items.FirstOrDefault(x => x.Id == itemId);
                if (existingItem is null)
                {
                    existingItem = new Item
                    {
                        Id = itemId,
                        Count = targetCount,
                        RefreshTime = DateTimeOffset.Now.ToUnixTimeSeconds(),
                        CreateTime = DateTimeOffset.Now.ToUnixTimeSeconds()
                    };
                    session.inventory.Items.Add(existingItem);
                }
                else
                {
                    existingItem.Count = targetCount;
                    existingItem.RefreshTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                }

                notifyItemData.ItemDataList.Add(existingItem);
            }

            session.inventory.Save();
            session.SendPush(notifyItemData);
        }

        private static long GetBatchAddMaxTargetCount(ItemTable itemTable)
        {
            // Gift and selection boxes are rendered as individual entries by the client.
            // Keep one copy during a bulk grant to avoid expanding the inventory excessively.
            if (itemTable.ItemType == (int)ItemType.Gift)
                return 1;

            long maxCount = Inventory.GetMaxCount(itemTable);
            if (MaxBatchResearchTicketIds.Contains(itemTable.Id))
                return maxCount;

            long gridCount = itemTable.GridCount ?? maxCount;
            return gridCount > 0 ? Math.Min(maxCount, gridCount) : maxCount;
        }
    }
}
