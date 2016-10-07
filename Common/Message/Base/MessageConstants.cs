namespace LunaCommon.Message.Base
{
    /// <summary>
    /// Message header specifications
    /// </summary>
    public class MessageConstants
    {
        public const int HeaderLength = 9;

        public const int MessageTypeStartIndex = 0;
        public const int MessageTypeLength = 2;
        public const int MessageTypeEndIndex = 1;

        public const int MessageSubTypeStartIndex = 2;
        public const int MessageSubTypeLength = 2;
        public const int MessageSubTypeEndIndex = 3;

        public const int MessageLengthStartIndex = 4;
        public const int MessageLengthLength = 4;
        public const int MessageLengthEndIndex = 7;

        public const int MessageCompressionValueIndex = 8;
        public const int MessageCompressionValueLength = 1;
    }
}