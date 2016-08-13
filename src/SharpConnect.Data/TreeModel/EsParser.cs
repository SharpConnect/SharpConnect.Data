//MIT, 2015-2016, brezza92, EngineKit and contributors
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpConnect.Data
{

    /// <summary>
    /// event-driven json-like parser 
    /// </summary>
    abstract class EsParserBase
    {
        enum EsElementKind
        {
            Unknown,
            Object,
            Array
        }
        enum ParsingState
        {
            _0_Init,
            _1_ObjectKey,
            _2_CollectStringLiteral,
            _3_StringEscape,
            _4_FinishKeyPartWaitForSemiColon,
            _5_ExpectObjectValueOrArrayElement,
            _6_AfterObjectValueOrArrayElement,
            _7_CollectNumberLiteral,
            _8_CollectIdentifier,
        }
        enum NumberPart
        {
            IntegerPart,
            FractionPart,
            E, //e or E
            ESign,//e and sign
            ExponentialPart,
        }
        protected enum ValueHint
        {
            Unknown,
            StringLiteral,
            IntegerNumber,
            NumberWithFractionPart,
            NumberWithExponentialPart,
            NumberWithSignedExponentialPart,
            Identifier,
            Comment,//extension
        }
#if DEBUG
        public static bool dbug_EnableLogParser = false;
        public static int dbug_file_count = 0;
#endif

#if DEBUG
        static EsParserBase()
        {
            if (dbug_EnableLogParser)
            {
                dbugEsParserLogger.Init("d:\\WImageTest\\parse_json.txt");
            }
        }
#endif 
        protected virtual void BeginObject()
        {
            //create new js object 
            //and set this to current object
        }
        protected virtual void EndObject()
        {
            //close current jsobject
        }
        protected virtual void BeginArray()
        {

        }
        protected virtual void EndArray()
        {

        }
        protected virtual void NewKey(StringBuilder tmpBuffer, ValueHint valueHint)
        {

        }
        protected virtual void NewValue(StringBuilder tmpBuffer, ValueHint valueHint)
        {

        }
        protected virtual void OnError(ref int currentIndex)
        {

        }
        protected virtual void OnParseEnd()
        {

        }
        protected virtual void OnParseStart()
        {

        }
        protected virtual void NotifyError()
        {
        }
        public virtual void Parse(char[] sourceBuffer)
        {
            OnParseStart();
            //--------------------------------------------------------------
            EsElementKind currentElementKind = EsElementKind.Unknown;
            Stack<EsElementKind> elemKindStack = new Stack<EsElementKind>();
            //--------------------------------------------------------------

            StringBuilder myBuffer = new StringBuilder();
            //string lastestKey = "";
            ParsingState currentState = ParsingState._0_Init;
            int j = sourceBuffer.Length;
            bool isSuccess = true;
            bool isInKeyPart = false;
            NumberPart numberPart = NumberPart.IntegerPart;

            //WARNING: custom version, about ending with comma
            //we may use implicit comma feature, 
            //in case we start new line but forget a comma,
            //we auto add comma 

            //bool implicitComma = false;
            char openStringWithChar = '"';
            int i = 0;
            ValueHint currentValueHint = ValueHint.Unknown;
            for (i = 0; i < j; i++)
            {
                if (!isSuccess)
                {

                    OnError(ref i);
                    //handle the error ****
                    //#if DEBUG
                    // if (dbug_EnableLogParser)
                    // {
                    //   dbugDataFormatParser.IndentLevel = myKeyStack.Count;
                    //   dbugDataFormatParser.WriteLine("fail at pos=" + i + " on " + currentState);
                    // }
                    //#endif
                    break; //break from loop
                }

                //--------------------------
                char c = sourceBuffer[i];
#if DEBUG
                if (dbug_EnableLogParser)
                {
                    dbugEsParserLogger.WriteLine(new string('\t', elemKindStack.Count) + i + " ," + c.ToString() + "," + currentState);
                }
#endif
                //--------------------------  
                switch (currentState)
                {
                    case ParsingState._0_Init:
                        {
                            switch (c)
                            {
                                case '{':


                                    BeginObject();
                                    //change current element kind after notification
                                    elemKindStack.Push(currentElementKind);
                                    currentElementKind = EsElementKind.Object;

                                    myBuffer.Length = 0;//clear
                                    isInKeyPart = true;
                                    currentState = ParsingState._1_ObjectKey;
                                    break;
                                default:
                                    {
                                        if (char.IsWhiteSpace(c))
                                        {
                                            //same state
                                            continue;
                                        }
                                        else
                                        {
                                            isSuccess = false;
                                            NotifyError();
                                        }
                                    }
                                    break;
                            }
                        }
                        break;
                    case ParsingState._1_ObjectKey:
                        {
                            //TODO: review here again
                            //in json spec, not support '\"' in keypart

                            if (c == '"' || c == '\'')
                            {
                                //**                                
                                openStringWithChar = c;
                                currentValueHint = ValueHint.StringLiteral;
                                currentState = ParsingState._2_CollectStringLiteral;
                            }
                            else if (char.IsWhiteSpace(c))
                            {
                                continue;
                            }
                            else if (c == '}')
                            {
                                //this is empty 
                                if (currentElementKind != EsElementKind.Object)
                                {
                                    NotifyError();
                                    isSuccess = false;
                                }
                                else
                                {
                                    if (myBuffer.Length > 0)
                                    {
                                        //error at this state
                                    }
                                    isInKeyPart = false;
                                    EndObject();//end current object 
                                    //1. close current object
                                    //2. pop current object and  switch back 
                                    //to prev state ***
                                    if (elemKindStack.Count > 0)
                                    {
                                        //switch back      
                                        currentElementKind = elemKindStack.Pop();
                                    }
                                    currentState = ParsingState._6_AfterObjectValueOrArrayElement;
                                }
                            }
                            else if (char.IsLetter(c) || c == '_')
                            {
                                currentValueHint = ValueHint.Identifier;
                                myBuffer.Append(c); //collect identifier
                                currentState = ParsingState._8_CollectIdentifier; //collect identifier
                            }
                            else
                            {
                                //extension***
                                //

                                //number or other token will error in keypart***
                                NotifyError();
                                isSuccess = false;
                                break;
                            }
                        }
                        break;
                    case ParsingState._2_CollectStringLiteral:
                        {
                            //collecting string 
                            if (c == '\\')
                            {
                                currentState = ParsingState._3_StringEscape;
                            }
                            else if (c == openStringWithChar)
                            {
                                //close current string collection
                                currentValueHint = ValueHint.StringLiteral;

                                if (isInKeyPart)
                                {
                                    NewKey(myBuffer, currentValueHint);
                                    myBuffer.Length = 0;//clear
                                    currentState = ParsingState._4_FinishKeyPartWaitForSemiColon;
                                }
                                else
                                {
                                    NewValue(myBuffer, currentValueHint);
                                    myBuffer.Length = 0;//clear
                                    currentState = ParsingState._6_AfterObjectValueOrArrayElement;

                                }
                            }
                            else
                            {
                                myBuffer.Append(c);
                            }
                        }
                        break;
                    case ParsingState._3_StringEscape:
                        {
                            switch (c)
                            {
                                case '"':
                                    {
                                        myBuffer.Append('\"');
                                    }
                                    break;
                                case '\'':
                                    {
                                        myBuffer.Append('\'');
                                    }
                                    break;
                                case '/':
                                    {
                                        myBuffer.Append('/');
                                    }
                                    break;
                                case '\\':
                                    {
                                        myBuffer.Append('\\');
                                    }
                                    break;
                                case 'b':
                                    {
                                        myBuffer.Append('\b');
                                    }
                                    break;
                                case 'f':
                                    {
                                        myBuffer.Append('\f');
                                    }
                                    break;
                                case 'r':
                                    {
                                        myBuffer.Append('\r');
                                    }
                                    break;
                                case 'n':
                                    {
                                        myBuffer.Append('\n');
                                    }
                                    break;
                                case 't':
                                    {
                                        myBuffer.Append('\t');
                                    }
                                    break;
                                case 'u':
                                    {
                                        //unicode char in hexa digit
                                        //TODO: review here if we have enough char to parse ***
                                        if (i < j - 4)
                                        {
                                            //json spec
                                            //this follow by  4 chars
                                            //for extension we check if it match with 4 chars or not 
                                            uint c_uint = ParseUnicode(
                                             sourceBuffer[i + 1],
                                             sourceBuffer[i + 2],
                                             sourceBuffer[i + 3],
                                             sourceBuffer[i + 4]);
                                            myBuffer.Append((char)c_uint);
                                            i += 4;
                                        }
                                        else
                                        {
                                            //error
                                            isSuccess = false;
                                            NotifyError();
                                        }
                                    }
                                    break;
                                default:
                                    {
                                        NotifyError();
                                        isSuccess = false;
                                    }
                                    break;
                            }
                            //switch back to state 2_collectStringLiteral
                            currentState = ParsingState._2_CollectStringLiteral;
                        }
                        break;
                    case ParsingState._4_FinishKeyPartWaitForSemiColon:
                        {
                            //wait for :
                            if (c == ':')
                            {
                                myBuffer.Length = 0;//clear
                                isInKeyPart = false;
                                currentState = ParsingState._5_ExpectObjectValueOrArrayElement; //object's value part                                
                            }
                            else if (char.IsWhiteSpace(c))
                            {
                                continue;
                            }
                            else
                            {
                                //TODO: add recovery extension here
                                NotifyError();
                                isSuccess = false;
                                break;
                            }
                        }
                        break;
                    case ParsingState._5_ExpectObjectValueOrArrayElement:
                        {
                            //in value part *** 
                            //of object or array 

                            if (c == '"' || c == '\'')
                            {
                                //TODO: string escape here
                                openStringWithChar = c;
                                //string val
                                currentState = ParsingState._2_CollectStringLiteral;
                            }
                            else if (char.IsDigit(c) || (c == '-'))
                            {
                                //TODO:
                                //support extension + 
                                myBuffer.Append(c);
                                //number
                                currentValueHint = ValueHint.IntegerNumber;
                                numberPart = NumberPart.IntegerPart;
                                currentState = ParsingState._7_CollectNumberLiteral;
                            }
                            else if (c == '{')
                            {
                                //store current object in stack
                                BeginObject();
                                elemKindStack.Push(currentElementKind);
                                currentElementKind = EsElementKind.Object;
                                isInKeyPart = true;
                                currentState = ParsingState._1_ObjectKey;
                            }
                            else if (c == '[')
                            {
                                BeginArray();
                                elemKindStack.Push(currentElementKind);
                                currentElementKind = EsElementKind.Array;
                                isInKeyPart = false;
                                currentState = ParsingState._5_ExpectObjectValueOrArrayElement; //on the same state -- value state
                            }
                            else if (c == ']')
                            {
                                if (currentElementKind != EsElementKind.Array)
                                {
                                    NotifyError();
                                    isSuccess = false;
                                }
                                else
                                {
                                    EndArray();//end current array

                                    if (elemKindStack.Count > 0)
                                    {

                                        currentElementKind = elemKindStack.Pop();
                                    }
                                }
                                currentState = ParsingState._6_AfterObjectValueOrArrayElement;
                            }
                            else if (char.IsWhiteSpace(c))
                            {
                                continue;
                            }
                            else
                            {
                                //we collect other character into buffer
                                //so we can collect 
                                //null, true, false
                                //or other identifier  ***
                                currentState = ParsingState._8_CollectIdentifier;
                                myBuffer.Append(c);
                            }
                        }
                        break;
                    case ParsingState._6_AfterObjectValueOrArrayElement:
                        {
                            switch (c)
                            {
                                case ',':
                                    switch (currentElementKind)
                                    {
                                        default: throw new NotSupportedException();
                                        case EsElementKind.Object:
                                            currentState = ParsingState._1_ObjectKey;
                                            isInKeyPart = true;
                                            break;
                                        case EsElementKind.Array:
                                            //array
                                            currentState = ParsingState._5_ExpectObjectValueOrArrayElement;
                                            break;
                                    }
                                    break;
                                case ']':
                                    if (currentElementKind != EsElementKind.Array)
                                    {
                                        //error
                                        throw new NotSupportedException();
                                    }
                                    EndArray();

                                    //close current array
                                    //then push value back to prev stored value
                                    if (elemKindStack.Count > 0)
                                    {

                                        //current value must be array
                                        currentElementKind = elemKindStack.Pop();
                                    }
                                    break;
                                case '}':

                                    if (currentElementKind != EsElementKind.Object)
                                    {
                                        //error
                                        throw new NotSupportedException();
                                    }
                                    EndObject();

                                    if (elemKindStack.Count > 0)
                                    {
                                        currentElementKind = elemKindStack.Pop();
                                    }
                                    currentState = ParsingState._6_AfterObjectValueOrArrayElement;
                                    break;
                                default:
                                    //?
                                    //TODO: error recovery / or handle error with some extension 
                                    break;
                            }

                            //else if (c == '\r' || c == '\n')
                            //{
                            //    //WARNING: review implicit comma
                            //    //check if we enable this option  or not
                            //    //if not -> this will error
                            //    implicitComma = true;
                            //}
                            //else
                            //{
                            //    //WARNING: review implicit comma
                            //    if (char.IsLetter(c) || c == '_' || c == '"')
                            //    {
                            //        if (implicitComma)
                            //        {
                            //            if (currentElementKind == EsElementKind.Object)
                            //            {
                            //                currentState = ParsingState._1_ObjectKey;
                            //                isInKeyPart = true;

                            //            }
                            //            else
                            //            {
                            //                currentState = ParsingState._5_ExpectObjectValueOrArrayElement;
                            //            }
                            //            i--;
                            //            implicitComma = false;
                            //        }
                            //        else
                            //        {
                            //            //?
                            //        }
                            //    }
                            //    else
                            //    {
                            //        //eg. whitespace
                            //        //?
                            //    }
                            //}
                        }
                        break;
                    case ParsingState._7_CollectNumberLiteral:
                        {
                            //------------------------------------------------------
                            //TODO: review pass sign state of number literal
                            //check if we support hex liternal or binary literal
                            //this is extension to normal json ***
                            //------------------------------------------------------ 

                            if (char.IsDigit(c))
                            {
                                myBuffer.Append(c);
                            }
                            else if (c == '.')
                            {
                                if (numberPart == NumberPart.IntegerPart)
                                {
                                    myBuffer.Append(c);
                                    numberPart = NumberPart.FractionPart;
                                    currentValueHint = ValueHint.NumberWithFractionPart;
                                }
                                else
                                {
                                    NotifyError();
                                    isSuccess = false;
                                    break;
                                }
                            }
                            else if (c == 'e' || c == 'E')
                            {
                                myBuffer.Append(c);
                                switch (numberPart)
                                {
                                    case NumberPart.IntegerPart:
                                        numberPart = NumberPart.E;
                                        currentValueHint = ValueHint.NumberWithExponentialPart;
                                        break;
                                    case NumberPart.FractionPart:
                                        numberPart = NumberPart.E;
                                        currentValueHint = ValueHint.NumberWithExponentialPart;
                                        break;
                                    default:
                                        NotifyError();
                                        isSuccess = false;
                                        break;
                                }
                            }
                            else if (c == '-' || c == '+')
                            {
                                myBuffer.Append(c);
                                switch (numberPart)
                                {
                                    case NumberPart.E://after e
                                        numberPart = NumberPart.ESign;
                                        break;
                                    default:
                                        NotifyError();
                                        isSuccess = false;
                                        break;
                                }
                            }
                            else if (c == ']')
                            {
                                NewValue(myBuffer, currentValueHint);
                                //--------------------------
                                myBuffer.Length = 0; //clear 
                                EndArray();

                                if (elemKindStack.Count > 0)
                                {

                                    currentElementKind = elemKindStack.Pop();
                                }
                                currentState = ParsingState._6_AfterObjectValueOrArrayElement;
                            }
                            else if (c == '}')
                            {
                                NewValue(myBuffer, currentValueHint);
                                myBuffer.Length = 0; //clear
                                EndObject();

                                if (elemKindStack.Count > 0)
                                {
                                    currentElementKind = elemKindStack.Pop();
                                }
                                currentState = ParsingState._6_AfterObjectValueOrArrayElement;
                            }
                            else if (c == ',')
                            {
                                NewValue(myBuffer, currentValueHint);
                                //clear
                                myBuffer.Length = 0;
                                switch (currentElementKind)
                                {
                                    default: throw new NotSupportedException();
                                    case EsElementKind.Array:
                                        isInKeyPart = false;
                                        currentState = ParsingState._6_AfterObjectValueOrArrayElement;
                                        break;
                                    case EsElementKind.Object:
                                        isInKeyPart = true;
                                        currentState = ParsingState._1_ObjectKey;
                                        break;
                                }
                            }
                            else
                            {

                                isSuccess = false;
                                NotifyError();
                            }
                        }
                        break;
                    case ParsingState._8_CollectIdentifier:
                        {

                            currentValueHint = ValueHint.Identifier;
                            switch (c)
                            {
                                case ':':
                                    //stop collect identifier and  
                                    if (isInKeyPart)
                                    {
                                        NewKey(myBuffer, currentValueHint);
                                        myBuffer.Length = 0;//clear
                                        currentState = ParsingState._5_ExpectObjectValueOrArrayElement; //object's value part
                                        isInKeyPart = false;
                                    }
                                    else
                                    {
                                        NotifyError();
                                        isSuccess = false;
                                    }
                                    break;
                                case '}':
                                    if (isInKeyPart)
                                    {
                                        NotifyError();
                                        isSuccess = false;
                                    }
                                    else
                                    {
                                        NewValue(myBuffer, currentValueHint);
                                        myBuffer.Length = 0; //clear 
                                        EndObject();

                                        if (elemKindStack.Count > 0)
                                        {
                                            currentElementKind = elemKindStack.Pop();
                                        }
                                        currentState = ParsingState._6_AfterObjectValueOrArrayElement;
                                    }
                                    break;
                                case ']':
                                    if (isInKeyPart)
                                    {
                                        NotifyError();
                                        isSuccess = false;
                                    }
                                    else
                                    {
                                        NewValue(myBuffer, currentValueHint);
                                        myBuffer.Length = 0; //clear 
                                        EndArray();

                                        if (elemKindStack.Count > 0)
                                        {

                                            currentElementKind = elemKindStack.Pop();
                                        }
                                        currentState = ParsingState._6_AfterObjectValueOrArrayElement;
                                    }
                                    break;
                                case ',':
                                    if (isInKeyPart)
                                    {
                                        NotifyError();
                                        isSuccess = false;
                                    }
                                    else
                                    {
                                        NewValue(myBuffer, currentValueHint);
                                        myBuffer.Length = 0; //clear 
                                        switch (currentElementKind)
                                        {
                                            default: throw new NotSupportedException();
                                            case EsElementKind.Array:
                                                isInKeyPart = false;
                                                currentState = ParsingState._6_AfterObjectValueOrArrayElement;
                                                break;
                                            case EsElementKind.Object:
                                                isInKeyPart = true;
                                                currentState = ParsingState._1_ObjectKey;
                                                break;
                                        }
                                    }
                                    break;
                                default:

                                    if (char.IsWhiteSpace(c))
                                    {
                                        //stop collect identifier***
                                        //wait for :                                
                                        currentState = ParsingState._4_FinishKeyPartWaitForSemiColon;
                                    }
                                    else
                                    {
                                        myBuffer.Append(c);
                                    }
                                    break;
                            }
                        }
                        break;
                }
            }

            OnParseEnd();

        }
        static uint ParseSingleChar(char c1, uint multipliyer)
        {
            if (c1 >= '0' && c1 <= '9')
                return ((uint)(c1 - '0') * multipliyer);
            else if (c1 >= 'A' && c1 <= 'F')
                return ((uint)((c1 - 'A') + 10) * multipliyer);
            else if (c1 >= 'a' && c1 <= 'f')
                return ((uint)((c1 - 'a') + 10) * multipliyer);
            else
                return 0;
        }
        static uint ParseUnicode(char c1, char c2, char c3, char c4)
        {
            uint p1 = ParseSingleChar(c1, 0x1000);
            uint p2 = ParseSingleChar(c2, 0x100);
            uint p3 = ParseSingleChar(c3, 0x10);
            uint p4 = ParseSingleChar(c4, 1);
            return p1 + p2 + p3 + p4;
        }
    }
}