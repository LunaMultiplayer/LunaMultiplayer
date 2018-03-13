namespace Server.Command.Common
{
    public class CommandSystemHelperMethods
    {
        public static void SplitCommand(string command, out string param1, out string param2)
        {
            param2 = "";
            var splittedCommand = command.Split(' ');
            param1 = splittedCommand[0];

            if (splittedCommand.Length > 1)
                param2 = splittedCommand[1];
        }

        public static void SplitCommandParamArray(string command, out string[] parameters)
        {
            parameters = null;
            var paramArray = command.Split(' ');
            if (paramArray.Length > 0)
                if (!string.IsNullOrEmpty(paramArray[0]))
                    parameters = paramArray;
        }
    }
}