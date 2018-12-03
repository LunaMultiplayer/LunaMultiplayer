using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace LmpClient.Extensions
{
    /// <summary>
    /// Dumps all the fields and properties of an object. Found here: https://stackoverflow.com/questions/852181/c-printing-all-properties-of-an-object
    /// </summary>
    public static class ObjectDumper
    {
        private static int _level;
        private static readonly StringBuilder StringBuilder = new StringBuilder();
        private static readonly HashSet<int> HashListOfFoundElements = new HashSet<int>();
        
        public static string Dump(this object element, int indentSize = 2)
        {
            StringBuilder.Length = 0;
            HashListOfFoundElements.Clear();

            return DumpElement(element, indentSize);
        }

        private static string DumpElement(object element, int indentSize)
        {
            if (element == null || element is ValueType || element is string)
            {
                Write(FormatValue(element), indentSize);
            }
            else
            {
                var objectType = element.GetType();
                if (!typeof(IEnumerable).IsAssignableFrom(objectType))
                {
                    Write("{{{0}}}", indentSize, objectType.FullName);
                    HashListOfFoundElements.Add(element.GetHashCode());
                    _level++;
                }

                if (element is IEnumerable enumerableElement)
                {
                    foreach (var item in enumerableElement)
                    {
                        if (item is IEnumerable && !(item is string))
                        {
                            _level++;
                            DumpElement(item, indentSize);
                            _level--;
                        }
                        else
                        {
                            if (!AlreadyTouched(item))
                                DumpElement(item, indentSize);
                            else
                                Write("{{{0}}} <-- bidirectional reference found", indentSize, item.GetType().FullName);
                        }
                    }
                }
                else
                {
                    var members = element.GetType().GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                    foreach (var memberInfo in members)
                    {
                        var fieldInfo = memberInfo as FieldInfo;
                        var propertyInfo = memberInfo as PropertyInfo;

                        if (fieldInfo == null && propertyInfo == null)
                            continue;

                        var type = fieldInfo != null ? fieldInfo.FieldType : propertyInfo.PropertyType;
                        var value = fieldInfo != null
                                           ? fieldInfo.GetValue(element)
                                           : propertyInfo.GetValue(element, null);

                        if (type.IsValueType || type == typeof(string))
                        {
                            Write("{0}: {1}", indentSize, memberInfo.Name, FormatValue(value));
                        }
                        else
                        {
                            var isEnumerable = typeof(IEnumerable).IsAssignableFrom(type);
                            Write("{0}: {1}", indentSize, memberInfo.Name, isEnumerable ? "..." : "{ }");

                            var alreadyTouched = !isEnumerable && AlreadyTouched(value);
                            _level++;
                            if (!alreadyTouched)
                                DumpElement(value, indentSize);
                            else
                                Write("{{{0}}} <-- bidirectional reference found", indentSize, value.GetType().FullName);
                            _level--;
                        }
                    }
                }

                if (!typeof(IEnumerable).IsAssignableFrom(objectType))
                {
                    _level--;
                }
            }

            return StringBuilder.ToString();
        }

        private static bool AlreadyTouched(object value)
        {
            if (value == null)
                return false;

            var hash = value.GetHashCode();
            return HashListOfFoundElements.Contains(hash);
        }

        private static void Write(string value, int indentSize, params object[] args)
        {
            var space = new string(' ', _level * indentSize);

            if (args != null)
                value = string.Format(value, args);

            StringBuilder.AppendLine(space + value);
        }

        private static string FormatValue(object o)
        {
            switch (o)
            {
                case null:
                    return "null";
                case DateTime time:
                    return time.ToShortDateString();
                case string _:
                    return $"\"{o}\"";
                case char c when c == '\0':
                    return string.Empty;
                case ValueType _:
                    return o.ToString();
                case IEnumerable _:
                    return "...";
                default:
                    return "{ }";
            }
        }
    }
}
