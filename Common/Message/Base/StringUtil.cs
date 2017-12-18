using System.Text;

namespace LunaCommon.Message.Base
{
    public static class StringUtil
    {
        public static int GetByteCount(this string[] array, int length)
        {
            var count = 0;
            for (var i = 0; i < length; i++)
            {
                count += array[i].GetByteCount();
            }

            return count;
        }

        public static int GetByteCount(this string stringToCheck)
        {
            if (string.IsNullOrEmpty(stringToCheck))
                return sizeof(int);

            //Lidgren writes the string length so it uses more bytes (usually less, but it's better to overshoot)
            return Encoding.UTF8.GetByteCount(stringToCheck) + sizeof(int);
        }
    }
}
