using AscNet.Common.MsgPack;
using AscNet.Common.Util;
using AscNet.GameServer.Handlers;
using AscNet.Table.V2.share.fashion;

namespace AscNet.GameServer.Commands
{
    [CommandName("coating")]
    internal class CoatingCommand : Command
    {
        public CoatingCommand(Session session, string[] args, bool validate = true) : base(session, args, validate) { }

        public override string Help => "解锁角色涂装。";

        [Argument(0, @"^unlock$", "要执行的操作（unlock）")]
        string Op { get; set; } = string.Empty;

        [Argument(1, @"^[0-9]+$|^all$", "目标角色，填写角色 ID 或 all（全部已拥有角色）")]
        string Target { get; set; } = string.Empty;

        public override void Execute()
        {
            int characterId = Miscs.ParseIntOr(Target);

            switch (Op)
            {
                case "unlock":
                    if (Target == "all")
                    {
                        List<FashionList> newFashions = new();
                        foreach (var fashion in TableReaderV2.Parse<FashionTable>().Where(x => session.character.Characters.Any(y => y.Id == x.CharacterId)))
                        {
                            if (session.character.Fashions.Any(x => x.Id == fashion.Id))
                                continue;

                            newFashions.Add(new() { Id = fashion.Id });
                        }

                        session.SendPush(new FashionSyncNotify() { FashionList = newFashions });
                        session.character.Fashions.AddRange(newFashions);
                    }
                    else
                    {
                        List<FashionList> newFashions = new();
                        foreach (var fashion in TableReaderV2.Parse<FashionTable>().Where(x => x.CharacterId == characterId))
                        {
                            if (session.character.Fashions.Any(x => x.Id == fashion.Id))
                                continue;

                            newFashions.Add(new() { Id = fashion.Id });
                        }

                        session.SendPush(new FashionSyncNotify() { FashionList = newFashions });
                        session.character.Fashions.AddRange(newFashions);
                    }

                    break;
                default:
                    throw new InvalidOperationException("无效的操作！");
            }
        }
    }
}
