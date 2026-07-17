using AscNet.Common.Database;
using AscNet.Common.MsgPack;
using AscNet.Common.Util;

namespace AscNet.GameServer.Commands
{
    [CommandName("bc")]
    internal class BlackCardCommand : Command
    {
        private const int DefaultBlackCardGrant = 30_000;

        public BlackCardCommand(Session session, string[] args, bool validate = true) : base(session, args, validate) { }

        public override string Help => "给当前在线玩家发放黑卡。用法：bc [数量|max]；默认发放 30000。";

        [Argument(0, @"^[1-9][0-9]*$|^max$", "要发放的黑卡数量，或 max", ArgumentFlags.Optional | ArgumentFlags.IgnoreCase)]
        string Amount { get; set; } = string.Empty;

        public override void Execute()
        {
            int amount = string.IsNullOrEmpty(Amount)
                ? DefaultBlackCardGrant
                : Amount.Equals("max", StringComparison.OrdinalIgnoreCase)
                    ? int.MaxValue
                    : Miscs.ParseIntOr(Amount);

            Item blackCards = session.inventory.Do(Inventory.FreeGem, amount);
            session.inventory.Save();
            session.SendPush(new NotifyItemDataList
            {
                ItemDataList = { blackCards }
            });
        }
    }
}
