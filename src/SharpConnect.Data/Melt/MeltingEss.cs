//MIT, 2018, EngineKit
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;


namespace SharpConnect.Data.Internal
{

    public static class GlobalRegisteredTypes
    {
        static Dictionary<Type, MyTypeInfo> _registeredDic = new Dictionary<Type, MyTypeInfo>();
        public static void Register(MyTypeInfo myTypeInfo)
        {
            if (!_registeredDic.ContainsKey(myTypeInfo._type))
            {
                _registeredDic.Add(myTypeInfo._type, myTypeInfo);
            }
        }
        public static bool TryGetMyTypeInfo(Type orgType, out MyTypeInfo found)
        {
            return _registeredDic.TryGetValue(orgType, out found);
        }
    }

    public delegate void SerializeDelegate(SerializeWalker walker, object obj);
    public delegate object DeserializeDelegate(DeserializerWalker walker, object obj);
    public delegate object DeserializeCreateInstanceDelegate(DeserializerWalker walker);

    public class MyTypeMbInfo
    {
        public readonly Type type;
        public readonly string name;
        public readonly int mbIndex;
        public MyTypeMbInfo(string name, Type type, int mbIndex)
        {
            this.type = type;
            this.name = name;
            this.mbIndex = mbIndex;
        }
    }
    public class MyTypeInfo
    {
        Dictionary<string, MyTypeMbInfo> _dic = new Dictionary<string, MyTypeMbInfo>();
        internal readonly Type _type;
        string _fullname;

        public SerializeDelegate _serDel;
        public DeserializeDelegate _deserDel;
        public DeserializeCreateInstanceDelegate _createInstDel;

        public MyTypeInfo(string fullname, System.Type type)
        {
            _fullname = fullname;
            _type = type;
            _dic.Add("", null);

            GlobalRegisteredTypes.Register(this);
        }
        public string FullName => _fullname;
        //
        public void RegisterMember(string memberName, System.Type memberRetType)
        {
            if (!_dic.ContainsKey(memberName))
            {
                _dic.Add(memberName, new MyTypeMbInfo(memberName, memberRetType, _dic.Count));
            }
        }
        public int TryGetIndex(string memberName)
        {
            if (_dic.TryGetValue(memberName, out var found))
            {
                return found.mbIndex;
            }
            return 0;
        }
    }
}
namespace SharpConnect.Data
{
    public class SerializeWalker
    {
        public StringBuilder _stbuilder;
        bool _useEscapedUnicode = false;
        public SerializeWalker()
        {

        }
        public void WriteStringOrNull(string value)
        {
            //write string or null value 
            //with escape value 
            if (value == null)
            {
                _stbuilder.Append("null");
            }
            else
            {
                WriteStringValueWithEscape(value);
            }
        }
        public void WriteByteBuffer(byte[] buffer)
        {
            //encode buffer as base64 

            _stbuilder.Append(Convert.ToBase64String(buffer));
        }
        public void WriteByte(byte b)
        {
            _stbuilder.Append(b.ToString());
        }
        void WriteStringValueWithEscape(string s)
        {
            StringBuilder _output = _stbuilder;
            _output.Append('\"');

            int runIndex = -1;
            int l = s.Length;
            for (var index = 0; index < l; ++index)
            {
                var c = s[index];

                if (_useEscapedUnicode)
                {
                    if (c >= ' ' && c < 128 && c != '\"' && c != '\\')
                    {
                        if (runIndex == -1)
                            runIndex = index;

                        continue;
                    }
                }
                else
                {
                    if (c != '\t' && c != '\n' && c != '\r' && c != '\"' && c != '\\' && c != '\0')// && c != ':' && c!=',')
                    {
                        if (runIndex == -1)
                            runIndex = index;

                        continue;
                    }
                }

                if (runIndex != -1)
                {
                    _output.Append(s, runIndex, index - runIndex);
                    runIndex = -1;
                }

                switch (c)
                {
                    case '\t': _output.Append("\\t"); break;
                    case '\r': _output.Append("\\r"); break;
                    case '\n': _output.Append("\\n"); break;
                    case '"':
                    case '\\': _output.Append('\\'); _output.Append(c); break;
                    case '\0': _output.Append("\\u0000"); break;
                    default:
                        if (_useEscapedUnicode)
                        {
                            _output.Append("\\u");
                            _output.Append(((int)c).ToString("X4", NumberFormatInfo.InvariantInfo));
                        }
                        else
                            _output.Append(c);

                        break;
                }
            }

            if (runIndex != -1)
                _output.Append(s, runIndex, s.Length - runIndex);

            _output.Append('\"');

        }

