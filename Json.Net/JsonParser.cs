using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Dynamic;
using UnityEngine;

namespace Json.Net
{
    /// <summary>
    /// JSON Parser Class
    /// </summary>
    public class JsonParser : ParserBase
    {
        IJsonConverter[] Converters;
        IPropertyNameTransform PropertyNameTransform;

        static Dictionary<char, char> EscapeMap = 
            new Dictionary<char, char>()
            {
                { 'b', (char)8 },
                { 't', (char)9 },
                { 'n', (char)10 },
                { 'f', (char)12 },
                { 'r', (char)13 }
            };


        public JsonParser()
        {
        }


        [ThreadStatic]
        static JsonParser _Instance;

        public static JsonParser Instance
        {
            get
            {
                return _Instance ??
                      (_Instance = new JsonParser());
            }
        }


        public JsonParser Initialize(string json, SerializationOptions options)
        {
            base.Initialize(json);
            Converters = options?.Converters;
            PropertyNameTransform = options?.PropertyNameTransform;
            return this;
        }


        public JsonParser Initialize(TextReader jsonReader, SerializationOptions options)
        {
            base.Initialize(jsonReader);
            Converters = options?.Converters;
            PropertyNameTransform = options.PropertyNameTransform;
            return this;
        }
        

        StringBuilder text = new StringBuilder();

        public object FromJson(Type type)
        {
            object result;

            SkipWhite();
            /*
            if (type.ToString().IndexOf("UnityEngine.Vector")==0)
            {
                Debug.Log(type);
                ReadNext();
                Debug.Log(NextChar);

                string _full = "";
                while (NextChar != '}') {
                    _full = _full + NextChar;
                    ReadNext();
                }
                Debug.Log(_full);
                object result2 = new Vector3(42,42,42) ;
                return result2;
                

            }
            */

            if (NextChar == '{')
            {
                if (type == null || type == typeof(object) || type == typeof(ExpandoObject))
                    type = typeof(ExpandoObject);
                
                ReadNext();
                SkipWhite();

                if (type.IsValueType)
                {
                    if (type.ToString().IndexOf("UnityEngine.Vector") != 0)
                    {
                        throw new FormatException("Unexpected type!");
                    }
                }

                result = Activator.CreateInstance(type);

                var nameType = result is IDictionary ?
                    type.GenericTypeArguments[0] :
                    typeof(string);

                var valueType = result is IDictionary ?
                    type.GenericTypeArguments[1] :
                    null;

                var mIndex = 0;

                 

                while (NextChar!='}')
                {
                    var name = FromJson(nameType);

                   

                    SkipWhite();
                    Match(":");

                    var map = SerializerMap.GetSerializerMap(type);

                    MemberAccessor field = null; 

                    
                   

                    if (valueType == null)
                    {
                        if (map.Members.Length == 0)
                            field = GetFieldAccessorFor(name.ToString());
                        else
                            for (var i = mIndex; i < map.Members.Length; i++)
                            {
                                var memberName = map.Members[i].Name;
                                
                                if (PropertyNameTransform != null)
                                    memberName = PropertyNameTransform.Transform(memberName);

                                var tName = name.ToString();
                                
                                if (memberName == tName || memberName.Equals(tName, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    field = map.Members[i];
                                    
                                    
                                    break;
                                }
                            }
                    }
                  
                    var fieldType = field == null ? valueType : field.ValueType;
                   
                    if (fieldType == null)
                    {
                        _ = FromJson(fieldType);
                    }
                    else
                    {
                        var value = FromJson(fieldType);
                        

                        if (type.ToString().IndexOf("UnityEngine.Vector") == 0)
                        {
                            
                            if (type.ToString() == ("UnityEngine.Vector3") )
                            {
                                Vector3 rev = (Vector3)result;
                                if (name.ToString() == "x") rev.x = (float)Convert.ToDouble(value);
                                if (name.ToString() == "y") rev.y = (float)Convert.ToDouble(value);
                                if (name.ToString() == "z") rev.z = (float)Convert.ToDouble(value); 
                                result = (object)rev;
                            }

                            if (type.ToString() == ("UnityEngine.Vector3Int"))
                            {
                                Vector3Int rev = (Vector3Int)result;
                                if (name.ToString() == "x") rev.x = Convert.ToInt32(value);
                                if (name.ToString() == "y") rev.y = Convert.ToInt32(value);
                                if (name.ToString() == "z") rev.z = Convert.ToInt32(value);
                                result = (object)rev;
                            }

                            if (type.ToString() == ("UnityEngine.Vector2Int"))
                            {
                                Vector2Int rev = (Vector2Int)result;
                                if (name.ToString() == "x") rev.x = Convert.ToInt32(value);
                                if (name.ToString() == "y") rev.y = Convert.ToInt32(value); 
                                result = (object)rev;
                            }
                            if (type.ToString() == ("UnityEngine.Vector2"))
                            {
                                Vector2 rev = (Vector2)result;
                                if (name.ToString() == "x") rev.x = (float)Convert.ToDouble(value);
                                if (name.ToString() == "y") rev.y = (float)Convert.ToDouble(value);
                                result = (object)rev;
                            }
                        }
                        else
                        {

                            if (field != null)
                            {
                                try
                                {
                                    field.SetValue(result, value);
                                }
                                catch
                                {
                                    // TODO: Cannot set, ignore for now
                                }
                            }
                            else
                            {
                                ((IDictionary)result).Add(name, value);
                            }
                        }

                    }

                    SkipWhite();

                    if (NextChar == ',')
                    {
                        ReadNext();
                        SkipWhite();
                        continue;
                    }

                    break;
                }
                
                Match("}");

                return result;
            }

            if (NextChar == '[')
            {
                if (type == null || type == typeof(object) || type == typeof(ExpandoObject))
                    type = typeof(object[]);
                
                ReadNext();
                SkipWhite();

                var elementType =
                    type.IsArray ?
                        type.GetElementType() :
                    type.IsGenericType ?
                        type.GenericTypeArguments[0] :
                        typeof(object);

                IList list;

                if (type.IsArray)
                    list = new ArrayList();
                else if (type == typeof(IEnumerable))
                    list = new List<object>();
                else 
                    list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));

