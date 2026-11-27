using System.Globalization;
using System.Text;

namespace LmpCommon.CraftLibrary
{
    /// <summary>
    /// Makes a craft name from the network safe to show in screen messages and logs.
    /// Strips control and formatting characters and angle brackets, so a rejected name can't inject rich text or fake log lines.
    /// Long names are shortened with an ellipsis.
    /// </summary>
    public static class CraftNameDisplay
    {
        private const int MaxDisplayLength = 50;
        private const string Ellipsis = "...";
        private const string Placeholder = "?";

        public static string Sanitize(string name)
        {
            if (string.IsNullOrEmpty(name)) return Placeholder;

            var builder = new StringBuilder(name.Length);
            foreach (var character in name)
            {
                if (IsDisplayable(character)) builder.Append(character);
            }

            if (builder.Length == 0) return Placeholder;
            if (builder.Length <= MaxDisplayLength) return builder.ToString();

            var length = MaxDisplayLength;

            // Don't cut a surrogate pair in half
            if (char.IsHighSurrogate(builder[length - 1])) length--;

            return builder.ToString(0, length) + Ellipsis;
        }

        private static bool IsDisplayable(char character)
        {
            if (character == '<' || character == '>' || char.IsControl(character)) return false;

            // Format characters include the invisible ones that reorder or hide text, such as right-to-left overrides
            // Line and paragraph separators are not control characters but still break a line
            switch (CharUnicodeInfo.GetUnicodeCategory(character))
            {
                case UnicodeCategory.Format:
                case UnicodeCategory.LineSeparator:
                case UnicodeCategory.ParagraphSeparator:
                    return false;
                default:
                    return true;
            }
        }
    }
}