        public void WriteInt16(short value)
        {
            _stbuilder.Append(value.ToString());
        }
        public void WriteUInt16(ushort value)
        {
            _stbuilder.Append(value.ToString());
        }
        public void WriteChar(char value)
        {
            _stbuilder.Append(value.ToString());
        }
        //
        public void WriteInt32(int value)
        {
            _stbuilder.Append(value.ToString());
        }
        public void WriteUInt32(uint value)
        {
            _stbuilder.Append(value.ToString());
        }
        public void WriteInt64(int value)
        {
            _stbuilder.Append(value.ToString());
        }
        public void WriteUInt64(uint value)
        {
            _stbuilder.Append(value.ToString());
        }
        //
        public void WriteSingle(float value)
        {
            _stbuilder.Append(value.ToString());
        }
        public void WriteDouble(double value)
        {
            _stbuilder.Append(value.ToString());
        }
        public void WriteDecimal(decimal value)
        {
            _stbuilder.Append(value.ToString());
        }
        void WriteDictionary(IDictionary dic)
        {
            _stbuilder.Append("{");
            if (dic is Dictionary<string, object>)
            {
                Dictionary<string, object> d1 = (Dictionary<string, object>)dic;
                foreach (var kp in d1)
                {
                    WriteStringValueWithEscape(kp.Key);
                    _stbuilder.Append(":");
                    WriteValue(kp.Value);
                }
            }
            else
            {
                //todo ...
            }

            _stbuilder.Append("}");
        }
        //
        public void WriteValueWithTypeHint(object value, Type typeinfo)
        {
            //write date with specific typeinfo

            //get runtime type of the value 
            if (value == null || value is DBNull)
            {
                _stbuilder.Append("null");
            }
            else
            {
                //check exact type of this value

                //if it is basic type /welknown type
                //the we write it as basic type 
                //if not then find proper deserializer 
                if (value is string)
                {
                    WriteStringValueWithEscape((string)value);
                }
                else if (value is char)
                {
                    _stbuilder.Append(value.ToString());
                }
                else if (value is int || value is long ||
                        value is decimal ||
                        value is byte || value is short ||
                        value is sbyte || value is ushort ||
                        value is uint || value is ulong)
                {
                    _stbuilder.Append(((IConvertible)value).ToString(NumberFormatInfo.InvariantInfo));
                }
                else if (value is double || value is Double)
                {
                    double d = (double)value;
                    if (double.IsNaN(d))
                        _stbuilder.Append("\"NaN\"");
                    else if (double.IsInfinity(d))
                    {
                        _stbuilder.Append("\"");
                        _stbuilder.Append(((IConvertible)value).ToString(NumberFormatInfo.InvariantInfo));
                        _stbuilder.Append("\"");
                    }
                    else
                        _stbuilder.Append(((IConvertible)value).ToString(NumberFormatInfo.InvariantInfo));
                }
                else if (value is float || value is Single)
                {
                    float d = (float)value;
                    if (float.IsNaN(d))
                        _stbuilder.Append("\"NaN\"");
                    else if (float.IsInfinity(d))
                    {
                        _stbuilder.Append("\"");
                        _stbuilder.Append(((IConvertible)value).ToString(NumberFormatInfo.InvariantInfo));
                        _stbuilder.Append("\"");
                    }
                    else
                        _stbuilder.Append(((IConvertible)value).ToString(NumberFormatInfo.InvariantInfo));
                }
                else if (value is DateTime)
                {
                    _stbuilder.Append(((DateTime)value).ToString("s"));
                }
                else if (value is byte[])
                {
                    WriteByteBuffer((byte[])value);
                }
                else
                {
                    System.Type t = value.GetType();
                    //check if we have a registered serializer  
                    if (Internal.GlobalRegisteredTypes.TryGetMyTypeInfo(t, out Internal.MyTypeInfo registerTypeInfo))
                    {
                        registerTypeInfo._serDel(this, value);
                        return;
                    }
                    if (value is IDictionary)
                    {
                        WriteDictionary((IDictionary)value);
                    }
                    else if (value is IEnumerable)
                    {
                        WriteIEnumerableAsArray((IEnumerable)value);
                    }
                    else
                    {


                        //this type is not declare as export type
                        //....
                        //then
                        //check only interface impl

                        //use type hint
                        if (Internal.GlobalRegisteredTypes.TryGetMyTypeInfo(typeinfo, out registerTypeInfo))
                        {
                            registerTypeInfo._serDel(this, value);
                            return;
                        }
                        else
                        {
                            //not found
                            _stbuilder.Append("null");
                        }

                    }
                }
            }

        }
        public void WriteValue(object value)
        {
            //write object value....

            //get runtime type of the value 
            if (value == null || value is DBNull)
            {
                _stbuilder.Append("null");
            }
            else
            {
                //check exact type of this value

                //if it is basic type /welknown type
                //the we write it as basic type 
                //if not then find proper deserializer 
                if (value is string)
                {
                    WriteStringValueWithEscape((string)value);
                }
                else if (value is char)
                {
                    _stbuilder.Append(value.ToString());
                }
                else if (value is int || value is long ||
                        value is decimal ||
                        value is byte || value is short ||
                        value is sbyte || value is ushort ||
                        value is uint || value is ulong)
                {
                    _stbuilder.Append(((IConvertible)value).ToString(NumberFormatInfo.InvariantInfo));
                }
                else if (value is double || value is Double)
                {
                    double d = (double)value;
                    if (double.IsNaN(d))
                        _stbuilder.Append("\"NaN\"");
                    else if (double.IsInfinity(d))
                    {
                        _stbuilder.Append("\"");
                        _stbuilder.Append(((IConvertible)value).ToString(NumberFormatInfo.InvariantInfo));
                        _stbuilder.Append("\"");
                    }
                    else
                        _stbuilder.Append(((IConvertible)value).ToString(NumberFormatInfo.InvariantInfo));
                }
                else if (value is float || value is Single)
                {
                    float d = (float)value;
                    if (float.IsNaN(d))
                        _stbuilder.Append("\"NaN\"");
                    else if (float.IsInfinity(d))
                    {
                        _stbuilder.Append("\"");
                        _stbuilder.Append(((IConvertible)value).ToString(NumberFormatInfo.InvariantInfo));
                        _stbuilder.Append("\"");
                    }
                    else
                        _stbuilder.Append(((IConvertible)value).ToString(NumberFormatInfo.InvariantInfo));
                }
                else if (value is DateTime)
                {
                    _stbuilder.Append(((DateTime)value).ToString("s"));
                }
                else if (value is byte[])
                {
                    WriteByteBuffer((byte[])value);
                }
                else
                {
                    System.Type t = value.GetType();
                    //check if we have a registered serializer  
                    if (Internal.GlobalRegisteredTypes.TryGetMyTypeInfo(t, out Internal.MyTypeInfo registerTypeInfo))
                    {
                        registerTypeInfo._serDel(this, value);
                        return;
                    }
                    if (value is IDictionary)
                    {
                        WriteDictionary((IDictionary)value);
                    }
                    else if (value is IEnumerable)
                    {
                        WriteIEnumerableAsArray((IEnumerable)value);
                    }
                    else
                    {
                        //this type is not declare as export type
                        //....
                        //then
                        //check only interface impl


                        throw new NotSupportedException();
                    }
                }
            }
        }
        public void AppendString(string str)
        {
            _stbuilder.Append(str);
        }
        void WriteIEnumerableAsArray(IEnumerable ienum)
        {
            _stbuilder.Append('[');
            bool appendComma = false;
            foreach (var o in ienum)
            {
                if (appendComma) { _stbuilder.Append(','); }
                WriteValue(o);
                //
                appendComma = true;

            }
            _stbuilder.Append(']');
        }

    }
    sealed class JSONParameters
    {
        /// <summary>
        /// Use the optimized fast Dataset Schema format (default = True)
        /// </summary>
        public bool UseOptimizedDatasetSchema = true;
        /// <summary>
        /// Use the fast GUID format (default = True)
        /// </summary>
        public bool UseFastGuid = true;
        /// <summary>
        /// Serialize null values to the output (default = True)
        /// </summary>
        public bool SerializeNullValues = true;
        /// <summary>
        /// Use the UTC date format (default = True)
        /// </summary>
        public bool UseUTCDateTime = true;
        /// <summary>
        /// Show the readonly properties of types in the output (default = False)
        /// </summary>
        public bool ShowReadOnlyProperties = false;
        /// <summary>
        /// Use the $types extension to optimise the output json (default = True)
        /// </summary>
        public bool UsingGlobalTypes = true;
        /// <summary>
        /// Ignore case when processing json and deserializing 
        /// </summary>
        [Obsolete("Not needed anymore and will always match")]
        public bool IgnoreCaseOnDeserialize = false;
        /// <summary>
        /// Anonymous types have read only properties 
        /// </summary>
        public bool EnableAnonymousTypes = false;
        /// <summary>
        /// Enable fastJSON extensions $types, $type, $map (default = True)
        /// </summary>
        public bool UseExtensions = true;
        /// <summary>
        /// Use escaped unicode i.e. \uXXXX format for non ASCII characters (default = True)
        /// </summary>
        public bool UseEscapedUnicode = true;
        /// <summary>
        /// Output string key dictionaries as "k"/"v" format (default = False) 
        /// </summary>
        public bool KVStyleStringDictionary = false;
        /// <summary>
        /// Output Enum values instead of names (default = False)
        /// </summary>
        public bool UseValuesOfEnums = false;
        /// <summary>
        /// Ignore attributes to check for (default : XmlIgnoreAttribute, NonSerialized)
        /// </summary>
        public List<Type> IgnoreAttributes = new List<Type> { /*typeof(System.Xml.Serialization.XmlIgnoreAttribute), */typeof(NonSerializedAttribute) };
        /// <summary>
        /// If you have parametric and no default constructor for you classes (default = False)
        /// 
        /// IMPORTANT NOTE : If True then all initial values within the class will be ignored and will be not set
        /// </summary>
        public bool ParametricConstructorOverride = false;
        /// <summary>
        /// Serialize DateTime milliseconds i.e. yyyy-MM-dd HH:mm:ss.nnn (default = false)
        /// </summary>
        public bool DateTimeMilliseconds = false;
        /// <summary>
        /// Maximum depth for circular references in inline mode (default = 20)
        /// </summary>
        public byte SerializerMaxDepth = 20;
        /// <summary>
        /// Inline circular or already seen objects instead of replacement with $i (default = False) 
        /// </summary>
        public bool InlineCircularReferences = false;
        /// <summary>
        /// Save property/field names as lowercase (default = false)
        /// </summary>
        public bool SerializeToLowerCaseNames = false;
        /// <summary>
        /// Formatter indent spaces (default = 3)
        /// </summary>
        public byte FormatterIndentSpaces = 3;