                while (NextChar != ']')
                {
                    var item = FromJson(elementType);

                    list.Add(item);

                    SkipWhite();

                    if (NextChar == ',')
                    {
                        ReadNext();
                        SkipWhite();
                        continue;
                    }

                    break;
                }

                Match("]");

                if (list is ArrayList)
                    return ((ArrayList)list).ToArray(elementType);
                else
                    return list;
            }

            if (NextChar == '"')
            {
                if (type == null || type == typeof(object) || type == typeof(ExpandoObject))
                    type = typeof(string);
                
                ReadNext();

                while (!EndOfStream && NextChar != '"')
                {
                    if (NextChar == '\\')
                    {
                        ReadNext();

                        switch (NextChar)
                        {
                            case 'b':
                            case 't':
                            case 'n':
                            case 'f':
                            case 'r':
                                text.Append(EscapeMap[NextChar]);
                                break;

                            case 'u':
                                ReadNext();

                                var unicode = "";

                                while (unicode.Length < 4 && IsHexDigit)
                                {
                                    unicode += NextChar;
                                    ReadNext();
                                }

                                text.Append(char.ConvertFromUtf32(int.Parse("0x" + unicode)));
                                continue;

                            default:
                                text.Append(NextChar);
                                break;
                        }
                    }
                    else
                        text.Append(NextChar);

                    ReadNext();
                }

                SkipWhite();
                Match("\"");

                result = text.ToString();

                text.Clear();

                var converter = Converters?.FirstOrDefault(c => c.GetConvertingType() == type);

                if (converter != null)
                    return converter.Deserializer((string)result);

                if (type == typeof(DateTime)
                 || type == typeof(DateTime?))
                    return DateTime.Parse((string)result, CultureInfo.InvariantCulture);

                if (type == typeof(DateTimeOffset)
                 || type == typeof(DateTimeOffset?))
                    return DateTimeOffset.Parse((string)result, CultureInfo.InvariantCulture);
                
                if (type == typeof(TimeSpan)
                 || type == typeof(TimeSpan?))
                    return TimeSpan.Parse((string)result, CultureInfo.InvariantCulture);

                if (type == typeof(Guid)
                 || type == typeof(Guid?))
                    return Guid.Parse((string)result);

                try
                {
                    return Convert.ChangeType(result, type);
                }
                catch
                {
                    throw new FormatException($"'{result}' cannot be converted to {type.Name}");
                }
            }
            else if (NextChar == 't')
            {
                Match("true");
                return true;
            }
            else if (NextChar == 'f')
            {
                Match("false");
                return false;
            }
            else if (NextChar == 'n')
            {
                Match("null");

                if (type == null)
                    type = typeof(object);

                if (!(type.IsClass
                   || type.IsInterface
                   || Nullable.GetUnderlyingType(type) != null))
                    throw new InvalidDataException("Type " + type.Name + "'s value cannot be null!");

                return null;
            }
            else if (NextChar == '-' || IsDigit)
            {
                if (type == null || type == typeof(object))
                    type = typeof(double);
                
                if (NextChar == '-')
                {
                    text.Append('-');
                    ReadNext();
                }

                if (NextChar == '0')
                {
                    text.Append('0');
                    ReadNext();
                }
                else if (IsDigit)
                {
                    do
                    {
                        text.Append(NextChar);
                        ReadNext();
                    }
                    while (IsDigit);
                }
                else
                    throw new FormatException("Digit expected!");

                if (NextChar == '.')
                {
                    text.Append('.');
                    ReadNext();

                    while (IsDigit)
                    {
                        text.Append(NextChar);
                        ReadNext();
                    }
                }

                if (NextChar == 'e' || NextChar == 'E')
                {
                    text.Append('e');
                    ReadNext();

                    if (NextChar == '+' || NextChar == '-')
                    {
                        text.Append(NextChar);
                        ReadNext();
                    }

                    while (IsDigit)
                    {
                        text.Append(NextChar);
                        ReadNext();
                    }
                }

                var t = text.ToString();
                text.Clear();

                var inv = CultureInfo.InvariantCulture;

                if (type.IsGenericType
                    && type.IsValueType
                    && Nullable.GetUnderlyingType(type).IsEnum)
                {
                    type = Nullable.GetUnderlyingType(type);
                }

                if (type.IsEnum)
                    return Enum.Parse(type, t);
                
                if (type == typeof(int)
                 || type == typeof(int?))
                    return int.Parse(t, inv);

                if (type == typeof(long)
                 || type == typeof(long?))
                    return long.Parse(t, inv);

                return double.Parse(t, inv);
            }

            throw new FormatException("Unexpected character! " + NextChar);
        }

        private MemberAccessor GetFieldAccessorFor(string fieldName)
        {
            var tName = fieldName.ToString();

            if (PropertyNameTransform != null)
                tName = PropertyNameTransform.Transform(tName);

            return new MemberAccessor
            {
                Name = tName,
                ValueType = typeof(ExpandoObject),
                GetValue = o => (((ExpandoObject)o) as IDictionary<string, object>)[tName],
                SetValue = (o, v) => (((ExpandoObject)o) as IDictionary<string, object>)[tName] = v,
            };
        }
    }
}
