using Server.Command.Command.Base;

namespace Server.Command.Command
{
    public class RestartServerCommand : SimpleCommand
    {
        //Executes the RestartServerCommand
        public override bool Execute(string commandArgs)
        {
            //Restart server
            RestartServer();
            return true;
        }

        //Restarts the server
        private static void RestartServer()
        {
            _ = MainServer.RestartAsync();
        }
    }
}