        public void FixValues()
        {
            if (UseExtensions == false) // disable conflicting params
            {
                UsingGlobalTypes = false;
                InlineCircularReferences = true;
            }
            if (EnableAnonymousTypes)
                ShowReadOnlyProperties = true;
        }
    }

    sealed class JSONSerializer
    {
        private StringBuilder _output = new StringBuilder();
        //private StringBuilder _before = new StringBuilder();
        private int _before;
        private int _MAX_DEPTH = 20;
        int _current_depth = 0;
        private Dictionary<string, int> _globalTypes = new Dictionary<string, int>();
        private Dictionary<object, int> _cirobj = new Dictionary<object, int>();
        private JSONParameters _params;
        private bool _useEscapedUnicode = false;

        internal JSONSerializer(JSONParameters param)
        {
            _params = param;
            _useEscapedUnicode = _params.UseEscapedUnicode;
            _MAX_DEPTH = _params.SerializerMaxDepth;
        }

        internal string ConvertToJSON(object obj)
        {
            WriteValue(obj);

            if (_params.UsingGlobalTypes && _globalTypes != null && _globalTypes.Count > 0)
            {
                var sb = new StringBuilder();
                sb.Append("\"$types\":{");
                var pendingSeparator = false;
                foreach (var kv in _globalTypes)
                {
                    if (pendingSeparator) sb.Append(',');
                    pendingSeparator = true;
                    sb.Append('\"');
                    sb.Append(kv.Key);
                    sb.Append("\":\"");
                    sb.Append(kv.Value);
                    sb.Append('\"');
                }
                sb.Append("},");
                _output.Insert(_before, sb.ToString());
            }
            return _output.ToString();
        }

