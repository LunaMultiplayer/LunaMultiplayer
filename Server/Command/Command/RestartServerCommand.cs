using Server.Command.Command.Base;

namespace Server.Command.Command
{
    public class RestartServerCommand : SimpleCommand
    {
        //Executes the RestartServerCommand
        public override void Execute(string commandArgs)
        {
            //Restart server
            RestartServer();
        }

        //Restarts the server
        private static void RestartServer()
        {
            MainServer.Restart();
        }
    }
}