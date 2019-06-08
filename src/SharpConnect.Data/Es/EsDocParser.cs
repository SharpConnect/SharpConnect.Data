//MIT, 2015-2016, brezza92, EngineKit and contributors

using System;

namespace SharpConnect.Data
{
    class EaseDocParser : EsParserBase<EsElem, EsArr>
    {
        EaseDocument easeDoc;
        public EaseDocParser(EaseDocument blankdoc)
        {
            easeDoc = blankdoc;
        }
        protected override EsElem CreateElement()
        {
            return easeDoc.CreateElement();
        }

        protected override EsArr CreateArray()
        {
            return easeDoc.CreateArray();
        }

        protected override void AddElementAttribute(EsElem targetElem, string key, object value)
        {
            targetElem[key] = value;
        }

        protected override void AddArrayElement(EsArr targetArray, object value)
        {
            targetArray.AddItem(value);
        }
    }

}

