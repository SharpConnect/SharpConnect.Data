//MIT, 2015-2016, brezza92, EngineKit and contributors

using System;
using System.Collections.Generic;
using System.Text;
namespace SharpConnect.Data
{
    public class EaseDocument : EsDoc
    {
        Dictionary<string, int> stringTable = new Dictionary<string, int>();
        public EaseDocument()
        {

        }
        public EsElem CreateElement(string elementName)
        {
            return new EaseElement(elementName, this);
        }
        public EsElem CreateElement()
        {
            return new EaseElement("", this);
        }
        public EsArr CreateArray()
        {
            return new EaseArray();
        }
        public int GetStringIndex(string str)
        {
            int found;
            stringTable.TryGetValue(str, out found);
            return found;
        }
        public EsElem DocumentElement
        {
            get;
            set;
        }
        public EsElem Parse(string jsonstr)
        {
            char[] buffer = jsonstr.ToCharArray();
            return Parse(buffer);         
        }
        public EsElem Parse(char[] jsonstr)
        {
            var parser = new EaseDocParser(this);
            parser.Parse(jsonstr);
            return parser.CurrentElement as EsElem;
        }
    }

    class EaseDocParser : EsParserBase
    {
        enum CurrentObject
        {
            Object,
            Array
        }

        EaseDocument easeDoc;
        Stack<string> keyStack = new Stack<string>();
        Stack<object> elemStack = new Stack<object>();
        object currentElem = null;
        string currentKey = null;
        public EaseDocParser(EaseDocument blankdoc)
        {
            easeDoc = blankdoc;
        }
        protected override void OnParseStart()
        {
            if (easeDoc == null)
            {
                easeDoc = new EaseDocument();
            }
        }
        protected override void BeginObject()
        {
            if (currentElem != null)
            {
                elemStack.Push(currentElem);
            }
            currentElem = easeDoc.CreateElement();
        }
        protected override void EndObject()
        {
            //current element should be object
            object c_object = currentElem;
            if (elemStack.Count > 0)
            {
                //pop from stack
                currentElem = elemStack.Pop();

                if (c_object == currentElem)
                {
                    throw new System.Exception();
                }
                if (currentElem is EsElem)
                {
                    ((EsElem)currentElem)[currentKey] = c_object;
                }
                else if (currentElem is EsArr)
                {
                    ((EsArr)currentElem).AddItem(c_object);
                }
                else
                {
                    throw new System.NotSupportedException();
                }
            }
        }
        protected override void BeginArray()
        {
            if (currentElem != null)
            {
                elemStack.Push(currentElem);
            }
            currentElem = easeDoc.CreateArray();
        }
        protected override void EndArray()
        {
            object c_object = currentElem;
            if (elemStack.Count > 0)
            {
                //pop from stack
                currentElem = elemStack.Pop();

                if (c_object == currentElem)
                {
                    throw new System.Exception();
                }
                if (currentElem is EsElem)
                {
                    ((EsElem)currentElem)[currentKey] = c_object;
                }
                else if (currentElem is EsArr)
                {
                    ((EsArr)currentElem).AddItem(c_object);
                }
                else
                {
                    throw new System.NotSupportedException();
                }
            }
        }
        protected override void OnParseEnd()
        {

        }
        protected override void NewKey(StringBuilder tmpBuffer, ValueHint valueHint)
        {
            //add key to current object
            currentKey = tmpBuffer.ToString();
        }
        protected override void NewValue(StringBuilder tmpBuffer, ValueHint valueHint)
        {
            object c_object = null;
            switch (valueHint)
            {
                default:
                case ValueHint.Comment:
                    throw new System.NotSupportedException();
                case ValueHint.Identifier:
                    string iden = tmpBuffer.ToString();
                    switch (iden)
                    {
                        case "true":
                            c_object = true;
                            break;
                        case "false":
                            c_object = false;
                            break;
                        case "null":
                            c_object = null;
                            break;
                        default:
                            c_object = iden;
                            break;
                    }
                    break;
                case ValueHint.StringLiteral:
                    c_object = tmpBuffer.ToString();
                    break;
                case ValueHint.IntegerNumber:
                    c_object = int.Parse(tmpBuffer.ToString());
                    break;
                case ValueHint.NumberWithFractionPart:
                case ValueHint.NumberWithSignedExponentialPart:
                case ValueHint.NumberWithExponentialPart:
                    c_object = double.Parse(tmpBuffer.ToString());
                    break;

            }


            if (currentElem is EsElem)
            {
                ((EsElem)currentElem)[currentKey] = c_object;
            }
            else if (currentElem is EsArr)
            {
                ((EsArr)currentElem).AddItem(c_object);
            }
            else
            {
                throw new System.NotSupportedException();
            }
        }
        protected override void NotifyError()
        {
            base.NotifyError();
        }
        protected override void OnError(ref int currentIndex)
        {
            base.OnError(ref currentIndex);
        }
        public object CurrentElement { get { return currentElem; } }
    }






