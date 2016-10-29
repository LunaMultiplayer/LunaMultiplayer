using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;

namespace LunaCommon
{
    public class ConfigParser<T> where T : class
    {
        public T Settings { get; private set; }
        private Dictionary<Type, Func<string, object>> fromString = new Dictionary<Type, Func<string, object>>();
        private string filePath;

        /// <summary>
        /// The parser's constructor.
        /// </summary>
        /// <param name="settings">A class property.</param>
        /// <param name="filePath">The path to the file the parser should load/write values from/to.</param>
        public ConfigParser(T settings, string filePath)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException("filePath");
            }

            this.Settings = settings;
            this.filePath = filePath;

            fromString[typeof(string)] = (string x) => x;
            fromString[typeof(decimal)] = (string x) => { decimal parse; if (decimal.TryParse(x, out parse)) return parse; return null; };
            fromString[typeof(short)] = (string x) => { short parse; if (short.TryParse(x, out parse)) return parse; return null; };
            fromString[typeof(ushort)] = (string x) => { ushort parse; if (ushort.TryParse(x, out parse)) return parse; return null; };
            fromString[typeof(int)] = (string x) => { int parse; if (int.TryParse(x, out parse)) return parse; return null; };
            fromString[typeof(uint)] = (string x) => { uint parse; if (uint.TryParse(x, out parse)) return parse; return null; };
            fromString[typeof(long)] = (string x) => { long parse; if (long.TryParse(x, out parse)) return parse; return null; };
            fromString[typeof(ulong)] = (string x) => { ulong parse; if (ulong.TryParse(x, out parse)) return parse; return null; };
            fromString[typeof(float)] = (string x) => { float parse; if (float.TryParse(x, out parse)) return parse; return null; };
            fromString[typeof(double)] = (string x) => { double parse; if (double.TryParse(x, out parse)) return parse; return null; };
            fromString[typeof(bool)] = (string x) => { return (x == "1" || x.ToLower() == bool.TrueString.ToLower()); };
            fromString[typeof(char)] = (string x) => { char parse; if (char.TryParse(x, out parse)) return parse; return null; };

            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en-US");
        }

        #region Load Settings
        /// <summary>
        /// Loads the values from a file, specified in the parser's constructor.
        /// </summary>
        public void LoadSettings()
        {
            FieldInfo[] settingFields = typeof(T).GetFields();

            if (!File.Exists(filePath))
            {
                SaveSettings();
            }

            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    while (!sr.EndOfStream)
                    {
                        string currentLine = sr.ReadLine();
                        if (currentLine == null)
                        {
                            break;
                        }

                        string trimmedLine = currentLine.Trim();
                        if (String.IsNullOrEmpty(trimmedLine))
                        {
                            continue;
                        }

                        if (!trimmedLine.Contains("=") || trimmedLine.StartsWith("#"))
                        {
                            continue;
                        }

                        string currentKey = trimmedLine.Substring(0, trimmedLine.IndexOf("="));
                        string currentValue = trimmedLine.Substring(trimmedLine.IndexOf("=") + 1);

                        foreach (FieldInfo settingField in settingFields)
                        {
                            if (settingField.Name == currentKey)
                            {
                                //Enums
                                if (settingField.FieldType.IsEnum)
                                {
                                    if (Enum.IsDefined(settingField.FieldType, currentValue))
                                    {
                                        object enumValue = Enum.Parse(settingField.FieldType, currentValue);
                                        settingField.SetValue(Settings, enumValue);
                                    }
                                    else
                                    {
                                        if (settingField.FieldType.GetEnumUnderlyingType() == typeof(int))
                                        {
                                            int intValue;
                                            if (int.TryParse(currentValue, out intValue))
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
                                    object newList = Activator.CreateInstance(settingField.FieldType);
                                    settingField.SetValue(Settings, newList);
                                    Type listType = settingField.FieldType.GetGenericArguments()[0];
                                    if (!fromString.ContainsKey(listType))
                                    {
                                        continue;
                                    }
                                    MethodInfo addMethodInfo = newList.GetType().GetMethod("Add");
                                    foreach (string splitValue in SplitArrayValues(currentValue))
                                    {
                                        object insertObject = fromString[listType](splitValue);
                                        if (insertObject != null)
                                        {
                                            addMethodInfo.Invoke(newList, new object[] { insertObject });
                                        }
                                    }
                                    continue;
                                }
                                //Array
                                if (settingField.FieldType.IsArray)
                                {
                                    Type elementType = settingField.FieldType.GetElementType();
                                    if (!fromString.ContainsKey(elementType))
                                    {
                                        object emptyArray = Activator.CreateInstance(settingField.FieldType, new object[] { 0 });
                                        settingField.SetValue(Settings, emptyArray);
                                        continue;
                                    }
                                    Type genericListType = typeof(List<>).MakeGenericType(new Type[] { elementType });
                                    object newList = Activator.CreateInstance(genericListType);
                                    MethodInfo addMethodInfo = genericListType.GetMethod("Add");
                                    foreach (string splitValue in SplitArrayValues(currentValue))
                                    {
                                        object insertObject = fromString[elementType](splitValue);
                                        if (insertObject != null)
                                        {
                                            addMethodInfo.Invoke(newList, new object[] { insertObject });
                                        }
                                    }
                                    MethodInfo toArrayMethodInfo = genericListType.GetMethod("ToArray");
                                    object newArray = toArrayMethodInfo.Invoke(newList, new object[0]);
                                    settingField.SetValue(Settings, newArray);
                                    continue;
                                }
                                //Field
                                if (fromString.ContainsKey(settingField.FieldType))
                                {
                                    object parseValue = fromString[settingField.FieldType](currentValue);
                                    if (parseValue != null)
                                    {
                                        settingField.SetValue(Settings, parseValue);
                                    }
                                }
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
            FieldInfo[] settingFields = typeof(T).GetFields();
            if (File.Exists(filePath + ".tmp"))
            {
                File.Delete(filePath + ".tmp");
            }
            using (FileStream fs = new FileStream(filePath + ".tmp", FileMode.CreateNew))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine("# Lines starting with hashtags are ignored by the reader");
                    sw.WriteLine("# Setting file format: (key)=(value)");
                    sw.WriteLine("#");
                    sw.WriteLine("# Invalid values will be reset to default");
                    sw.WriteLine("#");
                    sw.WriteLine("");
                    foreach (FieldInfo settingField in settingFields)
                    {
                        object descriptionAttribute = settingField.GetCustomAttributes(typeof(DescriptionAttribute), true).FirstOrDefault();
                        string settingDescription = descriptionAttribute != null ? ((DescriptionAttribute)descriptionAttribute).Description.Replace("\n", String.Format("{0}# ", Environment.NewLine)) : string.Empty;

                        if (!string.IsNullOrEmpty(settingDescription))
                        {
                            sw.WriteLine(string.Format("# {0} - {1}", settingField.Name, settingDescription));
                        }
                        if (settingField.FieldType == typeof(string))
                        {
                            sw.WriteLine(string.Format("{0}={1}", settingField.Name, settingField.GetValue(Settings)));
                        }
                        if (settingField.FieldType == typeof(int) || settingField.FieldType == typeof(uint) || settingField.FieldType == typeof(short) || settingField.FieldType == typeof(long) || settingField.FieldType == typeof(ushort) || settingField.FieldType == typeof(ulong) || settingField.FieldType == typeof(float) || settingField.FieldType == typeof(double) || settingField.FieldType == typeof(decimal) || settingField.FieldType == typeof(bool))
                        {
                            sw.WriteLine(string.Format("{0}={1}", settingField.Name, settingField.GetValue(Settings).ToString()));
                        }
                        if (settingField.FieldType.IsEnum)
                        {
                            sw.WriteLine("#");
                            sw.WriteLine("# Valid values are:");
                            foreach (object enumValue in settingField.FieldType.GetEnumValues())
                            {
                                sw.WriteLine(string.Format("#   {0}", enumValue.ToString()));
                            }
                            sw.WriteLine(string.Format("{0}={1}", settingField.Name, settingField.GetValue(Settings)));
                        }
                        if (settingField.FieldType.IsGenericType && settingField.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                        {
                            //Get list
                            object settingsList = settingField.GetValue(Settings);
                            //Get enumerator
                            MethodInfo iEnumeratorInfo = settingField.FieldType.GetMethod("GetEnumerator");
                            object listEnumerator = iEnumeratorInfo.Invoke(settingsList, new object[0]);
                            //Get enumerator methods
                            MethodInfo moveNextInfo = listEnumerator.GetType().GetMethod("MoveNext");
                            MethodInfo currentInfo = listEnumerator.GetType().GetProperty("Current").GetGetMethod();
                            MethodInfo disposeInfo = listEnumerator.GetType().GetMethod("Dispose");
                            List<string> escapedList = new List<string>();
                            //Foreach object in list...
                            while ((bool)moveNextInfo.Invoke(listEnumerator, null))
                            {
                                object current = currentInfo.Invoke(listEnumerator, null);
                                escapedList.Add(EscapeString(current.ToString()));
                            }
                            disposeInfo.Invoke(listEnumerator, null);
                            sw.WriteLine(string.Format("{0}={1}", settingField.Name, string.Join(",", escapedList)));
                        }
                        if (settingField.FieldType.IsArray)
                        {
                            MethodInfo IEnumeratorInfo = settingField.FieldType.GetMethod("GetEnumerator");
                            object settingsList = settingField.GetValue(Settings);
                            IEnumerator objectEnum = (IEnumerator)IEnumeratorInfo.Invoke(settingsList, new object[0]);
                            List<string> escapedList = new List<string>();
                            while (objectEnum.MoveNext())
                            {
                                escapedList.Add(EscapeString(objectEnum.Current.ToString()));
                            }
                            sw.WriteLine(string.Format("{0}={1}", settingField.Name, string.Join(",", escapedList)));
                        }
                        sw.WriteLine();
                    }
                }
            }
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            File.Move(filePath + ".tmp", filePath);
        }
        #endregion


        public static IEnumerable<string> SplitArrayValues(string inputString)
        {
            List<string> retList = new List<string>();
            bool isEscaped = false;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < inputString.Length; i++)
            {
                char currentChar = inputString[i];
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
                    if (currentChar == '\\')
                    {
                        isEscaped = true;
                    }
                    else
                    {
                        if (currentChar == ',')
                        {
                            retList.Add(sb.ToString());
                            sb.Clear();
                        }
                        else
                        {
                            sb.Append(currentChar);
                        }
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
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < inputString.Length; i++)
            {
                char currentChar = inputString[i];
                if (currentChar == '\\')
                {
                    sb.Append(@"\\");
                    continue;
                }
                if (currentChar == ',')
                {
                    sb.Append(@"\,");
                    continue;
                }
                if (currentChar == '\r')
                {
                    continue;
                }
                if (currentChar == '\n')
                {
                    sb.Append(@"\n");
                    continue;
                }
                sb.Append(currentChar);
            }
            return sb.ToString();
        }
    }
}