using AscNet.Common.Util;
using AscNet.Common;
using AscNet.Common.Database;
using AscNet.Table.V2.share.equip;
using AscNet.Common.MsgPack;
using AscNet.GameServer.Handlers;

namespace AscNet.GameServer.Commands
{
    [CommandName("equip")]
    internal class EquipCommand : Command
    {

        public EquipCommand(Session session, string[] args, bool validate = true) : base(session, args, validate)
        {
        }

        public override string Help => "管理当前账号的武器与意识。";

        [Argument(0, @"^add$|^clear$|^sync$", "要执行的操作（add、clear、sync）", ArgumentFlags.IgnoreCase)]
        string Op { get; set; } = string.Empty;

        [Argument(1, @"^[0-9]+$|^all$", "目标装备，填写装备 ID 或 all", ArgumentFlags.IgnoreCase | ArgumentFlags.Optional)]
        string Target { get; set; } = string.Empty;

        public override void Execute()
        {
            if (Op.Equals("sync", StringComparison.OrdinalIgnoreCase))
            {
                SyncEquipsFromDatabase();
                return;
            }

            NotifyEquipDataList notifyEquipData = new();


            switch (Op)
            {
                case "add":
                    if (Target == "all")
                    {
                        foreach (EquipTable equip in TableReaderV2.Parse<EquipTable>().Where(IsSafeBatchEquipTemplate))
                        {
                            var newEquip = session.character.AddEquip((uint)equip.Id);
                            if (newEquip is not null)
                                notifyEquipData.EquipDataList.Add(newEquip);
                        }
                    }
                    else
                    {
                        var equip = TableReaderV2.Parse<EquipTable>().Find(x => x.Id == Miscs.ParseIntOr(Target)) ?? throw new ServerCodeException("未找到对应 ID 的装备", 20021001);
                        var newEquip = session.character.AddEquip((uint)equip.Id);
                        if (newEquip is not null)
                            notifyEquipData.EquipDataList.Add(newEquip);
                    }
                    break;
                case "clear":
                    ClearEquips(notifyEquipData);
                    break;
                default:
                    throw new InvalidOperationException("无效的操作！");
            }

            session.SendPush(notifyEquipData);
        }

        private static bool IsSafeBatchEquipTemplate(EquipTable equip)
        {
            // 临时兼容：当前客户端缺少部分特殊意识套装配置，批量发放会导致仓库意识页反复查表报错并卡顿。
            // 在客户端配置补齐前，/equip add all 先跳过这些特殊意识。
            //服务端缺少我能理解，客户端缺少什么鬼？
            return Character.IsOwnableEquipTemplate(equip)
                && !IsSpecialAwarenessTemplate(equip);
        }

        private static bool IsSpecialAwarenessTemplate(EquipTable equip)
        {
            // 特殊/活动意识套装目前存在服务端表比客户端表新的情况，SuitId >= 20000 的意识批量下发不行。
            return equip.Type == 0 && equip.Site > 0 && equip.SuitId >= 20000;
        }

        private void ClearEquips(NotifyEquipDataList notifyEquipData)
        {
            if (Target == "all")
            {
                List<uint> deletedEquipIds = session.character.Equips
                    .Where(equip => equip.CharacterId == 0)
                    .Select(equip => equip.Id)
                    .ToList();

                session.character.Equips.RemoveAll(equip => deletedEquipIds.Contains(equip.Id));
                notifyEquipData.DeletedEquipIdList.AddRange(deletedEquipIds);
                session.character.Save();
                return;
            }

            if (string.IsNullOrEmpty(Target))
                throw new ArgumentException("请指定要清理的装备 ID 或 all！");

            uint equipId = (uint)Miscs.ParseIntOr(Target);
            EquipData? equip = session.character.Equips.FirstOrDefault(x => x.Id == equipId);
            if (equip is null)
                throw new ArgumentException("目标装备 ID 无效！");

            if (equip.CharacterId != 0)
                throw new ArgumentException("目标装备已装备，不能清理！");

            session.character.Equips.Remove(equip);
            notifyEquipData.DeletedEquipIdList.Add(equipId);
            session.character.Save();
        }

        private void SyncEquipsFromDatabase()
        {
            session.character = Character.FromUid(session.player.PlayerData.Id);
            AccountModule.SendLoginState(session);
        }
    }
}