        private void WriteValue(object obj)
        {
            if (obj == null || obj is DBNull)
                _output.Append("null");

            else if (obj is string || obj is char)
                WriteString(obj.ToString());

            else if (obj is Guid)
                WriteGuid((Guid)obj);

            else if (obj is bool)
                _output.Append(((bool)obj) ? "true" : "false"); // conform to standard

            else if (
                obj is int || obj is long ||
                obj is decimal ||
                obj is byte || obj is short ||
                obj is sbyte || obj is ushort ||
                obj is uint || obj is ulong
            )
                _output.Append(((IConvertible)obj).ToString(NumberFormatInfo.InvariantInfo));

            else if (obj is double || obj is Double)
            {
                double d = (double)obj;
                if (double.IsNaN(d))
                    _output.Append("\"NaN\"");
                else if (double.IsInfinity(d))
                {
                    _output.Append("\"");
                    _output.Append(((IConvertible)obj).ToString(NumberFormatInfo.InvariantInfo));
                    _output.Append("\"");
                }
                else
                    _output.Append(((IConvertible)obj).ToString(NumberFormatInfo.InvariantInfo));
            }
            else if (obj is float || obj is Single)
            {
                float d = (float)obj;
                if (float.IsNaN(d))
                    _output.Append("\"NaN\"");
                else if (float.IsInfinity(d))
                {
                    _output.Append("\"");
                    _output.Append(((IConvertible)obj).ToString(NumberFormatInfo.InvariantInfo));
                    _output.Append("\"");
                }
                else
                    _output.Append(((IConvertible)obj).ToString(NumberFormatInfo.InvariantInfo));
            }

            else if (obj is DateTime)
                WriteDateTime((DateTime)obj);

            else if (obj is DateTimeOffset)
                WriteDateTimeOffset((DateTimeOffset)obj);

            else if (obj is TimeSpan)
                _output.Append(((TimeSpan)obj).Ticks);

#if net4
            else if (_params.KVStyleStringDictionary == false &&
                obj is IEnumerable<KeyValuePair<string, object>>)

                WriteStringDictionary((IEnumerable<KeyValuePair<string, object>>)obj);
#endif

            else if (_params.KVStyleStringDictionary == false && obj is IDictionary &&
                obj.GetType().IsGenericType && obj.GetType().GetGenericArguments()[0] == typeof(string))

                WriteStringDictionary((IDictionary)obj);
            else if (obj is IDictionary)
                WriteDictionary((IDictionary)obj);

            else if (obj is byte[])
                WriteBytes((byte[])obj);

            else if (obj is IEnumerable)
                WriteArray((IEnumerable)obj);

            else if (obj is Enum)
                WriteEnum((Enum)obj);

            //custom type
            //else if (Reflection.Instance.IsTypeRegistered(obj.GetType()))
            //    WriteCustom(obj);

            else
                WriteObject(obj);
        }

