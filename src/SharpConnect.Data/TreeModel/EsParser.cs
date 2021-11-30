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
        NegativeIntegerNumber,

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
        public class TextSourceProvider
        {
            public char[] Buffer { get; set; }
            public int StartAt { get; set; }
            public int Len { get; set; }

            public int TotalOffset { get; set; }
        }
        public struct BuffRange
        {
            public readonly EsValueHint hint;
            public readonly int startAt;
            public readonly int len;
            public BuffRange(int startAt, int len)
            {
                this.startAt = startAt;
                this.len = len;
                hint = EsValueHint.Unknown;
            }
            public BuffRange(int startAt, int len, EsValueHint hint)
            {
                this.startAt = startAt;
                this.len = len;
                this.hint = hint;
            }
        }

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
            IntegerPart2,
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

        protected virtual void NewConcatKey(int len)
        {
        }

        protected virtual void NewKey(int start, int len)
        {
        }
        protected virtual void NewValue(int start, int len)
        {
        }
        protected virtual void NewConcatValue(int len)
        {
        }
        protected virtual void NewComment(int start, int len)
        {
        }
        protected virtual void NewConcatComment(int len)
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

        static bool ReadIdentifier(EsParserBase p, int startAt, out int len)
        {
            p.CollectedValueHint = EsValueHint.Identifier;

            int pos = startAt;
            char[] sourceBuffer = p._sourceBuffer;
            int lim = sourceBuffer.Length;

            len = 0;
            if (pos >= lim)
            {   //may not end of the iden

                p._collectingState = CollectingState.Iden;
                return false;
            }

            do
            {
                char c = sourceBuffer[pos];
                if (c == '_' || char.IsLetterOrDigit(c))
                {
                    //collect
                    pos++;
                    len++;
                }
                else
                {
                    //finish                     
                    p._collectingState = CollectingState.None;
                    return true;
                }

            } while (pos < lim);

            //may not end of the iden             
            p._collectingState = CollectingState.Iden;
            return false;
        }

        static bool ReadStringLiteral2(EsParserBase p, char escapeChar, int startAt, out int collected_len)
        {

            char[] sourceBuffer = p._sourceBuffer;
            p.CollectedValueHint = EsValueHint.StringLiteral;
            collected_len = 0;

            int pos = startAt;
            char c = sourceBuffer[pos];

            switch (p._collectingState)
            {
                case CollectingState.Escape_U1:
                    {
                        p._collectingState = CollectingState.Char;
                        pos += 3;
                        collected_len += 3;
                    }
                    break;
                case CollectingState.Escape_U2:
                    {
                        p._collectingState = CollectingState.Char;
                        pos += 2;
                        collected_len += 2;
                    }
                    break;
                case CollectingState.Escape_U3:
                    {
                        p._collectingState = CollectingState.Char;
                        pos++;
                        collected_len++;
                    }
                    break;
                case CollectingState.Escape:
                    {
                        //after escape
                        switch (c)
                        {
                            default:
                                throw new NotSupportedException();
                            case 'u':
                                {
                                    //uint c_uint = ParseUnicode(
                                    //sourceBuffer[pos + 1],
                                    //sourceBuffer[pos + 2],
                                    //sourceBuffer[pos + 3],
                                    //sourceBuffer[pos + 4]);
                                    pos += 4;
                                    collected_len += 4;
                                    p._collectingState = CollectingState.Char;
                                }
                                break;
                            case '\\':
                            case '/':
                            case 'r':
                            case 'n':
                            case 't':
                            case 'b':
                            case 'f':
                            case '"':
                                {
                                    pos++;
                                    collected_len++;
                                    p._collectingState = CollectingState.Char;
                                }
                                break;
                        }
                    }
                    break;
            }

            if (!ReadStringLiteral(p, escapeChar, pos, out int collectedLen2))
            {
                return false;
            }
            else
            {
                collected_len += collectedLen2;
                return true;
            }
        }

        static bool ReadStringLiteral(EsParserBase p, char escapeChar, int startAt, out int collected_len)
        {
            char[] sourceBuffer = p._sourceBuffer;

            p.CollectedValueHint = EsValueHint.StringLiteral;
            p._collectingState = CollectingState.Char;//**

            collected_len = 0;
            int pos = startAt;
            if (pos >= sourceBuffer.Length)
            {
                return false;
            }

            char c = sourceBuffer[pos];
            int lim = sourceBuffer.Length;

            bool complete = false;
            if (escapeChar == '"')
            {
                while (pos < lim)
                {
                    if (c == '"')
                    {
                        //stop here
                        complete = true;
                        p._collectingState = CollectingState.None;
                        collected_len++;
                        break;
                    }
                    //read until stop
                    if (c == '\\') //escape
                    {
                        collected_len++;
                        p._collectingState = CollectingState.Escape;
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
                                //case '\'': //extension
                                //    pos++;
                                //    break;
                                case '"':
                                case '/':
                                case '\\':
                                case 'b': // backspace
                                case 'f': //form feed
                                case 'n': //newline
                                case 'r': //carriage return
                                case 't'://t
                                    collected_len++;
                                    pos++;
                                    p._collectingState = CollectingState.Char;
                                    break;
                                case 'u':
                                    if (pos < lim - 4)
                                    {
                                        //json spec
                                        //this follow by  4 chars
                                        //for extension we check if it match with 4 chars or not 
                                        p._collectingState = CollectingState.Char;

                                        //uint c_uint = ParseUnicode(
                                        // sourceBuffer[pos + 1],
                                        // sourceBuffer[pos + 2],
                                        // sourceBuffer[pos + 3],
                                        // sourceBuffer[pos + 4]);

                                        pos += 4;
                                        collected_len += 4;
                                    }
                                    else
                                    {
                                        //incomplete

                                        int waiting = lim - pos - 1;
                                        switch (waiting)
                                        {
                                            case 3:
                                                pos += 3;
                                                collected_len += 3;
                                                p._collectingState = CollectingState.Escape_U3;
                                                break;
                                            case 2:
                                                pos += 2;
                                                collected_len += 2;
                                                p._collectingState = CollectingState.Escape_U2;
                                                break;

                                            case 1:
                                                pos += 1;
                                                collected_len += 1;
                                                p._collectingState = CollectingState.Escape_U1;
                                                break;
                                            default:
                                                throw new NotSupportedException();
                                        }
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            //collect more
                            p._collectingState = CollectingState.Escape;
                        }
                    }
                    else
                    {
                        collected_len++;
                    }

                    pos++;
                    if (pos < lim)
                    {
                        c = sourceBuffer[pos];
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else if (escapeChar == '\'')
            {
                throw new NotSupportedException();
            }

            if (!complete)
            {

            }

            return complete;
        }

        /// <summary>
        /// save latest read pos on current buffer
        /// </summary>
        /// <param name="startAt"></param>
        protected virtual void SaveLatestReadPos(int startAt)
        {

        }

        static bool ReadNumberLiteral(EsParserBase p, int startAt, out int collected_len)
        {
            char[] sourceBuffer = p._sourceBuffer;
            NumberPart state = NumberPart.IntegerPart;
            int lim = sourceBuffer.Length;

            int i = startAt;
            collected_len = 0;
            //10-based

            NumberParts numParts = new NumberParts();
            numParts.integer_at = startAt;

            int integer_part_count = 0;
            int fraction_part_count = 0;
            int exponent_part_count = 0;
            bool finish = false;

            CollectingState collectingState = p._collectingState;//**
            switch (collectingState)
            {
                default:
                    break;
                case CollectingState.Num_Exponent_NumPart:
                    state = NumberPart.ExponentialPart;
                    break;
                case CollectingState.Num_Exponent_E:
                    state = NumberPart.ExponentialPart;
                    break;
                case CollectingState.Num_Fraction:
                    state = NumberPart.FractionPart;
                    break;
                case CollectingState.Num_Integer:
                    state = NumberPart.IntegerPart2;
                    break;
                case CollectingState.None:
                    break;
            }
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
                                collectingState = CollectingState.Num_Integer;
                                state = NumberPart.IntegerPart2;//after minus
                                numParts.integer_minus = true;
                                numParts.integer_at++;
                                collected_len++;
                            }
                            else if (char.IsDigit(c))
                            {
                                //collect more 
                                //accum
                                collectingState = CollectingState.Num_Integer;
                                state = NumberPart.IntegerPart2;//after minus
                                integer_part_count++;
                                collected_len++;
                            }

                            else
                            {
                                //this char is not part of the literal number
                                collectingState = CollectingState.None;//finish
                                finish = true;
                                goto EXIT;
                                //break
                                //summary and return
                            }
                        }
                        break;
                    case NumberPart.IntegerPart2:
                        {
                            if (char.IsDigit(c))
                            {
                                //collect more 
                                //accum
                                integer_part_count++;
                                collected_len++;
                            }
                            else if (c == 'e' || c == 'E')
                            {
                                collectingState = CollectingState.Num_Exponent_E;
                                state = NumberPart.ExponentialPart;
                                collected_len++;//collect this e or E
                                if (i + 1 < lim)
                                {
                                    i++; //consume e or E
                                    c = sourceBuffer[i + 1];
                                    if (c == '+')
                                    {
                                        //ok
                                        collectingState = CollectingState.Num_Exponent_NumPart;
                                        collected_len++;//collect this e or E
                                        numParts.exponent_offset = (ushort)((i + 2) - numParts.integer_at);
                                    }
                                    else if (c == '-')
                                    {
                                        collectingState = CollectingState.Num_Exponent_NumPart;
                                        numParts.exponent_minus = true;
                                        collected_len++;//collect this e or E
                                        numParts.exponent_offset = (ushort)((i + 2) - numParts.integer_at);
                                    }
                                    else
                                    {
                                        //must be number
                                        numParts.exponent_offset = (ushort)((i + 1) - numParts.integer_at);
                                        goto case NumberPart.ExponentialPart;//
                                    }
                                }
                            }
                            else if (c == '.')
                            {
                                //fraction
                                collected_len++;
                                numParts.fraction_offset = (ushort)((i + 1) - numParts.integer_at);
                                state = NumberPart.FractionPart;
                                collectingState = CollectingState.Num_Fraction;
                            }
                            else
                            {
                                //this char is not part of the literal number
                                collectingState = CollectingState.None;//finish
                                finish = true;
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
                                collected_len++;
                            }
                            else if (c == 'e' || c == 'E')
                            {
                                //exponent part 
                                //base 10
                                collectingState = CollectingState.Num_Exponent_E;
                                state = NumberPart.ExponentialPart;
                                collected_len++;//collect this e or E
                                if (i + 1 < lim)
                                {
                                    i++; //consume e or E
                                    c = sourceBuffer[i + 1];
                                    if (c == '+')
                                    {
                                        //ok
                                        collectingState = CollectingState.Num_Exponent_NumPart;
                                        collected_len++;//collect this e or E
                                        numParts.exponent_offset = (ushort)((i + 2) - numParts.integer_at);
                                    }
                                    else if (c == '-')
                                    {
                                        collectingState = CollectingState.Num_Exponent_NumPart;
                                        numParts.exponent_minus = true;
                                        collected_len++;//collect this e or E
                                        numParts.exponent_offset = (ushort)((i + 2) - numParts.integer_at);
                                    }
                                    else
                                    {
                                        //must be number
                                        numParts.exponent_offset = (ushort)((i + 1) - numParts.integer_at);
                                        goto case NumberPart.ExponentialPart;//
                                    }
                                }
                            }
                            else
                            {
                                //this char is not part of the literal number
                                collectingState = CollectingState.None;//finish
                                finish = true;
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
                                collectingState = CollectingState.Num_Exponent_NumPart;
                                exponent_part_count++;
                                collected_len++;
                            }
                            else
                            {

                                //this char is not part of the literal number
                                collectingState = CollectingState.None;//finish
                                finish = true;
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
                case NumberPart.IntegerPart2:
                case NumberPart.IntegerPart:
                    {
                        p.CollectedValueHint = numParts.integer_minus ? EsValueHint.NegativeIntegerNumber : EsValueHint.IntegerNumber;
                    }
                    break;
                case NumberPart.FractionPart:
                    p.CollectedValueHint = EsValueHint.NumberWithFractionPart;
                    break;
                case NumberPart.ExponentialPart:
                    p.CollectedValueHint = EsValueHint.NumberWithExponentialPart;
                    break;
            }
            p.CollectedNumberParts = numParts;
            p._collectingState = collectingState;
            return finish;
        }

        static bool ReadComment(EsParserBase p, int startAt, out int collected_len)
        {
            char[] sourceBuffer = p._sourceBuffer;
            collected_len = 0;
            int pos = startAt;
            if (pos >= sourceBuffer.Length)
            {
                return false;
            }

            int lim = sourceBuffer.Length;
            bool complete = false;
            char c = sourceBuffer[pos];

            //state from previous state
            CollectingState collecting_state = p._collectingState;

            switch (collecting_state)
            {
                default:
                    throw new NotSupportedException();
                case CollectingState.Comment_Maybe:
                    {
                        if (c == '/')
                        {
                            collecting_state = p._collectingState = CollectingState.Comment_Line;
                            collected_len++; //consume /
                            pos++;
                            if (pos < lim)
                            {
                                goto case CollectingState.Comment_Line;
                            }
                        }
                        else if (c == '*')
                        {
                            //block comment
                            collecting_state = p._collectingState = CollectingState.Comment_Block;
                            collected_len++; //consume /
                            pos++;
                            if (pos < lim)
                            {
                                goto case CollectingState.Comment_Block;
                            }
                        }
                    }
                    break;
                case CollectingState.Comment_Line:
                    {
                        //read until end of this line comment
                        //single line comment
                        do
                        {
                            c = sourceBuffer[pos];
                            if (c == '\r')
                            {
                                if (pos + 1 < lim)
                                {
                                    pos++;
                                    c = sourceBuffer[pos];
                                    p._collectingState = CollectingState.None;//resetp._collectingState 
                                    if (c == '\n')
                                    {
                                        
                                    }
                                    else
                                    {

                                    }

                                    return true;
                                }
                                else
                                {
                                    p._collectingState = CollectingState.Comment_Line_Ending_R;
                                    return false;
                                }
                            }
                            else if (c == '\n')
                            {
                                //just \n
                                pos++;
                                p._collectingState = CollectingState.None;//resetp._collectingState
                                return true;
                            }
                            else
                            {
                                collected_len++; //consume any c
                                pos++;
                            }
                        } while (pos < lim);
                    }
                    break;
                case CollectingState.Comment_Line_Ending_R:
                    {
                        //
                        if (c == '\n')
                        {
                            p._collectingState = CollectingState.None;
                        }
                        return true;
                    }
                case CollectingState.Comment_Block_Ending:
                    {
                        //collecting content inside the block
                        if (c == '/')
                        {
                            //block comment 
                            collected_len++; //consume /
                            return true;
                        }
                        else
                        {
                            collected_len++; //consume 
                            p._collectingState = CollectingState.Comment_Block;
                            pos++;
                            if (pos < lim)
                            {
                                goto case CollectingState.Comment_Block;//turn to block cmment state
                            }
                        }
                    }
                    break;
                case CollectingState.Comment_Block:
                    {
                        do
                        {
                            c = sourceBuffer[pos];
                            collected_len++; //consume any c
                            if (c == '*')
                            {
                                p._collectingState = CollectingState.Comment_Block_Ending;
                                if (pos + 1 < lim)
                                {
                                    collected_len++;//consume any c
                                    pos++;
                                    c = sourceBuffer[pos];
                                    if (c == '/')
                                    {
                                        //stop the block
                                        p._collectingState = CollectingState.None;//resetp._collectingState
                                        return true;
                                    }
                                    else
                                    {
                                        p._collectingState = CollectingState.Comment_Block;//turn back
                                    }
                                }
                            }
                            pos++;

                        } while (pos < lim);
                    }
                    break;
            }
            return complete;
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
        protected EsValueHint CollectedValueHint { get; set; }
        protected NumberParts CollectedNumberParts { get; private set; }

        readonly Stack<EsElementKind> _elemKindStack = new Stack<EsElementKind>();
        ParsingState _curr_state = ParsingState._1_ExpectObjectValueOrArrayElement;
        EsElementKind _curr_elemKind = EsElementKind.Unknown;
        int _latestIndex = 0;

        public void Parse(char[] buff)
        {
            TextSourceProvider p = new TextSourceProvider();
            p.Buffer = buff;
            p.Len = buff.Length;
            Parse(p);
        }
        public void Reset()
        {
            _sourceBuffer = null;
            IsSuccess = true;
            _elemKindStack.Clear();
            _curr_state = ParsingState._1_ExpectObjectValueOrArrayElement;
            _curr_elemKind = EsElementKind.Unknown;
            _elemKindStack.Clear();
        }

        public bool IsFinish()
        {
            return _elemKindStack.Count == 0;
        }
        void NotifyErrorAndBreak(ref int read_index)
        {
            IsSuccess = false;
            read_index = _stopBefore + 1;
            NotifyError();
        }

        int _stopBefore;


        CollectingState _collectingState;

        enum CollectingState
        {
            None,
            Char,
            Escape,
            Escape_U3,
            Escape_U2,
            Escape_U1,

            Iden,

            Num_Integer,
            Num_Fraction,
            Num_Exponent_E,
            Num_Exponent_NumPart,

            //extension
            Comment_Maybe,
            Comment_Line,//line comment
            Comment_Line_Ending_R, //r
            Comment_Block,//multiline block comment /*
            Comment_Block_Ending,//  */

        }
#if DEBUG
        public int dbug_charIndex = 0;
        public int _start_index = 0;
#endif
        void SkipWhitespace(char[] buff, int startAt, out int collected_len)
        {
            int i = startAt;
            int collected = 0;
            for (; i < buff.Length; ++i)
            {
                char c = buff[i];
                if (!char.IsWhiteSpace(c))
                {
                    //stop here
                    collected_len = collected;
                    return;
                }
                else
                {
                    collected++;
                }
            }
            collected_len = collected;
        }


        public void Parse(TextSourceProvider source)
        {
            char[] buff = source.Buffer;
            int i = source.StartAt;
            int len = source.Len;

            int stopBefore = i + len;
            _stopBefore = stopBefore;
            _sourceBuffer = buff;

            int totalStartOffset = source.TotalOffset;


            EsElementKind currElemKind = _curr_elemKind;//from latest session
            ParsingState currentState = _curr_state;

            //---------- 

            //[A] check collecting state from previous session
            switch (_collectingState)
            {
                default:
                    {
                        throw new NotSupportedException();
                    }
                case CollectingState.None:
                    //nothing from prev session
                    break;
                case CollectingState.Iden:
                    {

                        if (!ReadIdentifier(this, i, out int collected_len))
                        {
                            throw new NotSupportedException();//TODO: review here
                        }
                        else
                        {

                            //accept new index 
                            //accept 
                            _collectingState = CollectingState.None;//reset

                            //int global_len = totalStartOffset + collected_len;
                            NewConcatValue(collected_len);

                            i += collected_len;
                            currentState = ParsingState._4_WaitForCommaOrEnd;
                        }
                    }
                    break;
                case CollectingState.Num_Integer:
                case CollectingState.Num_Fraction:
                case CollectingState.Num_Exponent_NumPart:
                case CollectingState.Num_Exponent_E:
                    {
                        //parse literal number
                        if (!ReadNumberLiteral(this, i, out int collected_len))
                        {
                            throw new NotSupportedException();//TODO: review here
                        }
                        else
                        {

                            _collectingState = CollectingState.None;//reset

                            //int global_len = totalStartOffset + collected_len;
                            //NewValueFromGlobalOffset(_latestIndex + i, global_len - _latestIndex);
                            NewConcatValue(collected_len);
                            i += collected_len;
                            currentState = ParsingState._4_WaitForCommaOrEnd;
                        }
                    }
                    break;
                case CollectingState.Escape_U3:
                case CollectingState.Escape_U2:
                case CollectingState.Escape_U1:
                case CollectingState.Escape:
                case CollectingState.Char:
                    {

                        if (!ReadStringLiteral2(this, '"', i, out int collected_len))
                        {
                            //not complete
                            //not acccept latest index
                            i = stopBefore + 1;//force stop

                            //save latest parse state
                            _curr_elemKind = currElemKind;
                            _curr_state = currentState;
                            return;//long string 
                        }

                        //accept latest index
                        _collectingState = CollectingState.None; //reset

                        //finish
                        int global_len = totalStartOffset + collected_len;
                        switch (currentState)
                        {
                            default:
                                throw new NotSupportedException();
                            case ParsingState._1_ExpectObjectValueOrArrayElement:
                                {
                                    //NewValueFromGlobalOffset(_latestIndex + i, global_len - _latestIndex);
                                    NewConcatValue(collected_len);
                                    i += collected_len;
                                    currentState = ParsingState._4_WaitForCommaOrEnd;
                                }
                                break;
                            case ParsingState._2_ExpectObjectKey:
                                {
                                    //new key from literal string, not include escape char on start and begin
                                    //NewKeyFromGlobalOffset(_latestIndex + i, global_len - _latestIndex);
                                    NewConcatKey(collected_len);
                                    i += collected_len;
                                    currentState = ParsingState._3_WaitForColon;
                                }
                                break;
                        }
                    }
                    break;
                case CollectingState.Comment_Maybe:
                case CollectingState.Comment_Block:
                case CollectingState.Comment_Block_Ending:
                case CollectingState.Comment_Line:
                case CollectingState.Comment_Line_Ending_R:
                    {
                        //found * after /*
                        if (!ReadComment(this, i, out int collected_len))
                        {
                            //not complete
                            //not acccept latest index
                            i = stopBefore + 1;//force stop                             
                            return;//long string 
                        }
                        else
                        {
                            CollectedValueHint = EsValueHint.Comment;
                            NewConcatComment(collected_len);
                        }
                    }
                    break;
            }

            //---------- 

            //[B]              
            for (; i < stopBefore;)
            {
#if DEBUG
                dbug_charIndex++;
                if (dbug_charIndex >= 44)
                {

                }
#endif
                char c = buff[i];

                if (char.IsWhiteSpace(c))
                {
                    SkipWhitespace(buff, i + 1, out int collected_len);
                    i += 1 + collected_len;
                    continue;
                }
                else if (c == '/')
                {
                    //line comment or 
                    _collectingState = CollectingState.Comment_Maybe;
                    if (!ReadComment(this, i + 1, out int collected_len))
                    {
                        SaveLatestReadPos(i);
                        return;
                    }
                    else
                    {
                        NewComment(i, collected_len + 1);
                        i += 1 + collected_len;
                        continue;
                    }

                }
                //-----------------------
#if DEBUG
                //System.Diagnostics.Debug.WriteLine(i + ":" + c + ":" + currentState);
                //if (i == 10)
                //{

                //}
#endif

                switch (currentState)
                {

                    case ParsingState._1_ExpectObjectValueOrArrayElement:
                        {
                            switch (c)
                            {
                                case '{':
                                    {
                                        _elemKindStack.Push(currElemKind);
                                        BeginObject(); //event 

                                        currentState = ParsingState._2_ExpectObjectKey;
                                        currElemKind = EsElementKind.Object;
                                        i++;
                                    }
                                    break;
                                case '[':
                                    {
                                        _elemKindStack.Push(currElemKind);

                                        BeginArray();//event
                                        currentState = ParsingState._1_ExpectObjectValueOrArrayElement; //on the same state -- value state
                                        currElemKind = EsElementKind.Array;
                                        i++;
                                    }
                                    break;
                                case ']':
                                    {
                                        if (currElemKind == EsElementKind.Array)
                                        {
                                            //empty arr
                                            EndArray();
                                            currElemKind = _elemKindStack.Pop();

                                            currentState = ParsingState._4_WaitForCommaOrEnd;
                                            i++;
                                            continue;
                                        }
                                        else
                                        {
                                            NotifyErrorAndBreak(ref i);//***
                                            continue;
                                        }
                                    }
                                case '"': //standard
                                case '\''://extension
                                    {
                                        //TODO: string escape here 
                                        if (!ReadStringLiteral(this, c, i + 1, out int collected_len))
                                        {
                                            //not complete
                                            //not acccept latest index
                                            SaveLatestReadPos(i);
                                            _latestIndex = i + totalStartOffset;//save before exit
                                            i = stopBefore + 1;//force stop 
                                            continue;
                                        }
                                        else
                                        {
                                            //accept latest index
                                            _collectingState = CollectingState.None;
                                            collected_len++;
                                        }

                                        NewValue(i, collected_len);
                                        i += collected_len;

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
                                            if (!ReadIdentifier(this, i + 1, out int collected_len))
                                            {
                                                SaveLatestReadPos(i);//***
                                                _latestIndex = i + totalStartOffset;//save start index
                                                i = stopBefore + 1;//force stop
                                            }
                                            else
                                            {
                                                NewValue(i, collected_len + 1);
                                                i += 1 + collected_len;
                                                currentState = ParsingState._4_WaitForCommaOrEnd;
                                            }
                                        }
                                        else if (char.IsDigit(c) || (c == '-'))
                                        {
                                            //number  
                                            if (!ReadNumberLiteral(this, i, out int collected_len))
                                            {
                                                SaveLatestReadPos(i);//before
                                                _latestIndex = i + totalStartOffset;
                                                i = stopBefore + 1;//force stop
                                            }
                                            else
                                            {
                                                NewValue(i, collected_len);
                                                i += collected_len;
                                                currentState = ParsingState._4_WaitForCommaOrEnd;
                                            }
                                        }
                                        else
                                        {
                                            NotifyErrorAndBreak(ref i);//***
                                            continue;
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

                                if (!ReadStringLiteral(this, c, i + 1, out int collected_len))
                                {
                                    //not complete
                                    //not acccept latest index
                                    SaveLatestReadPos(i);
                                    _latestIndex = i + totalStartOffset;
                                    i = stopBefore + 1;//force stop 
                                    continue;
                                }
                                else
                                {
                                    //accept latest index
                                    _collectingState = CollectingState.None;
                                    collected_len++;
                                }
                                //new key from literal string, not include escape char on start and begin
                                NewKey(i, collected_len);
                                i += collected_len;
                                currentState = ParsingState._3_WaitForColon;
                            }
                            else if (char.IsLetter(c) || c == '_')
                            {
                                throw new NotSupportedException();
                                ////extension?
                                //if (!ReadIdentifier(this, i + 1, out latestIndex))
                                //{

                                //}
                                ////new key from literal string
                                //NewKey(i, latestIndex - i + 1);//event
                                //i = latestIndex;
                                //currentState = ParsingState._3_WaitForColon;
                            }
                            else if (c == '}')
                            {
                                //no key
                                //this is empty object

                                i++;
                                EndObject();
                                currElemKind = _elemKindStack.Pop();
                                currentState = ParsingState._4_WaitForCommaOrEnd;
                            }
                            else
                            {
                                NotifyErrorAndBreak(ref i);//***
                                continue;
                            }
                        }
                        break;
                    case ParsingState._3_WaitForColon:
                        {
                            if (c == ':')
                            {
                                //value of the key

                                i++;
                                currentState = ParsingState._1_ExpectObjectValueOrArrayElement;
                            }
                            else
                            {
                                NotifyErrorAndBreak(ref i);//***
                                continue;
                            }
                        }
                        break;
                    case ParsingState._4_WaitForCommaOrEnd:
                        {
                            //after literal string, literal number, array, object


                            if (c == ',')
                            {
                                Comma();
                                if (currElemKind == EsElementKind.Object)
                                {
                                    currentState = ParsingState._2_ExpectObjectKey;

                                }
                                else if (currElemKind == EsElementKind.Array)
                                {
                                    currentState = ParsingState._1_ExpectObjectValueOrArrayElement;

                                }
                                else
                                {
                                    NotifyErrorAndBreak(ref i);//***
                                    continue;
                                }
                                i++;
                            }
                            else if (c == '}')
                            {

                                i++;
                                EndObject();
                                currElemKind = _elemKindStack.Pop();
                            }
                            else if (c == ']')
                            {

                                i++;
                                EndArray();
                                currElemKind = _elemKindStack.Pop();
                            }
                            else
                            {
                                NotifyErrorAndBreak(ref i);//***
                                continue;
                            }
                        }
                        break;
                }
            }


            //***
            //save latest parse state
            _curr_elemKind = currElemKind;
            _curr_state = currentState;


            //check if finish or not
            if (_elemKindStack.Count > 0)
            {
                //document is not complete
            }

            //if (latestIndex != stopBefore - 1)
            //{

            //}
        }
        //public int LatestIndex => _latestIndex;


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


    class StringDic
    {
        Dictionary<ulong, int> _keyDic = new Dictionary<ulong, int>();
        List<string> _keyList = new List<string>();

        public StringDic()
        {
            Register(new char[0], 0, 0);//empty string
        }
        public int Register(char[] buffer, int start, int len)
        {
            //calculate has for specific region
            ulong hash_value = CalculateHash(buffer, start, len);
            if (!_keyDic.TryGetValue(hash_value, out int index))
            {
                index = _keyList.Count;//**
                _keyDic.Add(hash_value, _keyDic.Count);
                _keyList.Add(new string(buffer, start, len));
            }
            return index;
        }
        public int Count => _keyList.Count;
        public string GetKey(int index) => _keyList[index];
        static ulong CalculateHash(char[] buffer, int start, int len)
        {
            //https://stackoverflow.com/questions/9545619/a-fast-hash-function-for-string-in-c-sharp
            ulong hashedValue = 3074457345618258791ul;
            int end = start + len;
            for (int i = start; i < end; i++)
            {
                hashedValue += buffer[i];
                hashedValue *= 3074457345618258799ul;
            }
            return hashedValue;
        }
    }

    public abstract class EsParserBase<E, A> : EsParserBase
        where E : class
        where A : class
    {
        struct CurrentObject
        {
            public E elem;
            public A arr;
            public bool isArr;
        }

        readonly Stack<int> _keyStack = new Stack<int>();
        readonly Stack<CurrentObject> _elemStack = new Stack<CurrentObject>();

        StringDic _keyDic = new StringDic();
        bool _emptyKey = true;

        int _currentKey;
        //---


        bool _emptyCurrentValue = true;
        CurrentObject _currValue;



        public EsParserBase()
        {

        }
        protected abstract E CreateElement();
        protected abstract A CreateArray();

        protected abstract void AddElementAttribute(E targetElem, int key, A value);
        protected abstract void AddElementAttribute(E targetElem, int key, E value);
        protected abstract void AddElementAttribute(E targetElem, int key, EsValueHint valueHint);

        protected abstract void AddArrayElement(A targetArray, A value);
        protected abstract void AddArrayElement(A targetArray, E value);
        protected abstract void AddArrayElement(A targetArray, EsValueHint valueHint);

        protected override void OnParseStart()
        {

        }

        protected int GetPrevKeyIndex()
        {
            if (_keyStack.Count < 1)
            {
                return -1;
            }
            else
            {
                return _keyStack.Peek();
            }
        }
        protected int CurrentKeyIndex => _currentKey;

        protected override void BeginObject()
        {
            if (!_emptyKey)
            {
                //copy old key

                _keyStack.Push(_currentKey);
            }
            _emptyKey = true;

            if (!_emptyCurrentValue)
            {
                _elemStack.Push(_currValue);
            }
            _currValue = new CurrentObject();
            _currValue.elem = CreateElement();
            _emptyCurrentValue = false;
        }
        void InternalPopCurrentObjectAndPushToPrevContext()
        {
            //current element should be object
            CurrentObject c_object = _currValue;
            if (_elemStack.Count > 0)
            {
                //pop from stack
                _currValue = _elemStack.Pop();
                _emptyKey = true;

                if (!_currValue.isArr)
                {
                    if (_keyStack.Count > 0)
                    {
                        _currentKey = _keyStack.Pop();
                        if (c_object.isArr)
                        {
                            AddElementAttribute(_currValue.elem, _currentKey, c_object.arr);
                        }
                        else
                        {
                            AddElementAttribute(_currValue.elem, _currentKey, c_object.elem);
                        }
                    }
                    else
                    {
                        //?
                    }
                }
                else
                {
                    if (c_object.isArr)
                    {
                        AddArrayElement(_currValue.arr, c_object.arr);
                    }
                    else
                    {
                        AddArrayElement(_currValue.arr, c_object.elem);
                    }

                }
            }
        }
        protected override void EndObject()
        {
            InternalPopCurrentObjectAndPushToPrevContext();
        }
        protected override void BeginArray()
        {
            if (!_emptyKey)
            {
                _keyStack.Push(_currentKey);
            }
            _emptyKey = true;


            if (!_emptyCurrentValue)
            {
                _elemStack.Push(_currValue);
            }

            _currValue = new CurrentObject();
            _currValue.arr = CreateArray();
            _currValue.isArr = true;
            _emptyCurrentValue = false;
        }
        protected override void EndArray()
        {
            InternalPopCurrentObjectAndPushToPrevContext();
        }
        protected override void OnParseEnd()
        {

        }

        public string GetKeyAsStringByIndex(int index) => _keyDic.GetKey(index);


        const int KEY_BUFFER_SIZE = 1024;
        TempSavedBuffer _keyBuffer = new TempSavedBuffer(new char[KEY_BUFFER_SIZE], 0);

        protected int RegisterKey(string key)
        {
            char[] buffer = key.ToCharArray();
            return _keyDic.Register(buffer, 0, buffer.Length);
        }
        protected override void NewKey(int start, int len)
        {
            //implement key
            //key trend
            _emptyKey = false;
            _currentKey = _keyDic.Register(_sourceBuffer, start + 1, len - 2);
        }
        protected override void NewConcatKey(int len)
        {
            //create new key
            //in this version we handle a short key
            //that can be fill inside 1 buffer

            if (_tmpSavedBufferList.Count > 1)
            {
                throw new NotSupportedException();
            }

            int dstPos = 0;
            for (int i = 0; i < _tmpSavedBufferList.Count; ++i)
            {
                TempSavedBuffer tmpBuffer = _tmpSavedBufferList[i];
                Array.Copy(tmpBuffer.buffer, 0, _keyBuffer.buffer, dstPos, tmpBuffer.len);
                dstPos += tmpBuffer.len;
                ReleaseFreeTempBuffer(tmpBuffer);
            }
            _tmpSavedBufferList.Clear();

            if (len > 0)
            {
                Array.Copy(_sourceBuffer, 0, _keyBuffer.buffer, dstPos, len);
                dstPos += len;
            }

            _emptyKey = false;
            _currentKey = _keyDic.Register(_keyBuffer.buffer, 1, dstPos - 2);
            _concat_value_len = 0;
        }

        struct TempSavedBuffer
        {
            public char[] buffer;
            public int len;
            public TempSavedBuffer(char[] buffer, int len)
            {
                this.buffer = buffer;
                this.len = len;
            }
#if DEBUG
            public override string ToString()
            {
                return len.ToString();
            }
#endif
        }

        Stack<TempSavedBuffer> _pool = new Stack<TempSavedBuffer>();
        TempSavedBuffer GetFreeTempBuffer(int size)
        {
            if (_pool.Count == 0)
            {
                return new TempSavedBuffer(new char[size], 0);
            }
            else
            {
                return _pool.Pop();
            }
        }
        void ReleaseFreeTempBuffer(TempSavedBuffer tmpBuffer)
        {
            _pool.Push(tmpBuffer);
        }

#if DEBUG
        int _count = 0;
#endif

        List<TempSavedBuffer> _tmpSavedBufferList = new List<TempSavedBuffer>();
        int _concat_value_len = 0;
        protected override void SaveLatestReadPos(int startAt)
        {

#if DEBUG
            _count++;
            //backup current buffer 
#endif

            int copy_len = _sourceBuffer.Length - startAt;
            //then select proper buffer
            if (startAt == 0)
            {

            }

            TempSavedBuffer tmpBuffer = GetFreeTempBuffer(_sourceBuffer.Length);
            //temp copy data to here //reuse buffer?
            Array.Copy(_sourceBuffer, startAt, tmpBuffer.buffer, 0, copy_len);
            tmpBuffer.len = copy_len;
            _tmpSavedBufferList.Add(tmpBuffer);
            _concat_value_len += copy_len;
            base.SaveLatestReadPos(startAt);
        }

        static int ParseInt32(char[] buffer, int start, int len)
        {

            int result = 0;
            int i = start;
            int sign = 1;
            for (int n = 0; n < len; ++n)
            {
                char c = buffer[i];
                i++;
                result *= 10;

                switch (c)
                {
                    case '-':
                        sign = -1;
                        break;
                    case '0':
                        break;
                    case '1':
                        result += 1;
                        break;
                    case '2':
                        result += 2;
                        break;
                    case '3':
                        result += 3;
                        break;
                    case '4':
                        result += 4;
                        break;
                    case '5':
                        result += 5;
                        break;
                    case '6':
                        result += 6;
                        break;
                    case '7':
                        result += 7;
                        break;
                    case '8':
                        result += 8;
                        break;
                    case '9':
                        result += 9;
                        break;
                    case '.':
                        throw new NotSupportedException();
                        break;
                }
            }
            return result * sign;
        }
        static long ParseInt64(char[] buffer, int start, int len)
        {
            long result = 0;
            bool negative = false;
            int i = start;

            for (int n = 0; n < len; ++n)
            {
                char c = buffer[i];
                i++;
                result *= 10L;

                switch (c)
                {
                    case '-':
                        negative = true;
                        break;
                    case '0':
                        break;
                    case '1':
                        result += 1;
                        break;
                    case '2':
                        result += 2;
                        break;
                    case '3':
                        result += 3;
                        break;
                    case '4':
                        result += 4;
                        break;
                    case '5':
                        result += 5;
                        break;
                    case '6':
                        result += 6;
                        break;
                    case '7':
                        result += 7;
                        break;
                    case '8':
                        result += 8;
                        break;
                    case '9':
                        result += 9;
                        break;
                    case '.':
                        throw new NotSupportedException();
                        break;
                }
            }

            if (negative)
            {
                return -1L * result;
            }
            else
            {
                return result;
            }
        }

        protected int GetValueAsInt32()
        {
            if (_isConcatValue)
            {
                TempSavedBuffer temp_buffer2 = GetFreeTempBuffer(_sourceBuffer.Length);
                char[] bb = temp_buffer2.buffer;
                int totalLen = ConcatSmallValue(bb, _local_value_len);
                int result = ParseInt32(bb, 0, totalLen);
                ReleaseFreeTempBuffer(temp_buffer2);
                return result;
            }
            else
            {
                //only current value
                if (_local_value_len > 10)
                {
                    //may be long/ulong
                    throw new NotSupportedException();
                }
                else
                {
                    return ParseInt32(_sourceBuffer, _value_start, _local_value_len);
                }

            }
            return 0;
        }
        protected long GetValueAsInt64()
        {
            if (_isConcatValue)
            {
                TempSavedBuffer temp_buffer2 = GetFreeTempBuffer(_sourceBuffer.Length);
                char[] bb = temp_buffer2.buffer;
                int totalLen = ConcatSmallValue(bb, _local_value_len);
                long result = ParseInt64(bb, 0, totalLen);
                ReleaseFreeTempBuffer(temp_buffer2);
                return result;
            }
            else
            {
                //only current value
                return ParseInt64(_sourceBuffer, _value_start, _local_value_len);
            }
        }

        StringBuilder _sb = new StringBuilder();

        static void AppendStringWithSomeEscape(StringBuilder sb, char[] source, int start, int len)
        {
            int i = start;

            for (int n = 0; n < len; ++n)
            {
                char c = source[i];
                i++;
                if (c == '\\')
                {
                    //escape
                    char c2 = source[i];
                    n++;
                    i++;
                    switch (c2)
                    {
                        default:

                            break;
                        case '"':
                            sb.Append('"');
                            break;
                        case 'u':
                            {

                            }
                            break;
                        case '\\':
                            sb.Append('\\');
                            break;
                        case 'f':
                            sb.Append('\f');
                            break;
                        case '/':
                            sb.Append('/');//?
                            break;
                        case 'b':
                            sb.Append('\b');
                            break;
                        case 't':
                            sb.Append('\t');
                            break;
                        case 'r':
                            sb.Append('\r');
                            break;
                        case 'n':
                            sb.Append('\n');
                            break;
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }
        }
        protected string GetValueAsStringWithEscape()
        {
            if (_isConcatValue)
            {
                //need to merge
                _sb.Length = 0;//clear
                for (int i = 0; i < _tmpSavedBufferList.Count; ++i)
                {
                    TempSavedBuffer bb = _tmpSavedBufferList[i];
                    if (i == 0)
                    {
                        AppendStringWithSomeEscape(_sb, bb.buffer, 1, bb.len - 1);
                    }
                    else
                    {
                        AppendStringWithSomeEscape(_sb, bb.buffer, 0, bb.len);
                    }
                }
                if (_local_value_len > 1)
                {
                    _sb.Append(_sourceBuffer, 0, _local_value_len - 1);
                }
                return _sb.ToString();
            }
            else
            {
                //not a concat value
                _sb.Length = 0;//clear
                AppendStringWithSomeEscape(_sb, _sourceBuffer, _value_start + 1, _local_value_len - 2);
                return _sb.ToString();
            }
        }
        protected string GetValueAsString()
        {
            if (_isConcatValue)
            {
                //need to merge
                _sb.Length = 0;//clear
                for (int i = 0; i < _tmpSavedBufferList.Count; ++i)
                {
                    TempSavedBuffer bb = _tmpSavedBufferList[i];
                    if (i == 0)
                    {
                        _sb.Append(bb.buffer, 1, bb.len - 1);
                    }
                    else
                    {
                        _sb.Append(bb.buffer, 0, bb.len);
                    }
                }
                if (_local_value_len > 1)
                {
                    _sb.Append(_sourceBuffer, 0, _local_value_len - 1);
                }
                return _sb.ToString();
            }
            else
            {
                //not a concat value
                if (_local_value_len < 2)
                {

                }
                return new string(_sourceBuffer, _value_start + 1, _local_value_len - 2);
            }
        }
        protected string GetCommentAsString()
        {
            if (_isConcatValue)
            {
                //need to merge
                _sb.Length = 0;//clear
                for (int i = 0; i < _tmpSavedBufferList.Count; ++i)
                {
                    TempSavedBuffer bb = _tmpSavedBufferList[i];
                    AppendStringWithSomeEscape(_sb, bb.buffer, 0, bb.len);
                }
                if (_local_value_len > 1)
                {
                    _sb.Append(_sourceBuffer, 0, _local_value_len);
                }
                return _sb.ToString();
            }
            else
            {
                //not a concat value
                _sb.Length = 0;//clear
                AppendStringWithSomeEscape(_sb, _sourceBuffer, _value_start, _local_value_len);
                return _sb.ToString();
            }
        }
        protected double GetValueAsDouble()
        {

            if (_isConcatValue)
            {
                TempSavedBuffer temp_buffer2 = GetFreeTempBuffer(_sourceBuffer.Length);
                char[] bb = temp_buffer2.buffer;
                int totalLen = ConcatSmallValue(bb, _local_value_len);
                string dd = new string(bb, 0, totalLen);
                //int result = ParseInt32(bb, 0, totalLen);
                ReleaseFreeTempBuffer(temp_buffer2);
                //return result;
                return double.Parse(dd);
            }
            else
            {
                string dd = new string(_sourceBuffer, _value_start, _local_value_len);
                return double.Parse(dd);
            }
        }
        protected void ReadValueAsByteBuffer(System.IO.StreamWriter w)
        {
            //apply to string value only

        }

        int _value_start;
        int _local_value_len;
        bool _isConcatValue;

        protected int ConcatValueLen => _concat_value_len;
        int ConcatSmallValue(char[] tmp_buffer, int localLen)
        {
            int dstPos = 0;
            for (int i = 0; i < _tmpSavedBufferList.Count; ++i)
            {
                TempSavedBuffer tmpBuffer = _tmpSavedBufferList[i];
                Array.Copy(tmpBuffer.buffer, 0, tmp_buffer, dstPos, tmpBuffer.len);
                dstPos += tmpBuffer.len;
            }

            if (localLen > 0)
            {
                Array.Copy(_sourceBuffer, 0, tmp_buffer, dstPos, localLen);
                dstPos += localLen;
            }
            return dstPos;
        }


        protected override void NewConcatComment(int len)
        {
            _isConcatValue = true;
            _value_start = 0;
            _concat_value_len += len;
            _local_value_len = len;
            //-------


#if DEBUG
            //test
            string cmt = GetCommentAsString();
#endif
            //-------
            //clear buffer
            for (int i = 0; i < _tmpSavedBufferList.Count; ++i)
            {
                ReleaseFreeTempBuffer(_tmpSavedBufferList[i]);
            }

            _concat_value_len = 0;
            _tmpSavedBufferList.Clear();
        }
        protected override void NewComment(int start, int len)
        {
            _isConcatValue = false;
            _value_start = start;
            _local_value_len = _concat_value_len = len;
            //-------
#if DEBUG
            //test
            string cmt = GetCommentAsString();
#endif


            //-------
            _concat_value_len = 0;//reset
        }
        protected override void NewConcatValue(int len)
        {
            _isConcatValue = true;
            _value_start = 0;
            _concat_value_len += len;
            _local_value_len = len;

            switch (CollectedValueHint)
            {
                case EsValueHint.Comment:
                    throw new NotSupportedException();

                case EsValueHint.IntegerNumber:
                    {
#if DEBUG
                        if (_concat_value_len >= 9)
                        {
                            //hint to long
                            TempSavedBuffer temp_buffer2 = GetFreeTempBuffer(_sourceBuffer.Length);
                            char[] bb = temp_buffer2.buffer;
                            int int_len = ConcatSmallValue(bb, len);
                            ReleaseFreeTempBuffer(temp_buffer2);
                        }
#endif

                    }
                    break;
                case EsValueHint.Identifier:
                    {
                        //special
                        switch (_concat_value_len)
                        {
                            default:
                                {
                                    //TempSavedBuffer temp_buffer2 = GetFreeTempBuffer(_sourceBuffer.Length);
                                    //char[] bb = temp_buffer2.buffer; 
                                    //ConcatSmallValue(bb, len); 
                                    //ReleaseFreeTempBuffer(temp_buffer2);
                                }
                                break;
                            case 4://true //null 
                                {

                                    TempSavedBuffer temp_buffer2 = GetFreeTempBuffer(_sourceBuffer.Length);
                                    char[] bb = temp_buffer2.buffer;

                                    ConcatSmallValue(bb, len);
                                    if (bb[0] == 'n' &&
                                        bb[0 + 1] == 'u' &&
                                        bb[0 + 2] == 'l' &&
                                        bb[0 + 3] == 'l')
                                    {
                                        CollectedValueHint = EsValueHint.Null;
                                    }
                                    else if (bb[0] == 't' &&
                                       bb[0 + 1] == 'r' &&
                                       bb[0 + 2] == 'u' &&
                                       bb[0 + 3] == 'e')
                                    {
                                        CollectedValueHint = EsValueHint.True;
                                    }
                                    else
                                    {

                                    }

                                    ReleaseFreeTempBuffer(temp_buffer2);
                                }
                                break;
                            case 5:
                                {

                                    TempSavedBuffer temp_buffer2 = GetFreeTempBuffer(_sourceBuffer.Length);
                                    char[] bb = temp_buffer2.buffer;
                                    ConcatSmallValue(bb, len);

                                    if (bb[0] == 'f' &&
                                        bb[0 + 1] == 'a' &&
                                        bb[0 + 2] == 'l' &&
                                        bb[0 + 3] == 's' &&
                                        bb[0 + 4] == 'e')
                                    {
                                        CollectedValueHint = EsValueHint.False;
                                    }
                                    else
                                    {

                                    }

                                    ReleaseFreeTempBuffer(temp_buffer2);
                                }

                                break;
                        }
                    }
                    break;
            }


            if (!_currValue.isArr)
            {
                AddElementAttribute(_currValue.elem, _currentKey, CollectedValueHint);
            }
            else
            {
                AddArrayElement(_currValue.arr, CollectedValueHint);
            }

            //clear buffer
            for (int i = 0; i < _tmpSavedBufferList.Count; ++i)
            {
                ReleaseFreeTempBuffer(_tmpSavedBufferList[i]);
            }

            _concat_value_len = 0;
            _tmpSavedBufferList.Clear();
        }

        protected override void NewValue(int start, int len)
        {
            _isConcatValue = false;
            _value_start = start;
            _local_value_len = _concat_value_len = len;
#if DEBUG
            //if (CollectedValueHint == EsValueHint.StringLiteral && _local_value_len < 2)
            //{

            //}
#endif

            if (CollectedValueHint == EsValueHint.Identifier)
            {
                //special
                switch (len)
                {
                    default:
                        {

                        }
                        break;
                    case 3:
                        {
#if DEBUG
                            char c0 = _sourceBuffer[start];
                            char c1 = _sourceBuffer[start + 1];
                            char c2 = _sourceBuffer[start + 2];
#endif

                        }
                        break;
                    case 4://true //null 
                        if (_sourceBuffer[start] == 'n' &&
                            _sourceBuffer[start + 1] == 'u' &&
                            _sourceBuffer[start + 2] == 'l' &&
                            _sourceBuffer[start + 3] == 'l')
                        {
                            CollectedValueHint = EsValueHint.Null;
                        }
                        else if (_sourceBuffer[start] == 't' &&
                           _sourceBuffer[start + 1] == 'r' &&
                           _sourceBuffer[start + 2] == 'u' &&
                           _sourceBuffer[start + 3] == 'e')
                        {
                            CollectedValueHint = EsValueHint.True;
                        }
                        else
                        {

                        }
                        break;
                    case 5:
                        //false
                        if (_sourceBuffer[start] == 'f' &&
                            _sourceBuffer[start + 1] == 'a' &&
                            _sourceBuffer[start + 2] == 'l' &&
                            _sourceBuffer[start + 3] == 's' &&
                            _sourceBuffer[start + 4] == 'e')
                        {
                            CollectedValueHint = EsValueHint.False;
                        }
                        else
                        {

                        }
                        break;
                }
            }

            if (!_currValue.isArr)
            {
                AddElementAttribute(_currValue.elem, _currentKey, CollectedValueHint);
            }
            else
            {
                AddArrayElement(_currValue.arr, CollectedValueHint);
            }

            _concat_value_len = 0;//reset
        }



        protected override void NotifyError()
        {
            base.NotifyError();
        }

        public object CurrentValue
        {
            get
            {
                if (_currValue.isArr)
                {
                    return _currValue.arr;
                }
                else
                {
                    return _currValue.elem;
                }
            }
        }
    }
}