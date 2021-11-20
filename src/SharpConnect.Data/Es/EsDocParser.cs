//MIT, 2015-present, brezza92, EngineKit and contributors

namespace SharpConnect.Data
{
    class EaseDocParser : EsParserBase<EsElem, EsArr>
    {
        EaseDocument _easeDoc;
        public EaseDocParser(EaseDocument blankdoc)
        {
            _easeDoc = blankdoc;
        }
        public bool EnableExtension { get; set; }
        protected override EsElem CreateElement() => _easeDoc.CreateElement();

        protected override EsArr CreateArray() => _easeDoc.CreateArray();

        protected override void AddElementAttribute(EsElem targetElem, string key, object value)
        {
            if (EnableExtension)
            {
                //our extension
                if (key == "!n" && value is string name)
                {
                    targetElem.Name = name;
                    return;
                }
                else if (key == "!c")
                {
                    if (value is EsArr arr)
                    {
                        //extract arr
                        int j = arr.Count;
                        for (int i = 0; i < j; ++i)
                        {
                            targetElem.AppendChild(arr[i]);
                        }
                    }
                    else
                    {
                        throw new System.NotSupportedException();
                        //targetElem.AppendChild(value);
                    }
                    return;
                }
            }
            targetElem.AppendAttribute(key, value);
        }

        protected override void AddArrayElement(EsArr targetArray, object value)
        {
            targetArray.AddItem(value);
        }
    }

}

