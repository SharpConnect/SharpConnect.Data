﻿//MIT, 2015-2019, brezza92, EngineKit and contributors
using System.Collections.Generic;
namespace SharpConnect.Data
{
    public interface EsElem
    {
        string Name { get; set; }
        //EsDoc OwnerDocument { get; }
        //bool HasOwnerDocument { get; }
        //int NameIndex { get; }
        IEnumerable<EsAttr> GetAttributeIterForward();
        void RemoveAttribute(string key);
        void AppendChild(EsElem element);
        //void AppendAttribute(EsAttr attr);
        void AppendAttribute(string key, object value);
        object GetAttributeValue(string key);
        //EsAttr GetAttribute(string key);
        int ChildCount { get; }
        object GetChild(int index);
        //object this[string attrName] { get; set; }
    }
    public interface EsAttr
    {
        string Name { get; }
        object Value { get; }
        //int AttributeLocalNameIndex { get; }
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

        EsElem CreateElement(string elementName);
        EsElem CreateElement();
        EsArr CreateArray();
        EsElem DocumentElement { get; set; }
    }
}