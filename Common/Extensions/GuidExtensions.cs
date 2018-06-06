using System;

namespace LunaCommon.Extensions
{
    public static class GuidExtensions
    {
        public static string ToSmallString(this Guid value)
        {
            return value.ToString("D").Substring(0, 8);
        }
    }
}
