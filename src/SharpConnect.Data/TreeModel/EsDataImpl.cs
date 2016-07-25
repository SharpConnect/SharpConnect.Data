//MIT, 2015-2016, brezza92, EngineKit and contributors

using System;
using System.Collections.Generic;
using System.Text;
namespace SharpConnect.Data
{
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
        public static string GetAttributeValueAsString(this EsElem lqElement, string attrName)
        {
            return lqElement.GetAttributeValue(attrName) as string;
        }
        public static int GetAttributeValueAsInt32(this EsElem lqElement, string attrName)
        {
            return (int)lqElement.GetAttributeValue(attrName);
        }
        public static bool GetAttributeValueAsBool(this EsElem lqElement, string attrName)
        {
            return (bool)lqElement.GetAttributeValue(attrName);
        }
        public static EsArr GetAttributeValueAsArray(this EsElem lqElement, string attrName)
        {
            return lqElement.GetAttributeValue(attrName) as EsArr;
        }
        //-----------------------------------------------------------------------
        public static void WriteJson(this EsDoc lqdoc, StringBuilder stBuilder)
        {
            //write to 
            var docElem = lqdoc.DocumentElement;
            if (docElem != null)
            {
                WriteJson(docElem, stBuilder);
            }
        }
        static void WriteJson(object lqElem, StringBuilder stBuilder)
        {
            //recursive
            if (lqElem == null)
            {
                stBuilder.Append("null");
            }
            else if (lqElem is string)
            {
                stBuilder.Append('"');
                stBuilder.Append((string)lqElem);
                stBuilder.Append('"');
            }
            else if (lqElem is double)
            {
                stBuilder.Append(((double)lqElem).ToString());
            }
            else if (lqElem is float)
            {
                stBuilder.Append(((float)lqElem).ToString());
            }
            else if (lqElem is int)
            {
                stBuilder.Append(((int)lqElem).ToString());
            }
            else if (lqElem is Array)
            {
                stBuilder.Append('[');
                //write element into array
                Array a = lqElem as Array;
                int j = a.Length;
                for (int i = 0; i < j; ++i)
                {
                    WriteJson(a.GetValue(i), stBuilder);
                    if (i > 0)
                    {
                        stBuilder.Append(',');
                    }
                }
                stBuilder.Append(']');
            }
            else if (lqElem is EaseElement)
            {
                EaseElement leqE = (EaseElement)lqElem;
                stBuilder.Append('{');
                //check docattr= 
                var nameAttr = leqE.GetAttribute("!n");
                if (nameAttr == null)
                {
                    //use specific name
                    stBuilder.Append("\"!n\":\"");
                    stBuilder.Append(leqE.Name);
                    stBuilder.Append('"');
                }
                else
                {
                    //use default elementname
                    stBuilder.Append("\"!n\":\"");
                    stBuilder.Append(leqE.Name);
                    stBuilder.Append('"');
                }

                int attrCount = 1;
                foreach (var attr in leqE.GetAttributeIterForward())
                {
                    if (attr.Name == "!n")
                    {
                        continue;
                    }
                    stBuilder.Append(',');
                    stBuilder.Append('"');
                    stBuilder.Append(attr.Name);
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
                    stBuilder.Append(',');
                    stBuilder.Append("\"!c\":[");
                    for (int i = 0; i < j; ++i)
                    {
                        WriteJson(leqE.GetChild(i), stBuilder);
                        if (i < j - 1)
                        {
                            stBuilder.Append(',');
                        }
                    }
                    stBuilder.Append(']');
                }
                //-------------------
                stBuilder.Append('}');
            }
            else
            {
            }
        }
        //-----------------------------------------------------------------------
        public static void WriteXml(this EsDoc lqdoc, StringBuilder stbuiolder)
        {
            throw new NotSupportedException();
        }
    }
}