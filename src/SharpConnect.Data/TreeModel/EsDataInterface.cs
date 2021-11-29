//MIT, 2015-present, brezza92, EngineKit and contributors
using System.Collections.Generic;
namespace SharpConnect.Data
{


    /// <summary>
    /// ease element
    /// </summary>
    public interface EsElem
    {
        string Name { get; }
        IEnumerable<EsAttr> GetAttributeIterForward();
        object GetAttributeValue(string key);
        EsAttr GetAttribute(string key);
        EsAttr GetAttribute(int index);

        int ChildCount { get; }
        int AttributeCount { get; }
        object GetChild(int index);
        object UserData { get; set; } //TODO review this again       
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

        IEnumerable<object> GetIter();
        void Clear();
        int Count { get; }
        object this[int index] { get; }
    }

    /// <summary>
    /// ease doc
    /// </summary>
    public interface EsDoc
    {
        //EsElem CreateElement();
        //EsElem CreateElement(string name);
        //EsArr CreateArray();
        EsElem DocumentElement { get; set; }
    }

    public static class EsElemExtensions
    {
        public static IEnumerable<EsElem> GetChildNodeIter(this EsElem elem)
        {
            int n = elem.ChildCount;
            for (int i = 0; i < n; ++i)
            {
                if (elem.GetChild(i) is EsElem childElem)
                {
                    yield return childElem;
                }
            }
        }
    }
}