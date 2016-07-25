//MIT 2015, brezza92, EngineKit and contributors

using System.Collections.Generic;
namespace SharpConnect.Data
{
    public interface EsElem
    {
        string Name { get; set; }
        EsDoc OwnerDocument { get; }
        bool HasOwnerDocument { get; }
        int NameIndex { get; }
        IEnumerable<EsAttr> GetAttributeIterForward();
        void RemoveAttribute(EsAttr attr);
        void AppendChild(EsElem element);
        void AppendAttribute(EsAttr attr);
        EsAttr AppendAttribute(string key, object value);
        object GetAttributeValue(string key);
        EsAttr GetAttributeElement(string key);
        int ChildCount { get; }
        object GetChild(int index);
    }
    public interface EsAttr
    {
        string Name { get; set; }
        object Value { get; set; }
        int AttributeLocalNameIndex { get; }
    }
    public interface EsArr
    {
        void AddItem(object item);
        IEnumerable<object> GetIterForward();
        void Clear();
        int Count { get; }
        object this[int index] { get; set; }
    }

    public class EsDoc
    {
        Dictionary<string, int> stringTable = new Dictionary<string, int>();
        public EsElem CreateElement(string elementName)
        {
            return new EaseElement(elementName, this);
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
    }
}