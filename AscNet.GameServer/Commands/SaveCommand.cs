namespace AscNet.GameServer.Commands
{
    [CommandName("save")]
    internal class SaveCommand : Command
    {
        public SaveCommand(Session session, string[] args, bool validate = true) : base(session, args, validate) { }

        public override string Help => "将当前会话状态保存到数据库。";

        public override void Execute()
        {
            session.Save();
        }
    }
}