        private void WriteDateTimeOffset(DateTimeOffset d)
        {
            DateTime dt = _params.UseUTCDateTime ? d.UtcDateTime : d.DateTime;

            write_date_value(dt);

            var ticks = dt.Ticks % TimeSpan.TicksPerSecond;
            _output.Append('.');
            _output.Append(ticks.ToString("0000000", NumberFormatInfo.InvariantInfo));

            if (_params.UseUTCDateTime)
                _output.Append('Z');
            else
            {
                if (d.Offset.Hours > 0)
                    _output.Append("+");
                else
                    _output.Append("-");
                _output.Append(d.Offset.Hours.ToString("00", NumberFormatInfo.InvariantInfo));
                _output.Append(":");
                _output.Append(d.Offset.Minutes.ToString("00", NumberFormatInfo.InvariantInfo));
            }

            _output.Append('\"');
        }

        //private void WriteNV(NameValueCollection nameValueCollection)
        //{
        //    _output.Append('{');

        //    bool pendingSeparator = false;

        //    foreach (string key in nameValueCollection)
        //    {
        //        if (_params.SerializeNullValues == false && (nameValueCollection[key] == null))
        //        {
        //        }
        //        else
        //        {
        //            if (pendingSeparator) _output.Append(',');
        //            if (_params.SerializeToLowerCaseNames)
        //                WritePair(key.ToLower(), nameValueCollection[key]);
        //            else
        //                WritePair(key, nameValueCollection[key]);
        //            pendingSeparator = true;
        //        }
        //    }
        //    _output.Append('}');
        //}

