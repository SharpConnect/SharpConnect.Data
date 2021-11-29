//MIT, 2015-present, brezza92, EngineKit and contributors

namespace SharpConnect.Data
{
    class EaseDocParser : EsParserBase<EaseElement, EaseArray>
    {
        EaseDocument _easeDoc;

        int _ext_n_keyIndex;
        int _ext_c_keyIndex;
        public EaseDocParser(EaseDocument blankdoc)
        {
            _easeDoc = blankdoc;
            _ext_n_keyIndex = RegisterKey("!n");
            _ext_c_keyIndex = RegisterKey("!c");
        }
        public bool EnableExtension { get; set; }
        protected override EaseElement CreateElement() => _easeDoc.CreateElement();

        protected override EaseArray CreateArray() => _easeDoc.CreateArray();

      
        protected override void AddElementAttribute(EaseElement targetElem, int key, EaseArray value)
        {
            if (EnableExtension)
            {
                if (key == _ext_c_keyIndex)
                {
                    //name
                    int j = value.Count;
                    for (int i = 0; i < j; ++i)
                    {
                        targetElem.AppendChild(value[i]);
                    }
                    return;
                }
            }

            targetElem.AppendAttribute(GetKeyAsStringByIndex(key), value);
        }
        protected override void AddElementAttribute(EaseElement targetElem, int key, EaseElement value)
        {
            targetElem.AppendAttribute(GetKeyAsStringByIndex(key), value);
        }
        protected override void AddElementAttribute(EaseElement targetElem, int key, EsValueHint valueHint)
        {
            //read key and its value
            if (EnableExtension)
            {
                if (key == _ext_n_keyIndex && valueHint == EsValueHint.StringLiteral)
                {
                    //name
                    ((EaseElement)targetElem).Name = GetValueAsString();
                    return;
                }
            }

            switch (valueHint)
            {
                default:
                    {

                    }
                    break;
                case EsValueHint.Null:
                    targetElem.AppendAttribute(GetKeyAsStringByIndex(key), null);
                    break;
                case EsValueHint.True:
                    targetElem.AppendAttribute(GetKeyAsStringByIndex(key), true);
                    break;
                case EsValueHint.False:
                    {
                        targetElem.AppendAttribute(GetKeyAsStringByIndex(key), false);
                    }
                    break;
                case EsValueHint.StringLiteral:
                    {
                        string str = GetValueAsString();
                        targetElem.AppendAttribute(GetKeyAsStringByIndex(key), str);
                    }
                    break;
                case EsValueHint.StringLiteralWithSomeEscape:
                    {
                        string str = GetValueAsStringWithEscape();
                        targetElem.AppendAttribute(GetKeyAsStringByIndex(key), str);
                    }
                    break;
                case EsValueHint.NegativeIntegerNumber:
                    {
                        if (ConcatValueLen > 9)
                        {
                            long value = GetValueAsInt64();
                            targetElem.AppendAttribute(GetKeyAsStringByIndex(key), value);
                        }
                        else
                        {
                            int value = GetValueAsInt32();
                            targetElem.AppendAttribute(GetKeyAsStringByIndex(key), value);
                        }

                    }
                    break;
                case EsValueHint.IntegerNumber:
                    {
                        //int64
                        if (ConcatValueLen > 9)
                        {
                            long value = GetValueAsInt64();
                            targetElem.AppendAttribute(GetKeyAsStringByIndex(key), value);
                        }
                        else
                        {
                            int value = GetValueAsInt32();
                            targetElem.AppendAttribute(GetKeyAsStringByIndex(key), value);
                        }
                    }
                    break;
                case EsValueHint.NumberWithExponentialPart:
                    {
                        double num = GetValueAsDouble();
                        targetElem.AppendAttribute(GetKeyAsStringByIndex(key), num);
                    }
                    break;
                case EsValueHint.NumberWithFractionPart:
                    {
                        double num = GetValueAsDouble();
                        targetElem.AppendAttribute(GetKeyAsStringByIndex(key), num);
                    }
                    break;
            }
        }

        protected override void AddArrayElement(EaseArray targetArray, EaseArray value)
        {
            targetArray.AddItem(value);
        }
        protected override void AddArrayElement(EaseArray targetArray, EaseElement value)
        {
            targetArray.AddItem(value);
        }

        protected override void AddArrayElement(EaseArray targetArray, EsValueHint valueHint)
        {
            switch (valueHint)
            {
                default:
                    {

                    }
                    break;
                case EsValueHint.Null:
                    targetArray.AddItem(null);
                    break;
                case EsValueHint.True:
                    targetArray.AddItem(true);
                    break;
                case EsValueHint.False:
                    targetArray.AddItem(false);
                    break;
                case EsValueHint.StringLiteral:
                    {
                        string str = GetValueAsString();
                        targetArray.AddItem(str);
                    }
                    break;
                case EsValueHint.StringLiteralWithSomeEscape:
                    {
                        string str = GetValueAsString();
                        targetArray.AddItem(str);
                    }
                    break;
                case EsValueHint.NegativeIntegerNumber:
                    {
                        if (ConcatValueLen > 9)
                        {
                            long value = GetValueAsInt64();
                            targetArray.AddItem(value);
                        }
                        else
                        {
                            int value = GetValueAsInt32();
                            targetArray.AddItem(value);
                        }

                    }
                    break;
                case EsValueHint.IntegerNumber:
                    {
                        //int64
                        if (ConcatValueLen > 9)
                        {
                            long value = GetValueAsInt64();
                            targetArray.AddItem(value);
                        }
                        else
                        {
                            int value = GetValueAsInt32();
                            targetArray.AddItem(value);
                        }
                    }
                    break;
                case EsValueHint.NumberWithExponentialPart:
                    {
                        double num = GetValueAsDouble();
                        targetArray.AddItem(num);
                    }
                    break;
                case EsValueHint.NumberWithFractionPart:
                    {
                        double num = GetValueAsDouble();
                        targetArray.AddItem(num);
                    }
                    break;
            }
        }

    }

}

