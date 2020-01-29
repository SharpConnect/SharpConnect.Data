//MIT, 2015-present, brezza92, EngineKit and contributors
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpConnect.Data
{

    /// <summary>
    /// event-driven json-like parser 
    /// </summary>
    public abstract class EsParserBase
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
                dbugEsParserLogger.Init("parse_json.txt");
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
        static void ReadSingleLineComment(char[] sourceBuffer, int startAt, ref int latestIndex)
        {
            latestIndex = startAt;
            for (; latestIndex < sourceBuffer.Length; ++latestIndex)
            {
                char c = sourceBuffer[latestIndex];
                if (c == '\r')
                {
                    if (latestIndex < sourceBuffer.Length - 1)
                    {
                        if (sourceBuffer[latestIndex + 1] == '\n')
                        {
                            //r,n
                            latestIndex++;
                            return;
                        }
                        else
                        {
                            //???
                            return;
                        }
                    }
                    else
                    {
                        //end
                        return;
                    }
                }
                else if (c == '\n')
                {
                    return;
                }
            }
        }
        static void ReadBlockComment(char[] sourceBuffer, int startAt, ref int latestIndex)
        {
            latestIndex = startAt;
            for (; latestIndex < sourceBuffer.Length; ++latestIndex)
            {
                char c = sourceBuffer[latestIndex];
                if (c == '*')
                {
                    if (latestIndex < sourceBuffer.Length - 1)
                    {
                        if (sourceBuffer[latestIndex + 1] == '/')
                        {
                            //r,n
                            latestIndex++;
                            return;
                        }
                        else
                        {
                            //read next
                        }
                    }
                    else
                    {
                        //end
                        return;
                    }
                }
            }
        }


        bool _isSuccess;
        bool IsSuccess
        {
            get => _isSuccess;
            set
            {
#if DEBUG
                if (!value)
                {

                }
#endif
                _isSuccess = value;
            }
        }



        public virtual void Parse(char[] sourceBuffer)
        {
            OnParseStart();
            //--------------------------------------------------------------
            EsElementKind currentElementKind = EsElementKind.Unknown;
            Stack<EsElementKind> elemKindStack = new Stack<EsElementKind>();
            //--------------------------------------------------------------
            IsSuccess = true;

            StringBuilder myBuffer = new StringBuilder();
            //string lastestKey = "";
            ParsingState currentState = ParsingState._0_Init;
            int j = sourceBuffer.Length;

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

                if (!IsSuccess)
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
                                case '/':
                                    {
                                        //-----------------------
                                        //comment syntax
                                        if (i < j - 1)
                                        {
                                            char next_c = sourceBuffer[i + 1];
                                            if (next_c == '/')
                                            {
                                                int latestIndex = i + 1;
                                                ReadSingleLineComment(sourceBuffer, latestIndex + 1, ref latestIndex);
                                                i = latestIndex;
                                            }
                                            else if (next_c == '*')
                                            {
                                                //inline comment
                                                int latestIndex = i + 1;
                                                ReadBlockComment(sourceBuffer, latestIndex + 1, ref latestIndex);
                                                i = latestIndex;
                                            }
                                            else
                                            {
                                                IsSuccess = false;
                                                NotifyError();
                                            }
                                        }
                                        //-----------------------
                                    }
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
                                            IsSuccess = false;
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
                                    IsSuccess = false;
                                }
                                else
                                {
#if DEBUG
                                    if (myBuffer.Length > 0)
                                    {
                                        //error at this state
                                    }
#endif
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
                            else if (c == '/')
                            {
                                //-----------------------
                                //comment syntax
                                if (i < j - 1)
                                {
                                    char next_c = sourceBuffer[i + 1];
                                    if (next_c == '/')
                                    {
                                        int latestIndex = i + 1;
                                        ReadSingleLineComment(sourceBuffer, latestIndex + 1, ref latestIndex);
                                        i = latestIndex;
                                    }
                                    else if (next_c == '*')
                                    {
                                        //inline comment
                                        int latestIndex = i + 1;
                                        ReadBlockComment(sourceBuffer, latestIndex + 1, ref latestIndex);
                                        i = latestIndex;
                                    }
                                    else
                                    {
                                        IsSuccess = false;
                                        NotifyError();
                                    }
                                }
                                //-----------------------
                            }
                            else
                            {
                                //extension***
                                //

                                //number or other token will error in keypart***
                                NotifyError();
                                IsSuccess = false;
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
                                            IsSuccess = false;
                                            NotifyError();
                                        }
                                    }
                                    break;
                                default:
                                    {
                                        NotifyError();
                                        IsSuccess = false;
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
                            else if (c == '/')
                            {
                                //-----------------------
                                //comment syntax
                                if (i < j - 1)
                                {
                                    char next_c = sourceBuffer[i + 1];
                                    if (next_c == '/')
                                    {
                                        int latestIndex = i + 1;
                                        ReadSingleLineComment(sourceBuffer, latestIndex + 1, ref latestIndex);
                                        i = latestIndex;
                                    }
                                    else if (next_c == '*')
                                    {
                                        //inline comment
                                        int latestIndex = i + 1;
                                        ReadBlockComment(sourceBuffer, latestIndex + 1, ref latestIndex);
                                        i = latestIndex;
                                    }
                                    else
                                    {
                                        IsSuccess = false;
                                        NotifyError();
                                    }
                                }
                                //-----------------------
                            }
                            else
                            {
                                //TODO: add recovery extension here
                                NotifyError();
                                IsSuccess = false;
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
                                    IsSuccess = false;
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
                            else if (c == '/')
                            {
                                //-----------------------
                                //comment syntax
                                if (i < j - 1)
                                {
                                    char next_c = sourceBuffer[i + 1];
                                    if (next_c == '/')
                                    {
                                        int latestIndex = i + 1;
                                        ReadSingleLineComment(sourceBuffer, latestIndex + 1, ref latestIndex);
                                        i = latestIndex;
                                    }
                                    else if (next_c == '*')
                                    {
                                        //inline comment
                                        int latestIndex = i + 1;
                                        ReadBlockComment(sourceBuffer, latestIndex + 1, ref latestIndex);
                                        i = latestIndex;
                                    }
                                    else
                                    {
                                        IsSuccess = false;
                                        NotifyError();
                                    }
                                }
                                //-----------------------
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
                                case '/':
                                    {
                                        //-----------------------
                                        //comment syntax
                                        if (i < j - 1)
                                        {
                                            char next_c = sourceBuffer[i + 1];
                                            if (next_c == '/')
                                            {
                                                int latestIndex = i + 1;
                                                ReadSingleLineComment(sourceBuffer, latestIndex + 1, ref latestIndex);
                                                i = latestIndex;
                                            }
                                            else if (next_c == '*')
                                            {
                                                //inline comment
                                                int latestIndex = i + 1;
                                                ReadBlockComment(sourceBuffer, latestIndex + 1, ref latestIndex);
                                                i = latestIndex;
                                            }
                                            else
                                            {
                                                IsSuccess = false;
                                                NotifyError();
                                            }
                                        }
                                        //-----------------------
                                    }
                                    break;
                                default:
                                    //?
                                    //TODO: error recovery / or handle error with some extension 
                                    break;
                            }
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
                                    IsSuccess = false;
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
                                        IsSuccess = false;
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
                                        IsSuccess = false;
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
                                        currentState = ParsingState._5_ExpectObjectValueOrArrayElement;
                                        break;
                                    case EsElementKind.Object:
                                        isInKeyPart = true;
                                        currentState = ParsingState._1_ObjectKey;
                                        break;
                                }
                            }
                            else if (c == '\r')
                            {
                                //stop here
                                if (i < j - 1)
                                {
                                    if (sourceBuffer[i + 1] == '\n')
                                    {
                                        //\r\n
                                        i++;

                                        NewValue(myBuffer, currentValueHint);
                                        //clear
                                        myBuffer.Length = 0;

                                        switch (currentElementKind)
                                        {
                                            default: throw new NotSupportedException();
                                            case EsElementKind.Array:
                                                isInKeyPart = false;
                                                currentState = ParsingState._5_ExpectObjectValueOrArrayElement;
                                                break;
                                            case EsElementKind.Object:
                                                isInKeyPart = true;
                                                currentState = ParsingState._1_ObjectKey;
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        //only R
                                    }
                                }
                            }
                            else if (c == '\n')
                            {
                                //stop 
                                NewValue(myBuffer, currentValueHint);
                                //clear
                                myBuffer.Length = 0;

                                switch (currentElementKind)
                                {
                                    default: throw new NotSupportedException();
                                    case EsElementKind.Array:
                                        isInKeyPart = false;
                                        currentState = ParsingState._5_ExpectObjectValueOrArrayElement;
                                        break;
                                    case EsElementKind.Object:
                                        isInKeyPart = true;
                                        currentState = ParsingState._1_ObjectKey;
                                        break;
                                }
                            }
                            else
                            {

                                IsSuccess = false;
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
                                        IsSuccess = false;
                                    }
                                    break;
                                case '}':
                                    if (isInKeyPart)
                                    {
                                        NotifyError();
                                        IsSuccess = false;
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
                                        IsSuccess = false;
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
                                        IsSuccess = false;
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
                                                currentState = ParsingState._5_ExpectObjectValueOrArrayElement;
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

        static uint ParseSingleChar(char c1)
        {
            switch (c1)
            {
                case '0': return 0;
                case '1': return 1;
                case '2': return 2;
                case '3': return 3;
                case '4': return 4;
                case '5': return 5;
                case '6': return 6;
                case '7': return 7;
                case '8': return 8;
                case '9': return 9;
                case 'A': return 10;
                case 'B': return 11;
                case 'C': return 12;
                case 'D': return 13;
                case 'E': return 14;
                case 'F': return 15;
                default: return 0;
            }
        }
        static uint ParseUnicode(char c1, char c2, char c3, char c4)
        {
            return (ParseSingleChar(c1) << 16) |
                    (ParseSingleChar(c2) << 8) |
                    (ParseSingleChar(c3) << 4) |
                    (ParseSingleChar(c4) << 0);
        }


    }



    public abstract class EsParserBase<E, A> : EsParserBase
        where E : class
        where A : class
    {
        enum CurrentObject
        {
            Object,
            Array
        }

        Stack<string> _keyStack = new Stack<string>();
        Stack<object> _elemStack = new Stack<object>();
        object _currentElem = null;
        string _currentKey = null;

        public EsParserBase()
        {

        }
        protected abstract E CreateElement();
        protected abstract A CreateArray();
        protected abstract void AddElementAttribute(E targetElem, string key, object value);
        protected abstract void AddArrayElement(A targetArray, object value);

        protected override void OnParseStart()
        {

        }
        protected override void BeginObject()
        {
            if (_currentKey != null)
            {
                _keyStack.Push(_currentKey);
            }
            _currentKey = null;
            if (_currentElem != null)
            {
                _elemStack.Push(_currentElem);
            }
            _currentElem = CreateElement();
        }
        void InternalPopCurrentObjectAndPushToPrevContext()
        {
            //current element should be object
            object c_object = _currentElem;
            if (_elemStack.Count > 0)
            {
                //pop from stack
                _currentElem = _elemStack.Pop();
                _currentKey = null;
                if (c_object == _currentElem)
                {
                    throw new System.Exception();
                }

                E c_elem = null;
                A c_arr = null;
                if ((c_elem = _currentElem as E) != null)
                {
                    _currentKey = _keyStack.Pop();
                    AddElementAttribute(c_elem, _currentKey, c_object);
                }
                else if ((c_arr = _currentElem as A) != null)
                {
                    AddArrayElement(c_arr, c_object);
                }
                else
                {
                    throw new System.NotSupportedException();
                }
            }
        }
        protected override void EndObject()
        {
            InternalPopCurrentObjectAndPushToPrevContext();
        }
        protected override void BeginArray()
        {
            if (_currentKey != null)
            {
                _keyStack.Push(_currentKey);
            }
            _currentKey = null;
            if (_currentElem != null)
            {
                _elemStack.Push(_currentElem);
            }
            _currentElem = CreateArray();
        }
        protected override void EndArray()
        {
            InternalPopCurrentObjectAndPushToPrevContext();
        }
        protected override void OnParseEnd()
        {

        }
        protected override void NewKey(StringBuilder tmpBuffer, ValueHint valueHint)
        {
            _currentKey = tmpBuffer.ToString();
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

            E c_elem = null;
            A c_arr = null;
            if ((c_elem = _currentElem as E) != null)
            {
                AddElementAttribute(c_elem, _currentKey, c_object);
            }
            else if ((c_arr = _currentElem as A) != null)
            {
                AddArrayElement(c_arr, c_object);
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
        public object CurrentElement => _currentElem;
    }
}