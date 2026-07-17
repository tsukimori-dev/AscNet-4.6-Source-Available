using AscNet.Common.Database;
using AscNet.Common.Util;
using AscNet.Common.MsgPack;
using AscNet.GameServer.Handlers;
using AscNet.Table.V2.share.character;

namespace AscNet.GameServer.Commands
{
    [CommandName("character")]
    internal class CharacterCommand : Command
    {
        public CharacterCommand(Session session, string[] args, bool validate = true) : base(session, args, validate) { }

        public override string Help => "修改角色数据。";

        [Argument(0, @"^add$", "要执行的操作（add）")]
        string Op { get; set; } = string.Empty;

        [Argument(1, @"^[0-9]+$|^all$", "目标角色，填写角色 ID 或 all")]
        string Target { get; set; } = string.Empty;

        public override void Execute()
        {
            int id = Miscs.ParseIntOr(Target);

            switch (Op)
            {
                case "add":
                    if (Target == "all")
                    {
                        var rewards = TableReaderV2
                            .Parse<CharacterTable>()
                            .Where(x => !session.character.Characters.Any(y => y.Id == x.Id))
                            .Select(x => new Reward { Id = x.Id, Type = RewardType.Character });

                        RewardHandler.GiveRewards(rewards, session);
                    }
                    else
                    {
                        RewardHandler.GiveRewards([ new Reward() { Id = id, Type = RewardType.Character } ], session);
                    }
                    break;
                default:
                    throw new InvalidOperationException("无效的操作！");
            }
        }
    }
}
