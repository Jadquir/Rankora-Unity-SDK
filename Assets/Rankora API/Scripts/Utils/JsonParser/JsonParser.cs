using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Rankora_API.Scripts.Utils.Json
{

    /// <summary>
    /// Interface for custom JSON converters
    /// </summary>
    public interface IJsonConverter
    {
        bool CanConvert(Type objectType);
        object ReadJson(object value, Type objectType, IJsonSerializer serializer);
        void WriteJson(object value, Type objectType, StringBuilder builder, IJsonSerializer serializer);
    }

    /// <summary>
    /// Interface for JSON serializer that can be used by converters
    /// </summary>
    public interface IJsonSerializer
    {
        object Deserialize(string json);
        string Serialize(object obj);
        void SerializeValue(object value, StringBuilder builder);
    }

    /// <summary>
    /// Settings for JSON serialization/deserialization
    /// </summary>
    public class JsonSettings
    {
        public List<IJsonConverter> Converters { get; set; } = new List<IJsonConverter>();

        public static JsonSettings Default = new JsonSettings();

        internal JsonSettings GetAddedConverters()
        {
            Converters.Add(new DateTimeConverter());
            Converters.Add(new DateTimeOffsetConverter());
            Converters.Add(new TimeSpanConverter());
            return this;
        }
    }


    /// <summary>
    /// This class encodes and decodes JSON strings.
    /// Spec. details, see http://www.json.org/
    ///
    /// JSON uses Arrays and Objects. These correspond here to the datatypes IList and IDictionary.
    /// All numbers are parsed to doubles.
    /// </summary>
    public static class Json
    {
        /// <summary>
        /// Parses the string json into a value
        /// </summary>
        /// <param name="json">A JSON string.</param>
        /// <returns>An List&lt;object&gt;, a Dictionary&lt;string, object&gt;, a double, an integer,a string, null, true, or false</returns>
        public static object Deserialize(string json)
        {
            return Deserialize(json, JsonSettings.Default);
        }

        /// <summary>
        /// Parses the string json into a value with custom settings
        /// </summary>
        /// <param name="json">A JSON string.</param>
        /// <param name="settings">JSON settings with custom converters.</param>
        /// <returns>An List&lt;object&gt;, a Dictionary&lt;string, object&gt;, a double, an integer,a string, null, true, or false</returns>
        public static object Deserialize(string json, JsonSettings settings)
        {
            // save the string for debug information
            if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return Parser.Parse(json, settings.GetAddedConverters());
        }

        /// <summary>
        /// Converts a IDictionary / IList object or a simple type (string, int, etc.) into a JSON string
        /// </summary>
        /// <param name="obj">A Dictionary&lt;string, object&gt; / List&lt;object&gt;</param>
        /// <returns>A JSON encoded string, or null if object 'obj' is not serializable</returns>
        public static string Serialize(object obj)
        {
            return Serialize(obj, JsonSettings.Default);
        }

        /// <summary>
        /// Converts a IDictionary / IList object or a simple type (string, int, etc.) into a JSON string with custom settings
        /// </summary>
        /// <param name="obj">A Dictionary&lt;string, object&gt; / List&lt;object&gt;</param>
        /// <param name="settings">JSON settings with custom converters.</param>
        /// <returns>A JSON encoded string, or null if object 'obj' is not serializable</returns>
        public static string Serialize(object obj, JsonSettings settings)
        {
            return Serializer.Serialize(obj, settings.GetAddedConverters());
        }

        sealed class Parser : IDisposable
        {
            const string WORD_BREAK = "{}[],:\"";

            public static bool IsWordBreak(char c)
            {
                return Char.IsWhiteSpace(c) || WORD_BREAK.IndexOf(c) != -1;
            }

            enum TOKEN
            {
                NONE,
                CURLY_OPEN,
                CURLY_CLOSE,
                SQUARED_OPEN,
                SQUARED_CLOSE,
                COLON,
                COMMA,
                STRING,
                NUMBER,
                TRUE,
                FALSE,
                NULL
            };

            StringReader json;
            JsonSettings settings;
            JsonSerializerImpl serializer;

            Parser(string jsonString, JsonSettings settings)
            {
                json = new StringReader(jsonString);
                this.settings = settings;
                this.serializer = new JsonSerializerImpl(settings);
            }

            public static object Parse(string jsonString, JsonSettings settings)
            {
                using (var instance = new Parser(jsonString, settings))
                {
                    return instance.ParseValue();
                }
            }

            public void Dispose()
            {
                json.Dispose();
                json = null;
            }

            object ParseObject()
            {
                var table = new Dictionary<string, object>(); // Change 'object' to 'Dictionary<string, object>'
              
                // ditch opening brace
                json.Read();

                // {
                while (true)
                {
                    switch (NextToken)
                    {
                        case TOKEN.NONE:
                            return null;
                        case TOKEN.COMMA:
                            continue;
                        case TOKEN.CURLY_CLOSE:
                            return table;
                        default:
                            // name
                            string name = ParseString();
                            if (name == null)
                            {
                                return null;
                            }

                            // :
                            if (NextToken != TOKEN.COLON)
                            {
                                return null;
                            }
                            // ditch the colon
                            json.Read();

                            // value
                            table[name] = ParseValue(); // This will now work as 'table' is a Dictionary<string, object>
                            break;
                    }
                }
            }
            public static ExpandoObject ToExpando(Dictionary<string, object> dict)
            {
                var expando = new ExpandoObject();
                var expandoDict = (IDictionary<string, object>)expando;

                foreach (var kvp in dict)
                {
                    expandoDict[kvp.Key] = kvp.Value;
                }

                return expando;
            }
            List<object> ParseArray()
            {
                List<object> array = new List<object>();

                // ditch opening bracket
                json.Read();

                // [
                var parsing = true;
                while (parsing)
                {
                    TOKEN nextToken = NextToken;

                    switch (nextToken)
                    {
                        case TOKEN.NONE:
                            return null;
                        case TOKEN.COMMA:
                            continue;
                        case TOKEN.SQUARED_CLOSE:
                            parsing = false;
                            break;
                        default:
                            object value = ParseByToken(nextToken);

                            array.Add(value);
                            break;
                    }
                }

                return array;
            }

            object ParseValue()
            {
                TOKEN nextToken = NextToken;
                return ParseByToken(nextToken);
            }

            object ParseByToken(TOKEN token)
            {
                switch (token)
                {
                    case TOKEN.STRING:
                        return ParseString();
                    case TOKEN.NUMBER:
                        return ParseNumber();
                    case TOKEN.CURLY_OPEN:
                        return ParseObject();
                    case TOKEN.SQUARED_OPEN:
                        return ParseArray();
                    case TOKEN.TRUE:
                        return true;
                    case TOKEN.FALSE:
                        return false;
                    case TOKEN.NULL:
                        return null;
                    default:
                        return null;
                }
            }

            string ParseString()
            {
                StringBuilder s = new StringBuilder();
                char c;

                // ditch opening quote
                json.Read();

                bool parsing = true;
                while (parsing)
                {

                    if (json.Peek() == -1)
                    {
                        parsing = false;
                        break;
                    }

                    c = NextChar;
                    switch (c)
                    {
                        case '"':
                            parsing = false;
                            break;
                        case '\\':
                            if (json.Peek() == -1)
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

                                    for (int i = 0; i < 4; i++)
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

            object ParseNumber()
            {
                string number = NextWord;

                if (number.IndexOf('.') == -1)
                {
                    long parsedInt;
                    Int64.TryParse(number, out parsedInt);
                    return parsedInt;
                }

                double parsedDouble;
                Double.TryParse(number, out parsedDouble);
                return parsedDouble;
            }

            void EatWhitespace()
            {
                while (Char.IsWhiteSpace(PeekChar))
                {
                    json.Read();

                    if (json.Peek() == -1)
                    {
                        break;
                    }
                }
            }

            char PeekChar
            {
                get
                {
                    return Convert.ToChar(json.Peek());
                }
            }

            char NextChar
            {
                get
                {
                    return Convert.ToChar(json.Read());
                }
            }

            string NextWord
            {
                get
                {
                    StringBuilder word = new StringBuilder();

                    while (!IsWordBreak(PeekChar))
                    {
                        word.Append(NextChar);

                        if (json.Peek() == -1)
                        {
                            break;
                        }
                    }

                    return word.ToString();
                }
            }

            TOKEN NextToken
            {
                get
                {
                    EatWhitespace();

                    if (json.Peek() == -1)
                    {
                        return TOKEN.NONE;
                    }

                    switch (PeekChar)
                    {
                        case '{':
                            return TOKEN.CURLY_OPEN;
                        case '}':
                            json.Read();
                            return TOKEN.CURLY_CLOSE;
                        case '[':
                            return TOKEN.SQUARED_OPEN;
                        case ']':
                            json.Read();
                            return TOKEN.SQUARED_CLOSE;
                        case ',':
                            json.Read();
                            return TOKEN.COMMA;
                        case '"':
                            return TOKEN.STRING;
                        case ':':
                            return TOKEN.COLON;
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
                            return TOKEN.NUMBER;
                    }

                    switch (NextWord)
                    {
                        case "false":
                            return TOKEN.FALSE;
                        case "true":
                            return TOKEN.TRUE;
                        case "null":
                            return TOKEN.NULL;
                    }

                    return TOKEN.NONE;
                }
            }
        }

        /// <summary>
        /// Implementation of IJsonSerializer for use by converters
        /// </summary>
        sealed class JsonSerializerImpl : IJsonSerializer
        {
            private JsonSettings settings;

            public JsonSerializerImpl(JsonSettings settings)
            {
                this.settings = settings;
            }

            public object Deserialize(string json)
            {
                return Json.Deserialize(json, settings);
            }

            public string Serialize(object obj)
            {
                return Json.Serialize(obj, settings);
            }

            public void SerializeValue(object value, StringBuilder builder)
            {
                var serializer = new Serializer(settings);
                serializer.SerializeValuePublic(value, builder);
            }
        }

        sealed class Serializer
        {
            private StringBuilder builder;
            private JsonSettings settings;
            private Json.JsonSerializerImpl serializer;

            // NEW: Track visited objects to prevent stack overflow
            private HashSet<object> visitedObjects = new HashSet<object>(new ReferenceEqualityComparer());

            internal Serializer(JsonSettings settings)
            {
                builder = new StringBuilder();
                this.settings = settings;
                this.serializer = new Json.JsonSerializerImpl(settings);
            }

            public static string Serialize(object obj, JsonSettings settings)
            {
                var instance = new Serializer(settings);
                instance.SerializeValue(obj);
                return instance.builder.ToString();
            }

            public void SerializeValuePublic(object value, StringBuilder targetBuilder)
            {
                var originalBuilder = builder;
                builder = targetBuilder;
                SerializeValue(value);
                builder = originalBuilder;
            }

            private void SerializeValue(object value)
            {
                if (value == null)
                {
                    builder.Append("null");
                    return;
                }

                Type valueType = value.GetType();

                foreach (var converter in settings.Converters)
                {
                    if (converter.CanConvert(valueType))
                    {
                        converter.WriteJson(value, valueType, builder, serializer);
                        return;
                    }
                }

                switch (value)
                {
                    case string str:
                        SerializeString(str);
                        break;
                    case bool b:
                        builder.Append(b ? "true" : "false");
                        break;
                    case char ch:
                        SerializeString(new string(ch, 1));
                        break;
                    case IList list:
                        SerializeArray(list);
                        break;
                    case IDictionary dict:
                        SerializeObject(dict);
                        break;
                    default:
                        if (!valueType.IsPrimitive && !valueType.IsEnum)
                        {
                            if (visitedObjects.Contains(value))
                            {
                                builder.Append("\"[Circular Reference]\"");
                                return;
                            }

                            visitedObjects.Add(value);

                            var dict = ConvertToDictionary(value);
                            SerializeObject((IDictionary)dict);

                            visitedObjects.Remove(value);
                        }
                        else
                        {
                            SerializeOther(value);
                        }
                        break;
                }
            }

            private void SerializeObject(IDictionary obj)
            {
                builder.Append('{');
                bool first = true;

                foreach (object key in obj.Keys)
                {
                    if (!first) builder.Append(',');
                    SerializeString(key.ToString());
                    builder.Append(':');
                    SerializeValue(obj[key]);
                    first = false;
                }

                builder.Append('}');
            }

            private void SerializeArray(IList array)
            {
                builder.Append('[');
                bool first = true;

                foreach (var item in array)
                {
                    if (!first) builder.Append(',');
                    SerializeValue(item);
                    first = false;
                }

                builder.Append(']');
            }

            private void SerializeString(string str)
            {
                builder.Append('\"');

                foreach (char c in str)
                {
                    switch (c)
                    {
                        case '"': builder.Append("\\\""); break;
                        case '\\': builder.Append("\\\\"); break;
                        case '\b': builder.Append("\\b"); break;
                        case '\f': builder.Append("\\f"); break;
                        case '\n': builder.Append("\\n"); break;
                        case '\r': builder.Append("\\r"); break;
                        case '\t': builder.Append("\\t"); break;
                        default:
                            int codepoint = Convert.ToInt32(c);
                            if (codepoint >= 32 && codepoint <= 126)
                                builder.Append(c);
                            else
                                builder.AppendFormat("\\u{0:x4}", codepoint);
                            break;
                    }
                }

                builder.Append('\"');
            }

            private void SerializeOther(object value)
            {
                switch (value)
                {
                    case float f:
                        builder.Append(f.ToString("R"));
                        break;
                    case double d:
                        builder.Append(d.ToString("R"));
                        break;
                    case decimal m:
                        builder.Append(Convert.ToDouble(m).ToString("R"));
                        break;
                    default:
                        builder.Append(Convert.ToString(value));
                        break;
                }
            }

            private IDictionary<string, object> ConvertToDictionary(object obj)
            {
                var dict = new Dictionary<string, object>();
                var type = obj.GetType();

                foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (prop.CanRead && prop.GetIndexParameters().Length == 0)
                    {
                        try
                        {
                            dict[prop.Name] = prop.GetValue(obj);
                        }
                        catch
                        {
                            dict[prop.Name] = null;
                        }
                    }
                }

                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    try
                    {
                        dict[field.Name] = field.GetValue(obj);
                    }
                    catch
                    {
                        dict[field.Name] = null;
                    }
                }

                return dict;
            }

            // 👇 Needed for reference-based equality in HashSet<object>
            class ReferenceEqualityComparer : IEqualityComparer<object>
            {
                public new bool Equals(object x, object y) => ReferenceEquals(x, y);
                public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
            }
        }

    }
}