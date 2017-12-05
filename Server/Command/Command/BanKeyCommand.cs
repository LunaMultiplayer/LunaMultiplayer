using Server.Client;
using Server.Command.Command.Base;
using Server.Command.Common;
using Server.Log;
using Server.Server;

namespace Server.Command.Command
{
    public class BanKeyCommand : HandledCommand
    {
        protected override string FileName => "LMPKeyBans.txt";
        protected override object CommandLock { get; } = new object();

        public override void Execute(string commandArgs)
        {
            CommandSystemHelperMethods.SplitCommand(commandArgs, out var publicKey, out var reason);
            reason = string.IsNullOrEmpty(reason) ? "No reason specified" : reason;

            var player = ClientRetriever.GetClientByPublicKey(publicKey);
            if (player != null)
                MessageQueuer.SendConnectionEnd(player, $"You were banned from the server: {reason}");

            Add(publicKey);
            LunaLog.Normal($"Public key '{publicKey}' was banned from the server: {reason}");
        }
    }
}