        //private void WriteSD(StringDictionary stringDictionary)
        //{
        //    _output.Append('{');

        //    bool pendingSeparator = false;

        //    foreach (DictionaryEntry entry in stringDictionary)
        //    {
        //        if (_params.SerializeNullValues == false && (entry.Value == null))
        //        {
        //        }
        //        else
        //        {
        //            if (pendingSeparator) _output.Append(',');

        //            string k = (string)entry.Key;
        //            if (_params.SerializeToLowerCaseNames)
        //                WritePair(k.ToLower(), entry.Value);
        //            else
        //                WritePair(k, entry.Value);
        //            pendingSeparator = true;
        //        }
        //    }
        //    _output.Append('}');
        //}

        //private void WriteCustom(object obj)
        //{
        //    //must found
        //    //if not then throw?
        //    Serialize s;
        //    Reflection.Instance._customSerializer.TryGetValue(obj.GetType(), out s);
        //    WriteStringFast(s(obj));
        //}

        private void WriteEnum(Enum e)
        {
            // FEATURE : optimize enum write
            if (_params.UseValuesOfEnums)
                WriteValue(Convert.ToInt32(e));
            else
                WriteStringFast(e.ToString());
        }

        private void WriteGuid(Guid g)
        {
            if (_params.UseFastGuid == false)
                WriteStringFast(g.ToString());
            else
                WriteBytes(g.ToByteArray());
        }

        private void WriteBytes(byte[] bytes)
        {
#if !SILVERLIGHT
            WriteStringFast(Convert.ToBase64String(bytes, 0, bytes.Length, Base64FormattingOptions.None));
#else
            WriteStringFast(Convert.ToBase64String(bytes, 0, bytes.Length));
#endif
        }

        private void WriteDateTime(DateTime dateTime)
        {
            // datetime format standard : yyyy-MM-dd HH:mm:ss
            DateTime dt = dateTime;
            if (_params.UseUTCDateTime)
                dt = dateTime.ToUniversalTime();

            write_date_value(dt);

            if (_params.DateTimeMilliseconds)
            {
                _output.Append('.');
                _output.Append(dt.Millisecond.ToString("000", NumberFormatInfo.InvariantInfo));
            }

            if (_params.UseUTCDateTime)
                _output.Append('Z');

            _output.Append('\"');
        }

        private void write_date_value(DateTime dt)
        {
            _output.Append('\"');
            _output.Append(dt.Year.ToString("0000", NumberFormatInfo.InvariantInfo));
            _output.Append('-');
            _output.Append(dt.Month.ToString("00", NumberFormatInfo.InvariantInfo));
            _output.Append('-');
            _output.Append(dt.Day.ToString("00", NumberFormatInfo.InvariantInfo));
            _output.Append('T'); // strict ISO date compliance 
            _output.Append(dt.Hour.ToString("00", NumberFormatInfo.InvariantInfo));
            _output.Append(':');
            _output.Append(dt.Minute.ToString("00", NumberFormatInfo.InvariantInfo));
            _output.Append(':');
            _output.Append(dt.Second.ToString("00", NumberFormatInfo.InvariantInfo));
        }
        bool _TypesWritten = false;
        private void WriteObject(object obj)
        {
            int i = 0;
            if (_cirobj.TryGetValue(obj, out i) == false)
                _cirobj.Add(obj, _cirobj.Count + 1);
            else
            {
                if (_current_depth > 0 && _params.InlineCircularReferences == false)
                {
                    //_circular = true;
                    _output.Append("{\"$i\":");
                    _output.Append(i.ToString());
                    _output.Append("}");
                    return;
                }
            }
            if (_params.UsingGlobalTypes == false)
                _output.Append('{');
            else
            {
                if (_TypesWritten == false)
                {
                    _output.Append('{');
                    _before = _output.Length;
                    //_output = new StringBuilder();
                }
                else
                    _output.Append('{');
            }
            _TypesWritten = true;
            _current_depth++;
            if (_current_depth > _MAX_DEPTH)
                throw new Exception("Serializer encountered maximum depth of " + _MAX_DEPTH);


            //Dictionary<string, string> map = new Dictionary<string, string>();
            //Type t = obj.GetType();
            //bool append = false;
            //if (_params.UseExtensions)
            //{
            //    if (_params.UsingGlobalTypes == false)
            //        WritePairFast("$type", Reflection.Instance.GetTypeAssemblyName(t));
            //    else
            //    {
            //        int dt = 0;
            //        string ct = Reflection.Instance.GetTypeAssemblyName(t);
            //        if (_globalTypes.TryGetValue(ct, out dt) == false)
            //        {
            //            dt = _globalTypes.Count + 1;
            //            _globalTypes.Add(ct, dt);
            //        }
            //        WritePairFast("$type", dt.ToString());
            //    }
            //    append = true;
            //}

            //Getters[] g = Reflection.Instance.GetGetters(t, _params.ShowReadOnlyProperties, _params.IgnoreAttributes);
            //int c = g.Length;
            //for (int ii = 0; ii < c; ii++)
            //{
            //    var p = g[ii];
            //    object o = p.Getter(obj);
            //    if (_params.SerializeNullValues == false && (o == null || o is DBNull))
            //    {
            //        //append = false;
            //    }
            //    else
            //    {
            //        if (append)
            //            _output.Append(',');
            //        if (p.memberName != null)
            //            WritePair(p.memberName, o);
            //        else if (_params.SerializeToLowerCaseNames)
            //            WritePair(p.lcName, o);
            //        else
            //            WritePair(p.Name, o);
            //        if (o != null && _params.UseExtensions)
            //        {
            //            Type tt = o.GetType();
            //            if (tt == typeof(System.Object))
            //                map.Add(p.Name, tt.ToString());
            //        }
            //        append = true;
            //    }
            //}
            //if (map.Count > 0 && _params.UseExtensions)
            //{
            //    _output.Append(",\"$map\":");
            //    WriteStringDictionary(map);
            //}
            _output.Append('}');
            _current_depth--;
        }

