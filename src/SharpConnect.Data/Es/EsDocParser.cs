//MIT, 2015-2016, brezza92, EngineKit and contributors

using System;

namespace SharpConnect.Data
{
    class EaseDocParser : Es.EsParserBase<EsElem, EsArr>
    {
        EaseDocument easeDoc;
        public EaseDocParser(EaseDocument blankdoc)
        {
            easeDoc = blankdoc;
        }
        protected override EsElem createElement()
        {
            return easeDoc.CreateElement();
        }

        protected override EsArr createArray()
        {
            return easeDoc.CreateArray();
        }

        protected override void addElementAttribute(EsElem targetElem, string key, object value)
        {
            targetElem[key] = value;
        }

        protected override void addArrayElement(EsArr targetArray, object value)
        {
            targetArray.AddItem(value);
        } 
    }

}

