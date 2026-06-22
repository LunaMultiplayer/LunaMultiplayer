using System.Text.RegularExpressions;
using System.Linq;

namespace Server.Command.Common
{
    public class CommandSystemHelperMethods
    {
        public static void SplitCommandParamArray(string command, out string[] parameters)
        {
            parameters = null;
            var paramArray = Regex.Matches(command, "\"[^\"]+\"|[^ \"]+")
            .Cast<Match>()
            .Select(m => m.Value.Trim('"'))
            .ToArray();

            if (paramArray.Length > 0)
                if (!string.IsNullOrEmpty(paramArray[0]))
                    parameters = paramArray;
        }

        public static void SplitCommand(string command, out string param1, out string param2)
        {
            SplitCommandParamArray(command, out var parameters);
            param1 = "";
            if (parameters != null && parameters.Length > 0)
                param1 = parameters[0];

            param2 = "";
            if (parameters != null && parameters.Length > 1)
                param2 = parameters[1];
        }
    }
}
