//MIT, 2015-present, brezza92, EngineKit and contributors
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpConnect.Data
{
    public enum EsValueHint : byte
    {
        Unknown,
        None, //None => no attribute here (!= null)

        True, False, Null,


        StringLiteral,
        StringLiteralWithSomeEscape,
        IntegerNumber,
        NumberWithFractionPart,
        NumberWithExponentialPart,

        Identifier,
        Comment,//extension
        Object,
        Array,
    }
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
            _1_ExpectObjectValueOrArrayElement,
            _2_ExpectObjectKey,
            _3_WaitForColon,
            _4_WaitForCommaOrEnd,
        }

        enum NumberPart : byte
        {
            IntegerPart,
            FractionPart,
            ExponentialPart,
        }



        public struct NumberParts
        {
            public bool integer_minus;
            public int integer_at;
            public byte integer_len; //255 
            //
            public ushort fraction_offset; //offset from intger_at
            public byte fraction_len; //255 
            //
            public bool exponent_minus;
            public ushort exponent_offset; //offset from intger_at
            public byte exponent_len; //255
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
        protected virtual void NewKey(int start, int len)
        {
        }
        protected virtual void NewValue(int start, int len)
        {
        }
        protected virtual void Comma() { }
        protected virtual void OnParseEnd()
        {

        }
        protected virtual void OnParseStart()
        {

        }
        protected virtual void NotifyError()
        {
        }

        static void ReadIdentifier(EsParserBase p, int startAt, out int latestIndex)
        {
            p.CollectedValueHint = EsValueHint.Identifier;
            int pos = startAt + 1;
            char[] sourceBuffer = p._sourceBuffer;
            int lim = sourceBuffer.Length;
            do
            {
                char c = sourceBuffer[pos];
                if (c == '_' || char.IsLetterOrDigit(c))
                {
                    //collect
                    pos++;
                }
                else
                {
                    latestIndex = pos - 1;
                    return;
                }
            } while (pos < lim);
            //
            latestIndex = pos;

        }
        static void ReadStringLiteral(EsParserBase p, char escapeChar, int startAt, out int latestIndex)
        {

            char[] sourceBuffer = p._sourceBuffer;

            p.CollectedValueHint = EsValueHint.StringLiteral;

            int pos = startAt + 1;
            char c = sourceBuffer[pos];
            int lim = sourceBuffer.Length - 1;

            if (escapeChar == '"')
            {
                while (c != '"' && pos < lim)
                {
                    //read until stop
                    if (c == '\\') //escape
                    {
                        p.CollectedValueHint = EsValueHint.StringLiteralWithSomeEscape;
                        //escape mode 1 char
                        if (pos + 1 < lim)
                        {
                            //read next char
                            char c2 = sourceBuffer[pos + 1];
                            switch (c2)
                            {
                                default:
                                    //error
                                    throw new NotSupportedException();
                                    break;
                                case '"':
                                    pos++;
                                    break;
                                //case '\'': //extension
                                //    pos++;
                                //    break;
                                case '/':
                                    pos++;
                                    break;
                                case '\\':
                                    pos++;
                                    break;
                                case 'b': // backspace
                                    pos++;
                                    break;
                                case 'f': //form ffed
                                    pos++;
                                    break;
                                case 'n': //newline
                                    pos++;
                                    break;
                                case 'r': //carriage return
                                    pos++;
                                    break;
                                case 't'://t
                                    pos++;
                                    break;
                                case 'u':
                                    if (pos < lim - 4)
                                    {
                                        //json spec
                                        //this follow by  4 chars
                                        //for extension we check if it match with 4 chars or not 
                                        uint c_uint = ParseUnicode(
                                         sourceBuffer[pos + 1],
                                         sourceBuffer[pos + 2],
                                         sourceBuffer[pos + 3],
                                         sourceBuffer[pos + 4]);
                                        pos += 4;
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            //collect more
                        }
                    }
                    pos++;
                    c = sourceBuffer[pos];
                }
            }
            else if (escapeChar == '\'')
            {
                //this is our extension
                while (c != '\'' && pos < lim)
                {
                    //read until stop
                    if (c == '\\') //escape
                    {
                        p.CollectedValueHint = EsValueHint.StringLiteralWithSomeEscape;
                        //escape mode 1 char
                        if (pos + 1 < lim)
                        {
                            //read next char
                            char c2 = sourceBuffer[pos + 1];
                            switch (c2)
                            {
                                default:
                                    //error
                                    throw new NotSupportedException();
                                    break;
                                case '"':
                                    pos++;
                                    break;
                                case '\'': //extension
                                    pos++;
                                    break;
                                case '/':
                                    pos++;
                                    break;
                                case '\\':
                                    pos++;
                                    break;
                                case 'b': // backspace
                                    pos++;
                                    break;
                                case 'f': //form ffed
                                    pos++;
                                    break;
                                case 'n': //newline
                                    pos++;
                                    break;
                                case 'r': //carriage return
                                    pos++;
                                    break;
                                case 't'://t
                                    pos++;
                                    break;
                                case 'u':
                                    if (pos < lim - 4)
                                    {
                                        //json spec
                                        //this follow by  4 chars
                                        //for extension we check if it match with 4 chars or not 
                                        uint c_uint = ParseUnicode(
                                         sourceBuffer[pos + 1],
                                         sourceBuffer[pos + 2],
                                         sourceBuffer[pos + 3],
                                         sourceBuffer[pos + 4]);
                                        pos += 4;
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            //collect more
                        }
                    }
                    pos++;
                    c = sourceBuffer[pos];
                }
            }

            latestIndex = pos;
        }



        static void ReadNumberLiteral(EsParserBase p, int startAt, out int latestIndex)
        {
            char[] sourceBuffer = p._sourceBuffer;
            NumberPart state = NumberPart.IntegerPart;
            int lim = sourceBuffer.Length - 1;
            int i = startAt;
            int collect = 0;
            //10-based
            NumberParts numParts = new NumberParts();
            numParts.integer_at = startAt;

            int integer_part_count = 0;
            int fraction_part_count = 0;
            int exponent_part_count = 0;
            for (; i < lim; ++i)
            {
                char c = sourceBuffer[i];
                switch (state)
                {
                    case NumberPart.IntegerPart:
                        {
                            if (c == '-')
                            {
                                //start integer with minus
                                numParts.integer_minus = true;
                                numParts.integer_at++;
                                collect++;
                            }
                            else if (char.IsDigit(c))
                            {
                                //collect more

                                //accum
                                integer_part_count++;
                                collect++;
                            }
                            else if (c == '.')
                            {
                                //fraction
                                collect++;
                                numParts.fraction_offset = (ushort)((i + 1) - numParts.integer_at);
                                state = NumberPart.FractionPart;
                            }
                            else
                            {
                                //this char is not part of the literal number
                                goto EXIT;
                                //break
                                //summary and return
                            }
                        }
                        break;
                    case NumberPart.FractionPart:
                        {
                            //fraction part
                            if (char.IsDigit(c))
                            {
                                //same state
                                //collect more
                                fraction_part_count++;
                                collect++;
                            }
                            else if (c == 'e' || c == 'E')
                            {
                                //exponent part 
                                //base 10
                                collect++;


                                if (i + 1 < lim)
                                {
                                    state = NumberPart.ExponentialPart;
                                    i++;
                                    c = sourceBuffer[i + 1];
                                    if (c == '+')
                                    {
                                        //ok
                                        numParts.exponent_offset = (ushort)((i + 2) - numParts.integer_at);
                                    }
                                    else if (c == '-')
                                    {
                                        numParts.exponent_minus = true;
                                        numParts.exponent_offset = (ushort)((i + 2) - numParts.integer_at);
                                    }
                                    else
                                    {
                                        //must be number
                                        numParts.exponent_offset = (ushort)((i + 1) - numParts.integer_at);
                                        goto case NumberPart.ExponentialPart;
                                    }
                                }

                            }
                            else
                            {
                                //this char is not part of the literal number
                                goto EXIT;
                            }
                        }
                        break;
                    case NumberPart.ExponentialPart:
                        {
                            //after exponent part
                            if (char.IsDigit(c))
                            {
                                //collect more
                                exponent_part_count++;
                                collect++;
                            }
                            else
                            {
                                //summary and return
                                //this char is not part of the literal number
                                goto EXIT;
                            }
                        }
                        break;
                }
            }
        EXIT:
            //--------------------
            //summary
            numParts.integer_len = (byte)integer_part_count;
            numParts.fraction_len = (byte)fraction_part_count;
            numParts.exponent_len = (byte)exponent_part_count;

            switch (state)
            {
                case NumberPart.IntegerPart:
                    p.CollectedValueHint = EsValueHint.IntegerNumber;
                    break;
                case NumberPart.FractionPart:
                    p.CollectedValueHint = EsValueHint.NumberWithFractionPart;
                    break;
                case NumberPart.ExponentialPart:
                    p.CollectedValueHint = EsValueHint.NumberWithExponentialPart;
                    break;
            }
            p.CollectedNumberParts = numParts;
            latestIndex = startAt + collect - 1;
        }
        static void ReadSingleLineComment(EsParserBase p, int startAt, out int latestIndex)
        {
            char[] sourceBuffer = p._sourceBuffer;
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
        static void ReadBlockComment(EsParserBase p, int startAt, out int latestIndex)
        {
            char[] sourceBuffer = p._sourceBuffer;
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


        protected char[] _sourceBuffer;


        protected EsValueHint CollectedValueHint { get; private set; }
        protected NumberParts CollectedNumberParts { get; private set; }

        Stack<EsElementKind> _isObjectStack = new Stack<EsElementKind>();
        public virtual void Parse(char[] buff)
        {
            _sourceBuffer = buff;
            IsSuccess = true;

            ParsingState currentState = ParsingState._1_ExpectObjectValueOrArrayElement;
            EsElementKind currentElementKind = EsElementKind.Unknown;
            _isObjectStack.Clear();

            for (int i = 0; i < buff.Length; i++)
            {
                char c = buff[i];

                if (char.IsWhiteSpace(c))
                {
                    continue;
                }
                else if (c == '/')
                {
                    //extension: comment syntax                  
                    if (i < buff.Length - 1) //has next
                    {
                        char next_c = buff[i + 1];
                        if (next_c == '/')
                        {
                            ReadSingleLineComment(this, i, out int latestIndex);
                            i = latestIndex;

                            continue;
                        }
                        else if (next_c == '*')
                        {
                            //inline comment 
                            ReadBlockComment(this, i, out int latestIndex);
                            i = latestIndex;
                            continue;
                        }
                        else
                        {
                            IsSuccess = false;
                            NotifyError();
                            return;
                        }
                    }
                    else
                    {
                        IsSuccess = false;
                        NotifyError();
                        return;
                    }
                }
                //-----------------------
#if DEBUG
                //System.Diagnostics.Debug.WriteLine(i + ":" + c + ":" + currentState);
                if (i == 10)
                {

                }
#endif

                switch (currentState)
                {
                    case ParsingState._1_ExpectObjectValueOrArrayElement:
                        {
                            switch (c)
                            {
                                case '{':
                                    {
                                        _isObjectStack.Push(currentElementKind);
                                        BeginObject(); //event 
                                        currentState = ParsingState._2_ExpectObjectKey;
                                        currentElementKind = EsElementKind.Object;
                                    }
                                    break;
                                case '[':
                                    {
                                        _isObjectStack.Push(currentElementKind);
                                        BeginArray();//event
                                        currentState = ParsingState._1_ExpectObjectValueOrArrayElement; //on the same state -- value state
                                        currentElementKind = EsElementKind.Array;
                                    }
                                    break;
                                case ']':
                                    {
                                        if (currentElementKind == EsElementKind.Array)
                                        {
                                            //empty arr
                                            currentElementKind = _isObjectStack.Pop();
                                        }
                                        else
                                        {
                                            IsSuccess = false;
                                            NotifyError();
                                            return;
                                        }
                                    }
                                    break;
                                case '"': //standard
                                case '\''://extension
                                    {
                                        //TODO: string escape here 
                                        ReadStringLiteral(this, c, i, out int latestIndex);
                                        NewValue(i, latestIndex - i + 1);
                                        i = latestIndex;
                                        currentState = ParsingState._4_WaitForCommaOrEnd;
                                    }
                                    break;
                                default:
                                    {
                                        //iden
                                        if (char.IsLetter(c) || c == '_')
                                        {
                                            //parse as  true, false, null 
                                            //or other iden
                                            //parse as idenitifer

                                            ReadIdentifier(this, i, out int latestIndex);
                                            NewValue(i, latestIndex - i + 1);
                                            i = latestIndex;
                                            currentState = ParsingState._4_WaitForCommaOrEnd;
                                        }
                                        else if (char.IsDigit(c) || (c == '-'))
                                        {
                                            //number  
                                            ReadNumberLiteral(this, i, out int latestIndex);
                                            NewValue(i, latestIndex - i + 1);
                                            i = latestIndex;
                                            currentState = ParsingState._4_WaitForCommaOrEnd;
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
                    case ParsingState._2_ExpectObjectKey:
                        {
                            //literal string 
                            //+ our extension=> iden
                            if (c == '"' || c == '\'')
                            {

                                ReadStringLiteral(this, c, i, out int latestIndex);
                                //new key from literal string, not include escape char on start and begin
                                NewKey(i + 1, latestIndex - i + 1 - 2);//event//***
                                i = latestIndex;
                                currentState = ParsingState._3_WaitForColon;
                            }
                            else if (char.IsLetter(c) || c == '_')
                            {
                                ReadIdentifier(this, i, out int latestIndex);
                                //new key from literal string
                                NewKey(i, latestIndex - i + 1);//event
                                i = latestIndex;
                                currentState = ParsingState._3_WaitForColon;
                            }
                            else if (c == '}')
                            {
                                //no key
                                //this is empty object
                                EndObject();
                                currentElementKind = _isObjectStack.Pop();
                            }
                            else
                            {
                                IsSuccess = false;
                                NotifyError();
                                //and stop
                                return;
                            }
                        }
                        break;
                    case ParsingState._3_WaitForColon:
                        {
                            if (c == ':')
                            {
                                //value of the key
                                currentState = ParsingState._1_ExpectObjectValueOrArrayElement;
                            }
                            else
                            {
                                IsSuccess = false;
                                NotifyError();
                                return;
                            }
                        }
                        break;
                    case ParsingState._4_WaitForCommaOrEnd:
                        {
                            //after literal string, literal number, array, object
                            //
                            if (c == ',')
                            {
                                Comma();

                                if (currentElementKind == EsElementKind.Object)
                                {
                                    currentState = ParsingState._2_ExpectObjectKey;
                                }
                                else if (currentElementKind == EsElementKind.Array)
                                {
                                    currentState = ParsingState._1_ExpectObjectValueOrArrayElement;
                                }
                                else
                                {
                                    IsSuccess = false;
                                    NotifyError();
                                    return;
                                }
                            }
                            else if (c == '}')
                            {
                                EndObject();
                                currentElementKind = _isObjectStack.Pop();
                            }
                            else if (c == ']')
                            {
                                EndArray();
                                currentElementKind = _isObjectStack.Pop();
                            }
                            else
                            {
                                IsSuccess = false;
                                NotifyError();
                                return;
                            }
                        }
                        break;
                }
            }
        }

        static uint ParseHex(char c1)
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
            return (ParseHex(c1) << 12) |
                    (ParseHex(c2) << 8) |
                    (ParseHex(c3) << 4) |
                    (ParseHex(c4) << 0);
        }
    }




    public abstract class EsParserBase<E, A> : EsParserBase
        where E : class
        where A : class
    {
       

        Stack<string> _keyStack = new Stack<string>();
        Stack<object> _elemStack = new Stack<object>();

        string _currentKey = null;
        object _currentValue = null;


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
            if (_currentValue != null)
            {
                _elemStack.Push(_currentValue);
            }
            _currentValue = CreateElement();
        }
        void InternalPopCurrentObjectAndPushToPrevContext()
        {
            //current element should be object
            object c_object = _currentValue;
            if (_elemStack.Count > 0)
            {
                //pop from stack
                _currentValue = _elemStack.Pop();
                _currentKey = null;
                if (c_object == _currentValue)
                {
                    throw new System.Exception();
                }
                if (_currentValue is E c_elem)
                {
                    _currentKey = _keyStack.Pop();
                    AddElementAttribute(c_elem, _currentKey, c_object);
                }
                else if (_currentValue is A c_arr)
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
            if (_currentValue != null)
            {
                _elemStack.Push(_currentValue);
            }
            _currentValue = CreateArray();
        }
        protected override void EndArray()
        {
            InternalPopCurrentObjectAndPushToPrevContext();
        }
        protected override void OnParseEnd()
        {

        }
        protected override void NewKey(int start, int len)
        {
            _currentKey = new string(_sourceBuffer, start, len);
        }

        string ParseStringWithSomeEscape(int start, int len)
        {
            //TODO: use pool
            StringBuilder sb = new StringBuilder();
            int end = start + len;
#if DEBUG
            string dbug_preview = new string(_sourceBuffer, start, len);
#endif
            for (int i = start; i < end; ++i)
            {
                char c = _sourceBuffer[i];
                if (c == '\\')
                {
                    //escape 1
                    i++;//consume
                    c = _sourceBuffer[i];
                }
                sb.Append(c);
            }
            return sb.ToString();
        }
        protected override void NewValue(int start, int len)
        {
            //current object
            string iden = "";
            object c_object = null;
            switch (CollectedValueHint)
            {
                default: throw new NotSupportedException();
                case EsValueHint.Identifier:
                    {
                        iden = new string(_sourceBuffer, start, len);
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
                    }
                    break;
                case EsValueHint.StringLiteralWithSomeEscape:
                    c_object = ParseStringWithSomeEscape(start + 1, len - 2);
                    break;
                case EsValueHint.StringLiteral:
                    c_object = new string(_sourceBuffer, start + 1, len - 2);
                    break;
                case EsValueHint.IntegerNumber:
                    iden = new string(_sourceBuffer, start, len);
                    if (len > 0)
                    {
                        if (len < 10)
                        {
                            c_object = int.Parse(iden);
                        }
                        else
                        {
                            //signed
                            long number = long.Parse(iden);
                            if (number >= int.MinValue && number <= int.MaxValue)
                            {
                                //int32 range
                                c_object = (int)number;
                            }
                            else
                            {
                                c_object = number;
                            }
                        }
                    }
                    else
                    {
                        //number len=0
                        throw new NotSupportedException();
                    }
                    break;
                case EsValueHint.NumberWithFractionPart:
                case EsValueHint.NumberWithExponentialPart:
                    iden = new string(_sourceBuffer, start, len);
                    c_object = double.Parse(iden);
                    break;
            }

            if (_currentValue is E c_elem)
            {
                AddElementAttribute(c_elem, _currentKey, c_object);
            }
            else if (_currentValue is A c_arr)
            {
                AddArrayElement(c_arr, c_object);
            }
            else
            {
                if (_currentValue == null)
                {
                    _currentValue = c_object;
                }
                else
                {
                    throw new System.NotSupportedException();
                }

            }
        }

        protected override void NotifyError()
        {
            base.NotifyError();
        }

        public object CurrentValue => _currentValue;
    }
}