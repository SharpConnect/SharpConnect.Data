//MIT, 2015-present, brezza92, EngineKit and contributors

using System;
using System.Collections.Generic;
using System.Text;
namespace SharpConnect.Data
{
    public class EaseDocument : EsDoc
    {
        public EaseDocument()
        {

        }
        public EaseElement CreateElement(string elementName)
        {
            var elem = new EaseElement();
            elem.Name = elementName;
            return elem;
        }
        public bool EnableExtension { get; set; }
        public EaseElement CreateElement() => new EaseElement();
        public EaseArray CreateArray() => new EaseArray();
        public EsElem DocumentElement { get; set; }
        public EaseAttribute CreateAttribute(string key, object value) => new EaseAttribute(key, value);

    }

    public static class EaseDocumentExtensions
    {
        public static object Parse(this EaseDocument doc, string jsonstr)
        {
            return Parse(doc, jsonstr.ToCharArray());
        }
        public static object Parse(this EaseDocument doc, char[] jsonstr)
        {
            var parser = new EaseDocParser(doc);
            parser.EnableExtension = doc.EnableExtension;
            parser.Parse(jsonstr);
            return parser.CurrentValue;
        }
    }

    public sealed class EaseArray : EsArr
    {
        List<object> _member = new List<object>();
        internal EaseArray() { }
        public object this[int index]
        {
            get => _member[index];
            set => _member[index] = value;
        }

        public int Count => _member.Count;

        public void AddItem(object item)
        {
            _member.Add(item);
        }

        public void Clear()
        {
            _member.Clear();
        }

        public IEnumerable<object> GetIter()
        {
            foreach (object obj in _member)
            {
                yield return obj;
            }
        }
    }

    public sealed class EaseElement : EsElem
    {

        List<object> _childNodes;
        Dictionary<string, int> _attrs = new Dictionary<string, int>();
        List<EaseAttribute> _attrsValues = new List<EaseAttribute>();

#if DEBUG
        static int s_dbugTotal;
        public readonly int dbugId = s_dbugTotal++;
#endif
        internal EaseElement()
        {
            Name = "";
#if DEBUG
            if (dbugId == 409)
            {

            }
#endif
        }
        public string Name { get; set; }
        public int AttributeCount => _attrs.Count;
        public int ChildCount => (_childNodes == null) ? 0 : _childNodes.Count;
        public object GetChild(int index) => _childNodes[index];
        public IEnumerable<EsAttr> GetAttributeIterForward()
        {
            if (_attrsValues != null)
            {
                foreach (EaseAttribute kv in _attrsValues)
                {
                    yield return kv;
                }
            }
        }

        public void AppendChild(object element)
        {
            if (_childNodes == null)
            {
                _childNodes = new List<object>();
            }
            _childNodes.Add(element);
        }
        public void RemoveAttribute(string key)
        {
            if (_attrs.TryGetValue(key, out int index))
            {
                _attrs.Remove(key);
                _attrsValues.RemoveAt(index);
            }
        }

        public void AppendAttribute(string key, object value)
        {
            //check unique key
            if (!_attrs.TryGetValue(key, out int index))
            {
                _attrs.Add(key, _attrsValues.Count);
                _attrsValues.Add(new EaseAttribute(key, value));
            }
            else
            {
                throw new Exception("duplicated key");
            }
        }


        public object GetAttributeValue(string key)
        {
            if (_attrs.TryGetValue(key, out int index))
            {
                return _attrsValues[index].Value;
            }
            return null;
        }
        public EsAttr GetAttribute(int index)
        {
            return _attrsValues[index];
        }

        public EsAttr GetAttribute(string key)
        {
            if (_attrs.TryGetValue(key, out int index))
            {
                return _attrsValues[index];
            }
            return null;
        }
        public object UserData { get; set; }

#if DEBUG
        public override string ToString() => Name;
#endif
    }

