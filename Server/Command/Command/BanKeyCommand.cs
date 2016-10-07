using LunaServer.Client;
using LunaServer.Command.Command.Base;
using LunaServer.Command.Common;
using LunaServer.Log;
using LunaServer.Server;

namespace LunaServer.Command.Command
{
    public class BanKeyCommand : HandledCommand
    {
        protected override string FileName => "LMPKeyBans.txt";
        protected override object CommandLock { get; } = new object();

        public override void Execute(string commandArgs)
        {
            string publicKey;
            string reason;
            CommandSystemHelperMethods.SplitCommand(commandArgs, out publicKey, out reason);
            reason = string.IsNullOrEmpty(reason) ? "No reason specified" : reason;

            var player = ClientRetriever.GetClientByPublicKey(publicKey);
            if (player != null)
                MessageQueuer.SendConnectionEnd(player, $"You were banned from the server: {reason}");

            Add(publicKey);
            LunaLog.Normal($"Public key '{publicKey}' was banned from the server: {reason}");
        }
    }
}