        private void WritePairFast(string name, string value)
        {
            WriteStringFast(name);

            _output.Append(':');

            WriteStringFast(value);
        }

        private void WritePair(string name, object value)
        {
            WriteString(name);

            _output.Append(':');

            WriteValue(value);
        }

        private void WriteArray(IEnumerable array)
        {
            _output.Append('[');

            bool pendingSeperator = false;

            foreach (object obj in array)
            {
                if (pendingSeperator) _output.Append(',');

                WriteValue(obj);

                pendingSeperator = true;
            }
            _output.Append(']');
        }

        private void WriteStringDictionary(IDictionary dic)
        {
            _output.Append('{');

            bool pendingSeparator = false;

            foreach (DictionaryEntry entry in dic)
            {
                if (_params.SerializeNullValues == false && (entry.Value == null))
                {
                }
                else
                {
                    if (pendingSeparator) _output.Append(',');

                    string k = (string)entry.Key;
                    if (_params.SerializeToLowerCaseNames)
                        WritePair(k.ToLower(), entry.Value);
                    else
                        WritePair(k, entry.Value);
                    pendingSeparator = true;
                }
            }
            _output.Append('}');
        }

        private void WriteStringDictionary(IEnumerable<KeyValuePair<string, object>> dic)
        {
            _output.Append('{');
            bool pendingSeparator = false;
            foreach (KeyValuePair<string, object> entry in dic)
            {
                if (_params.SerializeNullValues == false && (entry.Value == null))
                {
                }
                else
                {
                    if (pendingSeparator) _output.Append(',');
                    string k = entry.Key;

                    if (_params.SerializeToLowerCaseNames)
                        WritePair(k.ToLower(), entry.Value);
                    else
                        WritePair(k, entry.Value);
                    pendingSeparator = true;
                }
            }
            _output.Append('}');
        }

        private void WriteDictionary(IDictionary dic)
        {
            _output.Append('[');

            bool pendingSeparator = false;

            foreach (DictionaryEntry entry in dic)
            {
                if (pendingSeparator) _output.Append(',');
                _output.Append('{');
                WritePair("k", entry.Key);
                _output.Append(",");
                WritePair("v", entry.Value);
                _output.Append('}');

                pendingSeparator = true;
            }
            _output.Append(']');
        }

        private void WriteStringFast(string s)
        {
            _output.Append('\"');
            _output.Append(s);
            _output.Append('\"');
        }