    public sealed class EaseAttribute : EsAttr
    {
        internal EaseAttribute(string name, object value)
        {
            Name = name;
            Value = value;
        }
        public string Name { get; }
        public object Value { get; }
        public override string ToString() => Name + ":" + Value;
    }

    public class EsElemJsonWriteParameters
    {
        public bool WithElemName { get; set; }
        public StringBuilder Output { get; set; }
    }
    public static class EsElemExtensionMethods
    {
        public static string GetAttrValueOrDefaultAsString(this EsElem esElem, string attrName)
        {
            return esElem.GetAttributeValue(attrName) as string;
        }
        public static int GetAttrValueOrDefaultAsInt32(this EsElem esElem, string attrName)
        {
            object value = esElem.GetAttributeValue(attrName);
            if (value == null)
            {
                return 0;
            }
            return Convert.ToInt32(value);
        }
        public static double GetAttrValueOrDefaultAsDouble(this EsElem esElem, string attrName)
        {
            object value = esElem.GetAttributeValue(attrName);
#if DEBUG
            var t = value.GetType();
#endif
            if (value == null)
            {
                return 0;
            }
            return Convert.ToDouble(value);
        }
        public static bool GetAttrValueOrDefaultAsBool(this EsElem esElem, string attrName)
        {
            object value = esElem.GetAttributeValue(attrName);
            if (value == null)
            {
                return false;
            }
            return (bool)value;
        }
        //----------------------------------------------------------------------- 
        public static DateTime GetAttributeValueAsDateTime(this EsElem esElem, string attrName)
        {
            return (esElem.GetAttributeValue(attrName) is DateTime dtm) ? dtm : DateTime.MinValue;
        }
        public static string GetAttributeValueAsString(this EsElem esElem, string attrName)
        {
            return esElem.GetAttributeValue(attrName) as string;
        }
        public static double GetAttributeValueAsDouble(this EsElem esElem, string attrName)
        {
            return Convert.ToDouble(esElem.GetAttributeValue(attrName));
        }
        public static int GetAttributeValueAsInt32(this EsElem esElem, string attrName)
        {
            return Convert.ToInt32(esElem.GetAttributeValue(attrName));
        }
        public static uint GetAttributeValueAsUInt32(this EsElem esElem, string attrName)
        {
            return Convert.ToUInt32(esElem.GetAttributeValue(attrName));
        }
        public static bool GetAttributeValueAsBool(this EsElem esElem, string attrName)
        {
            return (bool)esElem.GetAttributeValue(attrName);
        }
        public static bool GetAttributeValueAsBool(this EsElem esElem, string attrName, bool defaultIfNotExists)
        {
            object found = esElem.GetAttributeValue(attrName);
            if (found == null) return defaultIfNotExists;
            return (bool)found;
        }
        public static EsArr GetAttributeValueAsArray(this EsElem esElem, string attrName)
        {
            return esElem.GetAttributeValue(attrName) as EsArr;
        }
        public static EsElem GetAttributeValueAsElem(this EsElem esElem, string attrName)
        {
            return esElem.GetAttributeValue(attrName) as EsElem;
        }

        //-----------------------------------------------------------------------
        public static void WriteJson(this EsDoc doc, EsElemJsonWriteParameters writePars)
        {
            //write to 
            var docElem = doc.DocumentElement;
            if (docElem != null)
            {
                WriteJson(docElem, writePars);
            }
        }
        public static void WriteJson(this EsDoc doc, StringBuilder stbuilder)
        {
            EsElemJsonWriteParameters pars = new EsElemJsonWriteParameters();
            pars.Output = stbuilder;
            WriteJson(doc, pars);
        }
        static void WriteJson(EsArr esArr, EsElemJsonWriteParameters pars)
        {
            StringBuilder sb = pars.Output;
            sb.Append('[');
            int j = esArr.Count;
            for (int i = 0; i < j; ++i)
            {
                if (i > 0)
                {
                    sb.Append(',');
                }
                WriteJson(esArr[i], pars);
            }
            sb.Append(']');
        }