    class EaseArray : List<object>, EsArr
    {
        public void AddItem(object item)
        {
            Add(item);
        }
        public IEnumerable<object> GetIterForward()
        {
            foreach (object obj in this)
            {
                yield return obj;
            }
        }
    }
    static class EsElemHelper
    {
        public static EsElem CreateXmlElementForDynamicObject(EsDoc doc)
        {
            return new EaseElement("!j", null);
        }
    }

    class EaseElement : EsElem
    {
        //xml-like element

        string _name;
        int _nameIndex;
        EsDoc _owner;
        List<EsElem> _childNodes;
        Dictionary<string, EsAttr> _attributeDic01 = new Dictionary<string, EsAttr>();
        public EaseElement(string elementName, EsDoc ownerdoc)
        {
            _name = elementName;
            _owner = ownerdoc;
        }
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }
        public EsDoc OwnerDocument
        {
            get
            {
                return _owner;
            }
        }
        public bool HasOwnerDocument
        {
            get
            {
                return _owner != null;
            }
        }
        public int ChildCount
        {
            get
            {
                if (_childNodes == null)
                {
                    return 0;
                }
                else
                {
                    return _childNodes.Count;
                }
            }
        }
        public object GetChild(int index)
        {
            return _childNodes[index];
        }
        public int NameIndex
        {
            get
            {
                return _nameIndex;
            }
        }
        public IEnumerable<EsAttr> GetAttributeIterForward()
        {
            if (_attributeDic01 != null)
            {
                foreach (EsAttr attr in this._attributeDic01.Values)
                {
                    yield return attr;
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
        public void RemoveAttribute(EsAttr attr)
        {
            _attributeDic01.Remove(attr.Name);
        }
        public void AppendAttribute(EsAttr attr)
        {
            _attributeDic01.Add(attr.Name, attr);
        }
        public EsAttr AppendAttribute(string key, object value)
        {
            var attr = new EaseAttribute(key, value);
            _attributeDic01.Add(key, attr);
            return attr;
        }

        public object GetAttributeValue(string key)
        {
            EsAttr found = GetAttribute(key);
            if (found != null)
            {
                return found.Value;
            }
            else
            {
                return null;
            }
        }
        public EsAttr GetAttribute(string key)
        {
            EsAttr existing;
            _attributeDic01.TryGetValue(key, out existing);
            return existing;
        }
        /// <summary>
        /// get attribute value if exist / set=> insert or replace existing value with specific value
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object this[string key]
        {
            get
            {
                EsAttr found = GetAttribute(key);
                if (found == null)
                {
                    return null;
                }
                else
                {
                    return found.Value;
                }
            }
            set
            {
                //replace value if existing
                //we create new attr and replace it
                //so it not affect existing attr
                _attributeDic01[key] = new EaseAttribute(key, value);
            }
        }
    }
    class EaseAttribute : EsAttr
    {
        int _localNameIndex;
        public EaseAttribute()
        {
        }
        public EaseAttribute(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public string Name
        {
            get;
            set;
        }
        public object Value
        {
            get;
            set;
        }
        public int AttributeLocalNameIndex
        {
            get
            {
                return _localNameIndex;
            }
        }
        public override string ToString()
        {
            return Name + ":" + Value;
        }
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
        public static string GetAttributeValueAsString(this EsElem esElem, string attrName)
        {
            return esElem.GetAttributeValue(attrName) as string;
        }
        public static int GetAttributeValueAsInt32(this EsElem esElem, string attrName)
        {
            return (int)esElem.GetAttributeValue(attrName);
        }
        public static bool GetAttributeValueAsBool(this EsElem esElem, string attrName)
        {
            return (bool)esElem.GetAttributeValue(attrName);
        }
        public static EsArr GetAttributeValueAsArray(this EsElem esElem, string attrName)
        {
            return esElem.GetAttributeValue(attrName) as EsArr;
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
            var nameAttr = leqE.GetAttribute("!n");
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


        static void WriteJson(object elem, StringBuilder stBuilder)
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
            else if (elem is Array)
            {
                stBuilder.Append('[');
                //write element into array
                Array a = elem as Array;
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
            else if (elem is EaseElement)
            {
                WriteJson((EsElem)elem, stBuilder);
            }
            else if (elem is EsArr)
            {
                WriteJson((EsArr)elem, stBuilder);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        //-----------------------------------------------------------------------
        public static void WriteXml(this EsDoc doc, StringBuilder stbuiolder)
        {
            throw new NotSupportedException();
        }
    }
}