//MIT, 2015-present, brezza92, EngineKit and contributors
using System.Collections.Generic;
namespace SharpConnect.Data
{
    public interface EsElem
    {
        string Name { get; set; }
        IEnumerable<EsAttr> GetAttributeIterForward();
        void RemoveAttribute(string key);
        void AppendChild(EsElem element);
        void AppendAttribute(string key, object value);
        object GetAttributeValue(string key);
        int ChildCount { get; }
        object GetChild(int index);
    }
    public interface EsAttr
    {
        string Name { get; }
        object Value { get; }
    }
    public interface EsArr
    {
        void AddItem(object item);
        IEnumerable<object> GetIter();
        void Clear();
        int Count { get; }
        object this[int index] { get; set; }
    } 

    public interface EsDoc
    {
        EsElem CreateElement();
        EsArr CreateArray();
        EsElem DocumentElement { get; set; }
    }
}