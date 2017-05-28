using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace LunaServer.Utilities
{
    public class ConfigParser<T> where T : class
    {
        public T Settings { get; }
        private readonly Dictionary<Type, Func<string, object>> _fromString = new Dictionary<Type, Func<string, object>>();
        private readonly string _filePath;

        /// <summary>
        /// The parser's constructor.
        /// </summary>
        /// <param name="settings">A class property.</param>
        /// <param name="filePath">The path to the file the parser should load/write values from/to.</param>
        public ConfigParser(T settings, string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _filePath = filePath;

            _fromString[typeof(string)] = x => x;
            _fromString[typeof(decimal)] = x => { if (decimal.TryParse(x, out var parse)) return parse; return null; };
            _fromString[typeof(short)] = x => { if (short.TryParse(x, out var parse)) return parse; return null; };
            _fromString[typeof(ushort)] = x => { if (ushort.TryParse(x, out var parse)) return parse; return null; };
            _fromString[typeof(int)] = x => { if (int.TryParse(x, out var parse)) return parse; return null; };
            _fromString[typeof(uint)] = x => { if (uint.TryParse(x, out var parse)) return parse; return null; };
            _fromString[typeof(long)] = x => { if (long.TryParse(x, out var parse)) return parse; return null; };
            _fromString[typeof(ulong)] = x => { if (ulong.TryParse(x, out var parse)) return parse; return null; };
            _fromString[typeof(float)] = x => { if (float.TryParse(x, out var parse)) return parse; return null; };
            _fromString[typeof(double)] = x => { if (double.TryParse(x, out var parse)) return parse; return null; };
            _fromString[typeof(bool)] = x => x == "1" || x.ToLower() == bool.TrueString.ToLower();
            _fromString[typeof(char)] = x => { if (char.TryParse(x, out var parse)) return parse; return null; };

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
        }

        #region Load Settings
        /// <summary>
        /// Loads the values from a file, specified in the parser's constructor.
        /// </summary>
        public void LoadSettings()
        {
            var settingFields = typeof(T).GetFields();

            if (!File.Exists(_filePath))
            {
                SaveSettings();
            }

            using (var fs = new FileStream(_filePath, FileMode.Open))
            using (var sr = new StreamReader(fs))
            {
                while (!sr.EndOfStream)
                {
                    var currentLine = sr.ReadLine();
                    if (currentLine == null)
                    {
                        break;
                    }

                    var trimmedLine = currentLine.Trim();
                    if (string.IsNullOrEmpty(trimmedLine))
                    {
                        continue;
                    }

                    if (!trimmedLine.Contains("=") || trimmedLine.StartsWith("#"))
                    {
                        continue;
                    }

                    var currentKey = trimmedLine.Substring(0, trimmedLine.IndexOf("=", StringComparison.Ordinal));
                    var currentValue = trimmedLine.Substring(trimmedLine.IndexOf("=", StringComparison.Ordinal) + 1);

                    foreach (var settingField in settingFields.Where(s => s.Name == currentKey))
                    {
                        //Enums
                        if (settingField.FieldType.IsEnum)
                        {
                            if (Enum.IsDefined(settingField.FieldType, currentValue))
                            {
                                var enumValue = Enum.Parse(settingField.FieldType, currentValue);
                                settingField.SetValue(Settings, enumValue);
                            }
                            else
                            {
                                if (settingField.FieldType.GetEnumUnderlyingType() == typeof(int))
                                {
                                    if (int.TryParse(currentValue, out var intValue))
                                    {
                                        if (Enum.IsDefined(settingField.FieldType, intValue))
                                        {
                                            settingField.SetValue(Settings, intValue);
                                        }
                                    }
                                }
                            }
                            continue;
                        }
                        //List
                        if (settingField.FieldType.IsGenericType && settingField.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                        {
                            var newList = Activator.CreateInstance(settingField.FieldType);
                            settingField.SetValue(Settings, newList);
                            var listType = settingField.FieldType.GetGenericArguments()[0];
                            if (!_fromString.ContainsKey(listType))
                            {
                                continue;
                            }
                            var addMethodInfo = newList.GetType().GetMethod("Add");
                            foreach (var splitValue in SplitArrayValues(currentValue))
                            {
                                var insertObject = _fromString[listType](splitValue);
                                if (insertObject != null)
                                {
                                    addMethodInfo.Invoke(newList, new[] { insertObject });
                                }
                            }
                            continue;
                        }
                        //Array
                        if (settingField.FieldType.IsArray)
                        {
                            var elementType = settingField.FieldType.GetElementType();
                            if (!_fromString.ContainsKey(elementType))
                            {
                                var emptyArray = Activator.CreateInstance(settingField.FieldType, 0);
                                settingField.SetValue(Settings, emptyArray);
                                continue;
                            }
                            var genericListType = typeof(List<>).MakeGenericType(elementType);
                            var newList = Activator.CreateInstance(genericListType);
                            var addMethodInfo = genericListType.GetMethod("Add");
                            foreach (var splitValue in SplitArrayValues(currentValue))
                            {
                                var insertObject = _fromString[elementType](splitValue);
                                if (insertObject != null)
                                {
                                    addMethodInfo.Invoke(newList, new[] { insertObject });
                                }
                            }
                            var toArrayMethodInfo = genericListType.GetMethod("ToArray");
                            var newArray = toArrayMethodInfo.Invoke(newList, new object[0]);
                            settingField.SetValue(Settings, newArray);
                            continue;
                        }
                        //Field
                        if (_fromString.ContainsKey(settingField.FieldType))
                        {
                            var parseValue = _fromString[settingField.FieldType](currentValue);
                            if (parseValue != null)
                            {
                                settingField.SetValue(Settings, parseValue);
                            }
                        }
                    }
                }
            }
            SaveSettings();
        }
        #endregion
        #region Save Settings
        /// <summary>
        /// Saves the settings to a file, specified in the parser's constructor.
        /// </summary>
        public void SaveSettings()
        {
            var settingFields = typeof(T).GetFields();
            if (File.Exists($"{_filePath}.tmp"))
            {
                File.Delete($"{_filePath}.tmp");
            }
            using (var fs = new FileStream($"{_filePath}.tmp", FileMode.CreateNew))
            {
                using (var sw = new StreamWriter(fs))
                {
                    sw.WriteLine("# Lines starting with hashtags are ignored by the reader");
                    sw.WriteLine("# Setting file format: (key)=(value)");
                    sw.WriteLine("#");
                    sw.WriteLine("# Invalid values will be reset to default");
                    sw.WriteLine("#");
                    sw.WriteLine("");
                    foreach (var settingField in settingFields)
                    {
                        var descriptionAttribute = settingField.GetCustomAttributes(typeof(DescriptionAttribute), true).FirstOrDefault();
                        var settingDescription = ((DescriptionAttribute)descriptionAttribute)?.Description.Replace("\n", $"{Environment.NewLine}# ") ?? string.Empty;

                        if (!string.IsNullOrEmpty(settingDescription))
                        {
                            sw.WriteLine("# {0} - {1}", settingField.Name, settingDescription);
                        }
                        if (settingField.FieldType == typeof(string))
                        {
                            sw.WriteLine("{0}={1}", settingField.Name, settingField.GetValue(Settings));
                        }
                        if (settingField.FieldType == typeof(int) || settingField.FieldType == typeof(uint) || settingField.FieldType == typeof(short) || settingField.FieldType == typeof(long) || settingField.FieldType == typeof(ushort) || settingField.FieldType == typeof(ulong) || settingField.FieldType == typeof(float) || settingField.FieldType == typeof(double) || settingField.FieldType == typeof(decimal) || settingField.FieldType == typeof(bool))
                        {
                            sw.WriteLine("{0}={1}", settingField.Name, settingField.GetValue(Settings));
                        }
                        if (settingField.FieldType.IsEnum)
                        {
                            sw.WriteLine("#");
                            sw.WriteLine("# Valid values are:");
                            foreach (var enumValue in settingField.FieldType.GetEnumValues())
                            {
                                sw.WriteLine("#   {0}", enumValue);
                            }
                            sw.WriteLine("{0}={1}", settingField.Name, settingField.GetValue(Settings));
                        }
                        if (settingField.FieldType.IsGenericType && settingField.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                        {
                            //Get list
                            var settingsList = settingField.GetValue(Settings);
                            //Get enumerator
                            var iEnumeratorInfo = settingField.FieldType.GetMethod("GetEnumerator");
                            var listEnumerator = iEnumeratorInfo.Invoke(settingsList, new object[0]);
                            //Get enumerator methods
                            var moveNextInfo = listEnumerator.GetType().GetMethod("MoveNext");
                            var propertyInfo = listEnumerator.GetType().GetProperty("Current");
                            if (propertyInfo != null)
                            {
                                var currentInfo = propertyInfo.GetGetMethod();
                                var disposeInfo = listEnumerator.GetType().GetMethod("Dispose");
                                var escapedList = new List<string>();
                                //Foreach object in list...
                                while ((bool)moveNextInfo.Invoke(listEnumerator, null))
                                {
                                    var current = currentInfo.Invoke(listEnumerator, null);
                                    escapedList.Add(EscapeString(current.ToString()));
                                }
                                disposeInfo.Invoke(listEnumerator, null);
                                sw.WriteLine("{0}={1}", settingField.Name, string.Join(",", escapedList));
                            }
                        }
                        if (settingField.FieldType.IsArray)
                        {
                            var enumeratorInfo = settingField.FieldType.GetMethod("GetEnumerator");
                            var settingsList = settingField.GetValue(Settings);
                            var objectEnum = (IEnumerator)enumeratorInfo.Invoke(settingsList, new object[0]);
                            var escapedList = new List<string>();
                            while (objectEnum.MoveNext())
                            {
                                escapedList.Add(EscapeString(objectEnum.Current.ToString()));
                            }
                            sw.WriteLine("{0}={1}", settingField.Name, string.Join(",", escapedList));
                        }
                        sw.WriteLine();
                    }
                }
            }
            if (File.Exists(_filePath))
            {
                File.Delete(_filePath);
            }
            File.Move($"{_filePath}.tmp", _filePath);
        }
        #endregion


        public static IEnumerable<string> SplitArrayValues(string inputString)
        {
            var retList = new List<string>();
            var isEscaped = false;
            var sb = new StringBuilder();
            foreach (var currentChar in inputString)
            {
                if (isEscaped)
                {
                    isEscaped = false;
                    if (currentChar == '\\')
                    {
                        sb.Append('\\');
                    }
                    if (currentChar == ',')
                    {
                        sb.Append(',');
                    }
                    if (currentChar == 'n')
                    {
                        sb.AppendLine();
                    }
                }
                else
                {
                    switch (currentChar)
                    {
                        case '\\':
                            isEscaped = true;
                            break;
                        case ',':
                            retList.Add(sb.ToString());
                            sb.Clear();
                            break;
                        default:
                            sb.Append(currentChar);
                            break;
                    }
                }
            }
            if (sb.Length != 0)
            {
                retList.Add(sb.ToString());
            }
            return retList;
        }

        public static string EscapeString(string inputString)
        {
            var sb = new StringBuilder();
            foreach (var currentChar in inputString)
            {
                switch (currentChar)
                {
                    case '\\':
                        sb.Append(@"\\");
                        continue;
                    case ',':
                        sb.Append(@"\,");
                        continue;
                    case '\r':
                        continue;
                    case '\n':
                        sb.Append(@"\n");
                        continue;
                }
                sb.Append(currentChar);
            }
            return sb.ToString();
        }
    }
}