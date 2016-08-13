//MIT, 2015-2016, brezza92, EngineKit and contributors

using System;
using System.Collections.Generic;
using System.Text;
namespace SharpConnect.Data
{
    class EaseDocParser : EsParserBase
    {
        enum CurrentObject
        {
            Object,
            Array
        }

        EaseDocument easeDoc;
        Stack<string> keyStack = new Stack<string>();
        Stack<object> elemStack = new Stack<object>();
        object currentElem = null;
        string currentKey = null;
        public EaseDocParser(EaseDocument blankdoc)
        {
            easeDoc = blankdoc;
        }
        protected override void OnParseStart()
        {
            if (easeDoc == null)
            {
                easeDoc = new EaseDocument();
            }
        }
        protected override void BeginObject()
        {
            if (currentKey != null)
            {
                keyStack.Push(currentKey);
            }
            if (currentElem != null)
            {
                elemStack.Push(currentElem);
            }
            currentElem = easeDoc.CreateElement();
        }
        protected override void EndObject()
        {
            //current element should be object
            object c_object = currentElem;
            if (elemStack.Count > 0)
            {
                //pop from stack
                currentElem = elemStack.Pop();
                currentKey = null;
                if (c_object == currentElem)
                {
                    throw new System.Exception();
                }
                if (currentElem is EsElem)
                {
                    currentKey = keyStack.Pop();
                    ((EsElem)currentElem)[currentKey] = c_object;
                }
                else if (currentElem is EsArr)
                {
                    ((EsArr)currentElem).AddItem(c_object);
                }
                else
                {
                    throw new System.NotSupportedException();
                }
            }
        }
        protected override void BeginArray()
        {
            if (currentKey != null)
            {
                keyStack.Push(currentKey);
            }
            if (currentElem != null)
            {
                elemStack.Push(currentElem);
            }
            currentElem = easeDoc.CreateArray();
        }
        protected override void EndArray()
        {
            object c_object = currentElem;
            if (elemStack.Count > 0)
            {
                //pop from stack
                currentElem = elemStack.Pop();
                currentKey = null;

                if (c_object == currentElem)
                {
                    throw new System.Exception();
                }

                if (currentElem is EsElem)
                {
                    currentKey = keyStack.Pop();
                    ((EsElem)currentElem)[currentKey] = c_object;
                }
                else if (currentElem is EsArr)
                {
                    ((EsArr)currentElem).AddItem(c_object);
                }
                else
                {
                    throw new System.NotSupportedException();
                }
            }
        }
        protected override void OnParseEnd()
        {

        }
        protected override void NewKey(StringBuilder tmpBuffer, ValueHint valueHint)
        {
            //add key to current object
            //if (currentKey != null)
            //{
            //    keyStack.Push(currentKey);
            //}
            currentKey = tmpBuffer.ToString();

        }
        protected override void NewValue(StringBuilder tmpBuffer, ValueHint valueHint)
        {
            object c_object = null;
            switch (valueHint)
            {
                default:
                case ValueHint.Comment:
                    throw new System.NotSupportedException();
                case ValueHint.Identifier:
                    string iden = tmpBuffer.ToString();
                    switch (iden)
                    {
                        case "true":
                            c_object = true;
                            break;
                        case "false":
                            c_object = false;
                            break;
                        case "null":
                            c_object = null;
                            break;
                        default:
                            c_object = iden;
                            break;
                    }
                    break;
                case ValueHint.StringLiteral:
                    c_object = tmpBuffer.ToString();
                    break;
                case ValueHint.IntegerNumber:
                    c_object = int.Parse(tmpBuffer.ToString());
                    break;
                case ValueHint.NumberWithFractionPart:
                case ValueHint.NumberWithSignedExponentialPart:
                case ValueHint.NumberWithExponentialPart:
                    c_object = double.Parse(tmpBuffer.ToString());
                    break;

            }


            if (currentElem is EsElem)
            {
                ((EsElem)currentElem)[currentKey] = c_object;
            }
            else if (currentElem is EsArr)
            {
                ((EsArr)currentElem).AddItem(c_object);
            }
            else
            {
                throw new System.NotSupportedException();
            }
        }
        protected override void NotifyError()
        {
            base.NotifyError();
        }
        protected override void OnError(ref int currentIndex)
        {
            base.OnError(ref currentIndex);
        }
        public object CurrentElement { get { return currentElem; } }
    }


}