        public static void WriteJson(this EsElem esElem, StringBuilder sb)
        {
            WriteJson(esElem, new EsElemJsonWriteParameters { Output = sb });
        }
        public static void WriteJson(this EsElem esElem, EsElemJsonWriteParameters pars)
        {
            StringBuilder sb = pars.Output;
            EaseElement elem = (EaseElement)esElem;
            sb.Append('{');
            //check docattr= 
            string nameAttr = elem.GetAttributeValueAsString("!n");
            int attrCount = 0;
            if (nameAttr == null)
            {
                //TODO: review here if we want auto element name or not?
                //use specific name
                if (pars.WithElemName && elem.Name != null)
                {
                    sb.Append("\"!n\":\"");
                    //TODO: review string escape here ***
                    sb.Append(elem.Name);
                    sb.Append('"');
                    attrCount = 1;
                }
            }
            else
            {
                //use default elementname
                if (elem.Name != null)
                {
                    sb.Append("\"!n\":\"");
                    //TODO: review string escape here ***
                    sb.Append(elem.Name);
                    sb.Append('"');
                    attrCount = 1;
                }
            }
            foreach (EsAttr attr in elem.GetAttributeIterForward())
            {
                if (attr.Name == "!n")
                {
                    continue;
                }
                if (attrCount > 0)
                {
                    sb.Append(',');
                }
                sb.Append('"');
                sb.Append(attr.Name); //TODO: review escape string here
                sb.Append('"');
                sb.Append(':');
                WriteJson(attr.Value, pars);
                attrCount++;
            }
            //-------------------
            //for children nodes
            int j = elem.ChildCount;
            //create children nodes
            if (j > 0)
            {
                if (attrCount > 0)
                {
                    sb.Append(',');
                }
                sb.Append("\"!c\":[");
                for (int i = 0; i < j; ++i)
                {
                    if (i > 0)
                    {
                        sb.Append(',');
                    }
                    WriteJson(elem.GetChild(i), pars);
                }
                sb.Append(']');
            }
            //-------------------
            sb.Append('}');
        }


        public static string ToJsonString(this EsElem esElem)
        {
            //TODO: review here again
            var stbuilder = new StringBuilder();
            var pars = new EsElemJsonWriteParameters();
            pars.Output = stbuilder;
            esElem.WriteJson(pars);
            return stbuilder.ToString();
        }

        public static void WriteJson(object elem, EsElemJsonWriteParameters pars)
        {
            //recursive
#if DEBUG
            Type t = elem.GetType();
#endif
            StringBuilder sb = pars.Output;
            if (elem == null)
            {
                sb.Append("null");
            }
            else if (elem is string)
            {
                sb.Append('"');
                //TODO: proper escape json string
                //ensure we scape " inside this string
                sb.Append((string)elem);
                sb.Append('"');
            }
            else if (elem is double ||
                (elem is float) ||
                (elem is int) ||
                (elem is uint))
            {
                //TODO: review all primitive conversion
                sb.Append(elem.ToString());
            }
            else if (elem is Array a)
            {
                sb.Append('[');
                //write element into array

                int j = a.Length;
                for (int i = 0; i < j; ++i)
                {
                    if (i > 0)
                    {
                        sb.Append(',');
                    }
                    WriteJson(a.GetValue(i), pars);
                }
                sb.Append(']');
            }
            else if (elem is EaseElement ease_elem)
            {
                WriteJson(ease_elem, pars);
            }
            else if (elem is EsArr es_arr)
            {
                WriteJson(es_arr, pars);
            }
            else if (elem is DateTime d)
            {
                //write datetime as string
                sb.Append('"');
                sb.Append(string.Format("{0:u}", d));
                sb.Append('"');
            }
            else
            {
                //get if we 
                Type elemType = elem.GetType();
                //find codec of this type

                sb.Append(elem.ToString());
                //throw new NotSupportedException();
            }
        }

    }
}