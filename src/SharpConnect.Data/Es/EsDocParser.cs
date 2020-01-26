//MIT, 2015-2019, brezza92, EngineKit and contributors

namespace SharpConnect.Data
{
    class EaseDocParser : EsParserBase<EsElem, EsArr>
    {
        EaseDocument _easeDoc;
        public EaseDocParser(EaseDocument blankdoc)
        {
            _easeDoc = blankdoc;
        }

        protected override EsElem CreateElement() => _easeDoc.CreateElement();

        protected override EsArr CreateArray() => _easeDoc.CreateArray();

        protected override void AddElementAttribute(EsElem targetElem, string key, object value)
        {
            targetElem.AppendAttribute(key, value);             
        }

        protected override void AddArrayElement(EsArr targetArray, object value)
        {
            targetArray.AddItem(value);
        }
    }

}

