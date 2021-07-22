//MIT, 2015-present, brezza92, EngineKit and contributors

using System;
using System.Collections.Generic;
using System.Text;
namespace SharpConnect.Data
{
    public class EaseDocument : EsDoc
    {
        //Dictionary<string, int> _stringTable = new Dictionary<string, int>();
        public EaseDocument()
        {

        }
        public EsElem CreateElement(string elementName)
        {
            var elem = new EaseElement();
            elem.Name = elementName;
            return elem;
        }

        public EsElem CreateElement() => new EaseElement();

        public EsArr CreateArray() => new EaseArray();

        //public int GetStringIndex(string str)
        //{
        //    _stringTable.TryGetValue(str, out int found);
        //    return found;
        //}

        public EsElem DocumentElement { get; set; }

        public EsElem Parse(string jsonstr)
        {
            return Parse(jsonstr.ToCharArray());
        }
        public EsElem Parse(char[] jsonstr)
        {
            var parser = new EaseDocParser(this);
            parser.Parse(jsonstr);
            return parser.CurrentElement as EsElem;
        }
        public EsAttr CreateAttribute(string key, object value) => new EaseAttribute(key, value);
    }


    static class EsElemHelper
    {
        public static EsElem CreateXmlElementForDynamicObject(EsDoc doc)
        {
            var elem = new EaseElement();
            elem.Name = "!j";
            return elem;
        }
    }
    class EaseArray : EsArr
    {
        List<object> _member = new List<object>();

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

    class EaseElement : EsElem
    {
        List<EsElem> _childNodes;

        Dictionary<string, int> _attrs = new Dictionary<string, int>();
        List<EaseAttribute> _attrsValues = new List<EaseAttribute>();

        public EaseElement()
        {
            Name = "";
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

        public void AppendChild(EsElem element)
        {
            if (_childNodes == null)
            {
                _childNodes = new List<EsElem>();
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
    }

    class EaseAttribute : EsAttr
    {
        public EaseAttribute(string name, object value)
        {
            Name = name;
            Value = value;
        }
        public string Name { get; }
        public object Value { get; }
        public override string ToString() => Name + ":" + Value;
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
        public static void WriteJson(this EsDoc doc, StringBuilder stBuilder)
        {
            //write to 
            var docElem = doc.DocumentElement;
            if (docElem != null)
            {
                WriteJson(docElem, stBuilder);
            }
        }

        static void WriteJson(EsArr esArr, StringBuilder stBuilder)
        {
            stBuilder.Append('[');
            int j = esArr.Count;
            for (int i = 0; i < j; ++i)
            {
                if (i > 0)
                {
                    stBuilder.Append(',');
                }
                WriteJson(esArr[i], stBuilder);
            }
            stBuilder.Append(']');
        }
        public static void WriteJson(this EsElem esElem, StringBuilder stBuilder)
        {
            EaseElement leqE = (EaseElement)esElem;
            stBuilder.Append('{');
            //check docattr= 
            var nameAttr = leqE.GetAttributeValueAsString("!n");
            int attrCount = 0;
            if (nameAttr == null)
            {
                //TODO: review here if we want auto element name or not?
                //use specific name
                //stBuilder.Append("\"!n\":\"");
                //stBuilder.Append(leqE.Name);
                //stBuilder.Append('"');
            }
            else
            {
                //use default elementname
                stBuilder.Append("\"!n\":\"");
                //TODO: review string escape here ***
                stBuilder.Append(leqE.Name);
                stBuilder.Append('"');
                attrCount = 1;
            }


            foreach (var attr in leqE.GetAttributeIterForward())
            {
                if (attr.Name == "!n")
                {
                    continue;
                }
                if (attrCount > 0)
                {
                    stBuilder.Append(',');
                }
                stBuilder.Append('"');
                stBuilder.Append(attr.Name); //TODO: review escape string here
                stBuilder.Append('"');
                stBuilder.Append(':');
                WriteJson(attr.Value, stBuilder);
                attrCount++;
            }
            //-------------------
            //for children nodes
            int j = leqE.ChildCount;
            //create children nodes
            if (j > 0)
            {
                if (attrCount > 0)
                {
                    stBuilder.Append(',');
                }
                stBuilder.Append("\"!c\":[");
                for (int i = 0; i < j; ++i)
                {
                    if (i > 0)
                    {
                        stBuilder.Append(',');
                    }
                    WriteJson(leqE.GetChild(i), stBuilder);
                }
                stBuilder.Append(']');
            }
            //-------------------
            stBuilder.Append('}');
        }

        public static string ToJsonString(this EsElem esElem)
        {
            var stbuilder = new StringBuilder();
            esElem.WriteJson(stbuilder);
            return stbuilder.ToString();
        }


        public static void WriteJson(object elem, StringBuilder stBuilder)
        {
            //recursive
#if DEBUG
            Type t = elem.GetType();
#endif
            if (elem == null)
            {
                stBuilder.Append("null");
            }
            else if (elem is string)
            {
                stBuilder.Append('"');
                //TODO: proper escape json string
                //ensure we scape " inside this string
                stBuilder.Append((string)elem);
                stBuilder.Append('"');
            }
            else if (elem is double ||
                (elem is float) ||
                (elem is int) ||
                (elem is uint))
            {
                //TODO: review all primitive conversion
                stBuilder.Append(elem.ToString());
            }
            else if (elem is Array a)
            {
                stBuilder.Append('[');
                //write element into array

                int j = a.Length;
                for (int i = 0; i < j; ++i)
                {
                    if (i > 0)
                    {
                        stBuilder.Append(',');
                    }
                    WriteJson(a.GetValue(i), stBuilder);
                }
                stBuilder.Append(']');
            }
            else if (elem is EaseElement ease_elem)
            {
                WriteJson(ease_elem, stBuilder);
            }
            else if (elem is EsArr es_arr)
            {
                WriteJson(es_arr, stBuilder);
            }
            else if (elem is DateTime d)
            {
                //write datetime as string
                stBuilder.Append('"');
                stBuilder.Append(string.Format("{0:u}", d));
                stBuilder.Append('"');
            }
            else
            {
                //get if we 
                Type elemType = elem.GetType();
                //find codec of this type

                stBuilder.Append(elem.ToString());
                //throw new NotSupportedException();
            }
        }
       
    }
}