        private void WriteString(string s)
        {
            _output.Append('\"');

            int runIndex = -1;
            int l = s.Length;
            for (var index = 0; index < l; ++index)
            {
                var c = s[index];

                if (_useEscapedUnicode)
                {
                    if (c >= ' ' && c < 128 && c != '\"' && c != '\\')
                    {
                        if (runIndex == -1)
                            runIndex = index;

                        continue;
                    }
                }
                else
                {
                    if (c != '\t' && c != '\n' && c != '\r' && c != '\"' && c != '\\' && c != '\0')// && c != ':' && c!=',')
                    {
                        if (runIndex == -1)
                            runIndex = index;

                        continue;
                    }
                }

                if (runIndex != -1)
                {
                    _output.Append(s, runIndex, index - runIndex);
                    runIndex = -1;
                }

                switch (c)
                {
                    case '\t': _output.Append("\\t"); break;
                    case '\r': _output.Append("\\r"); break;
                    case '\n': _output.Append("\\n"); break;
                    case '"':
                    case '\\': _output.Append('\\'); _output.Append(c); break;
                    case '\0': _output.Append("\\u0000"); break;
                    default:
                        if (_useEscapedUnicode)
                        {
                            _output.Append("\\u");
                            _output.Append(((int)c).ToString("X4", NumberFormatInfo.InvariantInfo));
                        }
                        else
                            _output.Append(c);

                        break;
                }
            }

            if (runIndex != -1)
                _output.Append(s, runIndex, s.Length - runIndex);

            _output.Append('\"');
        }
    }


    public class DeserializerWalker
    {
        //we walk along the data stream

        Dictionary<string, object> _rootObject;
        Dictionary<string, object>.Enumerator _objEnum;
        KeyValuePair<string, object> _currentValue; 
        public void SetRootObject(Dictionary<string, object> rootObject)
        {
            _rootObject = rootObject;
            _objEnum = rootObject.GetEnumerator();
        }

        public bool Read()
        {
            //read on current level
            bool moveNext = _objEnum.MoveNext();
            _currentValue = _objEnum.Current;
            return moveNext;
        }
        public string ReadKey()
        {
            return _currentValue.Key;
        }
        public string ReadValueAsString()
        {
            return Convert.ToString(_currentValue.Value);
        }
        public object ReadValueAsObject(System.Type type)
        {
            //read data as specific object type
            if (SharpConnect.Data.Internal.GlobalRegisteredTypes.TryGetMyTypeInfo(type,
                out SharpConnect.Data.Internal.MyTypeInfo foundMyTypeInfo))
            {
                //found mytype info
                //1. create that type 
                var data = _currentValue.Value as Dictionary<string, object>;

                if (data != null && foundMyTypeInfo._createInstDel != null)
                {
                    object newInst = foundMyTypeInfo._createInstDel(this);
                    if (newInst != null)
                    {
                        DeserializerWalker newWalker = new DeserializerWalker();
                        newWalker.SetRootObject(data);
                        object result = foundMyTypeInfo._deserDel(newWalker, newInst);
                        return result;
                    }
                }
            }
            return null;
        }
        //8
        public byte ReadValueAsByte()
        {
            return Convert.ToByte(_currentValue.Value);
        }
        //16
        public short ReadValueAsInt16()
        {
            return Convert.ToInt16(_currentValue.Value);
        }
        public ushort ReadValueAsUInt16()
        {
            return Convert.ToUInt16(_currentValue.Value);
        }
        public char ReadChar()
        {
            return Convert.ToChar(_currentValue.Value);
        }
        //32
        public int ReadValueAsInt32()
        {
            return Convert.ToInt32(_currentValue.Value);
        }
        public uint ReadValueAsUInt32()
        {
            return Convert.ToUInt32(_currentValue.Value);
        }
        //64
        public long ReadValueAsInt64()
        {
            return Convert.ToInt64(_currentValue.Value);
        }
        public ulong ReadValueAsUInt64()
        {
            return Convert.ToUInt64(_currentValue.Value);
        }
        //
        public double ReadValueAsDouble()
        {
            return Convert.ToDouble(_currentValue.Value);
        }
        public float ReadValueAsSingle()
        {
            return Convert.ToSingle(_currentValue.Value);
        }
        public decimal ReadValueAsDecimal()
        {
            return Convert.ToDecimal(_currentValue.Value);
        }

        //byte buffer
        public byte[] ReadAsByteBuffer()
        {
            return _currentValue.Value as byte[];
        }
        //date-time


    }

}