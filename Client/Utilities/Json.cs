using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LunaClient.Utilities
{
    /*
     * Copyright (c) 2013 Calvin Rien
     *
     * Based on the JSON parser by Patrick van Bergen
     * http://techblog.procurios.nl/k/618/news/view/14605/14863/How-do-I-write-my-own-parser-for-JSON.html
     *
     * Simplified it so that it doesn't throw exceptions
     * and can be used in Unity iPhone with maximum code stripping.
     *
     * Permission is hereby granted, free of charge, to any person obtaining
     * a copy of this software and associated documentation files (the
     * "Software"), to deal in the Software without restriction, including
     * without limitation the rights to use, copy, modify, merge, publish,
     * distribute, sublicense, and/or sell copies of the Software, and to
     * permit persons to whom the Software is furnished to do so, subject to
     * the following conditions:
     *
     * The above copyright notice and this permission notice shall be
     * included in all copies or substantial portions of the Software.
     *
     * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
     * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
     * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
     * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
     * CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
     * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
     * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
     */

    /// <summary>
    ///     This class encodes and decodes JSON strings.
    ///     Spec. details, see http://www.json.org/
    ///     JSON uses Arrays and Objects. These correspond here to the datatypes IList and IDictionary.
    ///     All numbers are parsed to doubles.
    /// </summary>
    public static class Json
    {
        #region Methods: public

        /// <summary>
        ///     Parses the string json into a value
        /// </summary>
        /// <param name="json">A JSON string.</param>
        /// <returns>An List&lt;object&gt;, a Dictionary&lt;string, object&gt;, a double, an integer,a string, null, true, or false</returns>
        public static object Deserialize(string json)
        {
            try
            {
                // save the string for debug information
                return string.IsNullOrEmpty(json) ? null : Parser.Parse(json);
            }
            catch (Exception ex)
            {
                LunaLog.LogError(ex.ToString());
                return null;
            }
        }

        /// <summary>
        ///     Converts a IDictionary / IList object or a simple type (string, int, etc.) into a JSON string
        /// </summary>
        /// <param name="json">A Dictionary&lt;string, object&gt; / List&lt;object&gt;</param>
        /// <returns>A JSON encoded string, or null if object 'json' is not serializable</returns>
        public static string Serialize(object obj)
        {
            return Serializer.Serialize(obj);
        }

        #endregion

        #region Nested Type: Parser

        private sealed class Parser : IDisposable
        {
            #region Constants

            private const string WordBreak = "{}[],:\"";

            #endregion

            #region Fields

            private StringReader _json;

            #endregion

            #region Constructors

            private Parser(string jsonString) => _json = new StringReader(jsonString);

            #endregion

            #region Enums

            private enum Token
            {
                None,
                CurlyOpen,
                CurlyClose,
                SquaredOpen,
                SquaredClose,
                Colon,
                Comma,
                String,
                Number,
                True,
                False,
                Null
            };

            #endregion

            #region Properties

            private char NextChar => Convert.ToChar(_json.Read());

            private Token NextToken
            {
                get
                {
                    EatWhitespace();

                    if (_json.Peek() == -1)
                    {
                        return Token.None;
                    }

                    switch (PeekChar)
                    {
                        case '{':
                            return Token.CurlyOpen;
                        case '}':
                            _json.Read();
                            return Token.CurlyClose;
                        case '[':
                            return Token.SquaredOpen;
                        case ']':
                            _json.Read();
                            return Token.SquaredClose;
                        case ',':
                            _json.Read();
                            return Token.Comma;
                        case '"':
                            return Token.String;
                        case ':':
                            return Token.Colon;
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                        case '-':
                            return Token.Number;
                    }

                    switch (NextWord.ToLower())
                    {
                        case "false":
                            return Token.False;
                        case "true":
                            return Token.True;
                        case "null":
                            return Token.Null;
                    }

                    return Token.None;
                }
            }

            private string NextWord
            {
                get
                {
                    var word = new StringBuilder();

                    while (!IsWordBreak(PeekChar))
                    {
                        word.Append(NextChar);

                        if (_json.Peek() == -1)
                        {
                            break;
                        }
                    }

                    return word.ToString();
                }
            }

            private char PeekChar => Convert.ToChar(_json.Peek());

            #endregion

            #region Methods: public

            public static bool IsWordBreak(char c)
            {
                return char.IsWhiteSpace(c) || WordBreak.IndexOf(c) != -1;
            }

            public static object Parse(string jsonString)
            {
                using (var instance = new Parser(jsonString))
                {
                    return instance.ParseValue();
                }
            }

            public void Dispose()
            {
                _json.Dispose();
                _json = null;
            }

            #endregion

            #region Methods: private

            private void EatWhitespace()
            {
                while (char.IsWhiteSpace(PeekChar))
                {
                    _json.Read();

                    if (_json.Peek() == -1)
                    {
                        break;
                    }
                }
            }

            private List<object> ParseArray()
            {
                var array = new List<object>();

                // ditch opening bracket
                _json.Read();

                // [
                var parsing = true;
                while (parsing)
                {
                    var nextToken = NextToken;

                    switch (nextToken)
                    {
                        case Token.None:
                            return null;
                        case Token.Comma:
                            continue;
                        case Token.SquaredClose:
                            parsing = false;
                            break;
                        default:
                            var value = ParseByToken(nextToken);

                            array.Add(value);
                            break;
                    }
                }

                return array;
            }

            private object ParseByToken(Token token)
            {
                switch (token)
                {
                    case Token.String:
                        return ParseString();
                    case Token.Number:
                        return ParseNumber();
                    case Token.CurlyOpen:
                        return ParseObject();
                    case Token.SquaredOpen:
                        return ParseArray();
                    case Token.True:
                        return true;
                    case Token.False:
                        return false;
                    case Token.Null:
                        return null;
                    default:
                        return null;
                }
            }

            private object ParseNumber()
            {
                var number = NextWord;

                if (number.IndexOf('.') == -1)
                {
                    long.TryParse(number, out var parsedInt);
                    return parsedInt;
                }

                double.TryParse(number, out var parsedDouble);
                return parsedDouble;
            }

            private Dictionary<string, object> ParseObject()
            {
                var table = new Dictionary<string, object>();

                // ditch opening brace
                _json.Read();

                // {
                while (true)
                {
                    switch (NextToken)
                    {
                        case Token.None:
                            return null;
                        case Token.Comma:
                            continue;
                        case Token.CurlyClose:
                            return table;
                        default:
                            // name
                            var name = ParseString();
                            if (name == null)
                            {
                                return null;
                            }

                            // :
                            if (NextToken != Token.Colon)
                            {
                                return null;
                            }
                            // ditch the colon
                            _json.Read();

                            // value
                            table[name] = ParseValue();
                            break;
                    }
                }
            }

            private string ParseString()
            {
                var s = new StringBuilder();

                // ditch opening quote
                _json.Read();

                var parsing = true;
                while (parsing)
                {
                    if (_json.Peek() == -1)
                    {
                        break;
                    }

                    var c = NextChar;
                    switch (c)
                    {
                        case '"':
                            parsing = false;
                            break;
                        case '\\':
                            if (_json.Peek() == -1)
                            {
                                parsing = false;
                                break;
                            }

                            c = NextChar;
                            switch (c)
                            {
                                case '"':
                                case '\\':
                                case '/':
                                    s.Append(c);
                                    break;
                                case 'b':
                                    s.Append('\b');
                                    break;
                                case 'f':
                                    s.Append('\f');
                                    break;
                                case 'n':
                                    s.Append('\n');
                                    break;
                                case 'r':
                                    s.Append('\r');
                                    break;
                                case 't':
                                    s.Append('\t');
                                    break;
                                case 'u':
                                    var hex = new char[4];

                                    for (var i = 0; i < 4; i++)
                                    {
                                        hex[i] = NextChar;
                                    }

                                    s.Append((char)Convert.ToInt32(new string(hex), 16));
                                    break;
                            }
                            break;
                        default:
                            s.Append(c);
                            break;
                    }
                }

                return s.ToString();
            }

            private object ParseValue()
            {
                var nextToken = NextToken;
                return ParseByToken(nextToken);
            }

            #endregion
        }

        #endregion

        #region Nested Type: Serializer

        private sealed class Serializer
        {
            #region Fields

            private readonly StringBuilder _builder;

            #endregion

            #region Constructors

            private Serializer() => _builder = new StringBuilder();

            #endregion

            #region Methods: public

            public static string Serialize(object obj)
            {
                var instance = new Serializer();

                instance.SerializeValue(obj);

                return instance._builder.ToString();
            }

            #endregion

            #region Methods: private

            private void SerializeArray(IList anArray)
            {
                _builder.Append('[');

                var first = true;

                foreach (var obj in anArray)
                {
                    if (!first)
                    {
                        _builder.Append(',');
                    }

                    SerializeValue(obj);

                    first = false;
                }

                _builder.Append(']');
            }

            private void SerializeObject(IDictionary obj)
            {
                var first = true;

                _builder.Append('{');

                foreach (var e in obj.Keys)
                {
                    if (!first)
                    {
                        _builder.Append(',');
                    }

                    SerializeString(e.ToString());
                    _builder.Append(':');

                    SerializeValue(obj[e]);

                    first = false;
                }

                _builder.Append('}');
            }

            private void SerializeOther(object value)
            {
                // NOTE: decimals lose precision during serialization.
                // They always have, I'm just letting you know.
                // Previously floats and doubles lost precision too.
                if (value is float)
                {
                    _builder.Append(((float)value).ToString("R"));
                }
                else if (value is int
                         || value is uint
                         || value is long
                         || value is sbyte
                         || value is byte
                         || value is short
                         || value is ushort
                         || value is ulong)
                {
                    _builder.Append(value);
                }
                else if (value is double
                         || value is decimal)
                {
                    _builder.Append(Convert.ToDouble(value).ToString("R"));
                }
                else
                {
                    SerializeString(value.ToString());
                }
            }

            private void SerializeString(string str)
            {
                _builder.Append('\"');

                var charArray = str.ToCharArray();
                foreach (var c in charArray)
                {
                    switch (c)
                    {
                        case '"':
                            _builder.Append("\\\"");
                            break;
                        case '\\':
                            _builder.Append("\\\\");
                            break;
                        case '\b':
                            _builder.Append("\\b");
                            break;
                        case '\f':
                            _builder.Append("\\f");
                            break;
                        case '\n':
                            _builder.Append("\\n");
                            break;
                        case '\r':
                            _builder.Append("\\r");
                            break;
                        case '\t':
                            _builder.Append("\\t");
                            break;
                        default:
                            var codepoint = Convert.ToInt32(c);
                            if (codepoint >= 32 && codepoint <= 126)
                            {
                                _builder.Append(c);
                            }
                            else
                            {
                                _builder.Append("\\u");
                                _builder.Append(codepoint.ToString("x4"));
                            }
                            break;
                    }
                }

                _builder.Append('\"');
            }

            private void SerializeValue(object value)
            {
                IList asList;
                IDictionary asDict;
                string asStr;

                if (value == null)
                {
                    _builder.Append("null");
                }
                else if ((asStr = value as string) != null)
                {
                    SerializeString(asStr);
                }
                else if (value is bool)
                {
                    _builder.Append((bool)value ? "true" : "false");
                }
                else if ((asList = value as IList) != null)
                {
                    SerializeArray(asList);
                }
                else if ((asDict = value as IDictionary) != null)
                {
                    SerializeObject(asDict);
                }
                else if (value is char)
                {
                    SerializeString(new string((char)value, 1));
                }
                else
                {
                    SerializeOther(value);
                }
            }

            #endregion
        }

        #endregion
    }
}
