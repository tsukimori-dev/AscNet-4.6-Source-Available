using AscNet.Common.MsgPack;
using AscNet.Common.Util;
using AscNet.GameServer.Handlers;
using AscNet.Table.V2.share.player;

namespace AscNet.GameServer.Commands
{
    [CommandName("level")]
    internal class LevelCommand : Command
    {
        public LevelCommand(Session session, string[] args, bool validate = true) : base(session, args, validate) { }

        public override string Help => "修改指挥官等级。";

        [Argument(0, @"^[0-9]+$|^max$", "目标等级，填写数字或 max")]
        string Level { get; set; } = string.Empty;

        public override void Execute()
        {
            List<PlayerTable> playerLevels = TableReaderV2.Parse<PlayerTable>();
            int level = Miscs.ParseIntOr(Level);

            if (Level == "max")
            {
                session.player.PlayerData.Level = playerLevels.OrderByDescending(x => x.Level).First().Level;
                NotifyPlayerLevel notifyPlayerLevel = new()
                {
                    Level = (int)session.player.PlayerData.Level
                };
                session.SendPush(notifyPlayerLevel);
                session.ExpSanityCheck();
            }
            else if (playerLevels.Any(x => x.Level == level))
            {
                if (session.player.PlayerData.Level > level)
                {
                    session.player.PlayerData.Level = level;
                    // Waiting for SendChatResponse, pls fix later
                    Task.Run(() =>
                    {
                        // ReconnectPlayerLogout
                        session.SendPush(new ForceLogoutNotify() { Code = 1030 });
                        session.DisconnectProtocol();
                    });
                    return;
                }
                session.player.PlayerData.Level = level;
                NotifyPlayerLevel notifyPlayerLevel = new()
                {
                    Level = (int)session.player.PlayerData.Level
                };
                session.SendPush(notifyPlayerLevel);
                session.ExpSanityCheck();
            }
            else
            {
                throw new ArgumentException("等级无效！");
            }
        }
    }
}
