//MIT, 2015-present, brezza92, EngineKit and contributors
using System.Collections.Generic;
namespace SharpConnect.Data
{
    /// <summary>
    /// ease element
    /// </summary>
    public interface EsElem
    {
        string Name { get; set; }
        IEnumerable<EsAttr> GetAttributeIterForward();
        void RemoveAttribute(string key);
        void AppendChild(EsElem element);
        void AppendAttribute(string key, object value);
        object GetAttributeValue(string key);
        EsAttr GetAttribute(string key);
        EsAttr GetAttribute(int index);

        int ChildCount { get; }
        int AttributeCount { get; }
        object GetChild(int index);
        object UserData { get; set; }
    }
    /// <summary>
    /// ease attribute
    /// </summary>
    public interface EsAttr
    {
        string Name { get; }
        object Value { get; }
    }
    /// <summary>
    /// ease array
    /// </summary>
    public interface EsArr
    {
        void AddItem(object item);
        IEnumerable<object> GetIter();
        void Clear();
        int Count { get; }
        object this[int index] { get; set; }
    }

    /// <summary>
    /// ease doc
    /// </summary>
    public interface EsDoc
    {
        EsElem CreateElement();
        EsElem CreateElement(string name);
        EsArr CreateArray();
        EsElem DocumentElement { get; set; }
    }
}