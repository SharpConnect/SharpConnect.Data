//MIT 2015- 2016, brezza92, EngineKit and contributors
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SharpConnect.Data.Meltable
{
    public class LiquidDocumentDeserializer : LiquidDeserializer
    {

        enum ParsingState : byte
        {
            Init,
            ExpectKeyName,
            ExpectKeyValue,
            ExpectArrayValue
        }

        ParsingState _state;
        LiquidDoc _doc;

        LiquidElement _currentElement;
        LiquidArray _currentArray;
        object _currentObject;

        Stack<object> _objStack = new Stack<object>();
        Stack<ParsingState> _parseStateStack = new Stack<ParsingState>();
        Stack<string> _keyNameStack = new Stack<string>();
        string _keyName;

        public LiquidDocumentDeserializer()
        {

        }
        public void ReadDocument()
        {
            _doc = new LiquidDoc();

            //init all values
            _state = ParsingState.Init;
            _currentElement = null;
            _currentObject = null;

            _objStack.Clear();
            _parseStateStack.Clear();

            MarkerCode marker;
            ReadValue(out marker);

            _doc.DocumentElement = _currentElement;

        }
        public LiquidDoc Result
        {
            get { return this._doc; }
        }

        void PushCurrentState()
        {
            _parseStateStack.Push(_state);
            _objStack.Push(_currentObject);
            _keyNameStack.Push(_keyName);
        }
        void RestorePrevState()
        {
            _state = _parseStateStack.Pop();
            _currentObject = _objStack.Pop();
            _keyName = _keyNameStack.Pop();
        }
        protected override void OnBeginArray()
        {
            PushCurrentState();

            //enter new state : create new array, and set to current object
            _currentObject = _currentArray = _doc.CreateArray();
            //expect array value
            _state = ParsingState.ExpectArrayValue;
        }
        protected override void OnEndArray()
        {
            //end current array
            LiquidArray tmpCurrentObject = _currentArray;
            string prevKey = _keyName;

            RestorePrevState();

            switch (_state)
            {
                case ParsingState.ExpectArrayValue:
                    _currentArray = (LiquidArray)_currentObject;
                    _currentArray.AddItem(_currentArray);
                    break;
                case ParsingState.ExpectKeyValue:
                    _currentElement = (LiquidElement)_currentObject;
                    _currentElement.AppendAttribute(_keyName, _currentArray);
                    _keyName = null;
                    _state = ParsingState.ExpectKeyName; //?
                    break;
                default: throw new NotSupportedException();
            }
        }

        protected override void OnBeginObject()
        {
            //create new Object
            PushCurrentState();
            //enter new state : create new element, and set to current object
            _currentObject = _currentElement = _doc.CreateElement("");
            //expected key name
            _state = ParsingState.ExpectKeyName;
        }
        protected override void OnEndObject()
        {
            //end current object
            LiquidElement tmpCurrentObject = _currentElement;
            string prevKey = _keyName;
            RestorePrevState();
            switch (_state)
            {
                case ParsingState.Init:
                    break;
                case ParsingState.ExpectArrayValue:
                    throw new NotSupportedException();
                    break;
                case ParsingState.ExpectKeyValue:
                    _currentElement = (LiquidElement)_currentObject;
                    _currentElement.AppendAttribute(_keyName, tmpCurrentObject);

                    break;
                default: throw new NotSupportedException();
            }
        }
        protected override void OnBlob(byte[] binaryBlobData)
        {
            switch (_state)
            {
                case ParsingState.ExpectArrayValue:
                    _currentArray.AddItem(binaryBlobData);
                    break;
                case ParsingState.ExpectKeyValue:
                    AppendKeyValue(binaryBlobData);
                    break;
                default: throw new NotSupportedException();
            }
        }
        protected override void OnBoolean(bool value)
        {
            switch (_state)
            {
                case ParsingState.ExpectArrayValue:
                    _currentArray.AddItem(value);
                    break;
                case ParsingState.ExpectKeyValue:
                    AppendKeyValue(value);
                    break;
                default: throw new NotSupportedException();
            }
        }
        protected override void OnDateTime(DateTime value)
        {
            switch (_state)
            {
                case ParsingState.ExpectArrayValue:
                    _currentArray.AddItem(value);
                    break;
                case ParsingState.ExpectKeyValue:
                    AppendKeyValue(value);
                    break;
                default: throw new NotSupportedException();
            }
        }
        protected override void OnByte(byte value)
        {
            switch (_state)
            {
                case ParsingState.ExpectKeyName:
                    _keyName = value.ToString();
                    _state = ParsingState.ExpectKeyValue;
                    break;
                case ParsingState.ExpectArrayValue:
                    _currentArray.AddItem(value);
                    break;
                case ParsingState.ExpectKeyValue:
                    AppendKeyValue(value);
                    break;

                default: throw new NotSupportedException();
            }
        }
        protected override void OnChar(char value)
        {
            switch (_state)
            {
                case ParsingState.ExpectKeyName:
                    _keyName = value.ToString();
                    _state = ParsingState.ExpectKeyValue;
                    break;
                case ParsingState.ExpectArrayValue:
                    _currentArray.AddItem(value);
                    break;
                case ParsingState.ExpectKeyValue:
                    AppendKeyValue(value);
                    break;

                default: throw new NotSupportedException();
            }
        }
        protected override void OnNullObject()
        {
            switch (_state)
            {
                case ParsingState.ExpectArrayValue:
                    _currentArray.AddItem(null);
                    break;
                case ParsingState.ExpectKeyValue:
                    AppendKeyValue(null);
                    break;

                default: throw new NotSupportedException();
            }
        }
        protected override void OnNullString()
        {
            switch (_state)
            {
                case ParsingState.ExpectArrayValue:
                    _currentArray.AddItem(null);
                    break;
                case ParsingState.ExpectKeyValue:
                    AppendKeyValue(null);
                    break;

                default: throw new NotSupportedException();
            }
        }
        protected override void OnEmptyGuid()
        {
            switch (_state)
            {
                case ParsingState.ExpectKeyName:
                    _keyName = Guid.Empty.ToString();
                    _state = ParsingState.ExpectKeyValue;
                    break;
                case ParsingState.ExpectArrayValue:
                    _currentArray.AddItem(Guid.Empty);
                    break;
                case ParsingState.ExpectKeyValue:
                    AppendKeyValue(Guid.Empty);
                    break;

                default: throw new NotSupportedException();
            }
        }
        protected override void OnEmptyString()
        {
            switch (_state)
            {
                case ParsingState.ExpectKeyName:
                    _keyName = "";
                    _state = ParsingState.ExpectKeyValue;
                    break;
                case ParsingState.ExpectArrayValue:
                    _currentArray.AddItem("");
                    break;
                case ParsingState.ExpectKeyValue:
                    AppendKeyValue("");
                    break;

                default: throw new NotSupportedException();
            }
        }
        protected override void OnDecimal(decimal value)
        {
            switch (_state)
            {
                case ParsingState.ExpectArrayValue:
                    _currentArray.AddItem(value);
                    break;
                case ParsingState.ExpectKeyValue:
                    AppendKeyValue(value);
                    break;

                default: throw new NotSupportedException();
            }
        }
        protected override void OnUInt16(ushort value)
        {
            switch (_state)
            {
                case ParsingState.ExpectKeyName:
                    _keyName = value.ToString();
                    _state = ParsingState.ExpectKeyValue;
                    break;
                case ParsingState.ExpectArrayValue:
                    _currentArray.AddItem(value);
                    break;
                case ParsingState.ExpectKeyValue:
                    AppendKeyValue(value);
                    break;

                default: throw new NotSupportedException();
            }
        }
        protected override void OnFloat32(float value)
        {
            switch (_state)
            {
                case ParsingState.ExpectKeyName:
                    _keyName = value.ToString();
                    _state = ParsingState.ExpectKeyValue;
                    break;
                case ParsingState.ExpectArrayValue:
                    _currentArray.AddItem(value);
                    break;
                case ParsingState.ExpectKeyValue:
                    AppendKeyValue(value);
                    break;

                default: throw new NotSupportedException();
            }
        }
        protected override void OnInt16(short value)
        {
            switch (_state)
            {
                case ParsingState.ExpectKeyName:
                    _keyName = value.ToString();
                    _state = ParsingState.ExpectKeyValue;
                    break;
                case ParsingState.ExpectArrayValue:
                    _currentArray.AddItem(value);
                    break;
                case ParsingState.ExpectKeyValue:
                    AppendKeyValue(value);
                    break;

                default: throw new NotSupportedException();
            }
        }
        protected override void OnFloat64(double value)
        {
            switch (_state)
            {
                case ParsingState.ExpectKeyName:
                    _keyName = value.ToString();
                    _state = ParsingState.ExpectKeyValue;
                    break;
                case ParsingState.ExpectArrayValue:
                    _currentArray.AddItem(value);
                    break;
                case ParsingState.ExpectKeyValue:
                    AppendKeyValue(value);
                    break;

                default: throw new NotSupportedException();
            }
        }
        protected override void OnGuidData(byte[] guid)
        {
            switch (_state)
            {
                case ParsingState.ExpectKeyName:
                    _keyName = (new Guid(guid)).ToString();
                    _state = ParsingState.ExpectKeyValue;
                    break;
                case ParsingState.ExpectArrayValue:
                    _currentArray.AddItem(new Guid(guid));
                    break;
                case ParsingState.ExpectKeyValue:
                    AppendKeyValue(new Guid(guid));
                    break;

                default: throw new NotSupportedException();
            }
        }
        protected override void OnInt32(int value)
        {
            switch (_state)
            {
                case ParsingState.ExpectKeyName:
                    _keyName = value.ToString();
                    _state = ParsingState.ExpectKeyValue;
                    break;
                case ParsingState.ExpectArrayValue:
                    _currentArray.AddItem(value);
                    break;
                case ParsingState.ExpectKeyValue:
                    AppendKeyValue(value);
                    break;
                default: throw new NotSupportedException();
            }
        }
        protected override void OnSByte(sbyte value)
        {
            switch (_state)
            {
                case ParsingState.ExpectKeyName:
                    _keyName = value.ToString();
                    _state = ParsingState.ExpectKeyValue;
                    break;
                case ParsingState.ExpectArrayValue:
                    _currentArray.AddItem(value);
                    break;
                case ParsingState.ExpectKeyValue:
                    AppendKeyValue(value);
                    break;
                default: throw new NotSupportedException();
            }
        }
        protected override void OnInt64(long value)
        {
            switch (_state)
            {
                case ParsingState.ExpectKeyName:
                    _keyName = value.ToString();
                    _state = ParsingState.ExpectKeyValue;
                    break;
                case ParsingState.ExpectArrayValue:
                    _currentArray.AddItem(value);
                    break;
                case ParsingState.ExpectKeyValue:
                    AppendKeyValue(value);
                    break;
                default: throw new NotSupportedException();
            }
        }
        protected override void OnUInt32(uint value)
        {
            switch (_state)
            {
                case ParsingState.ExpectKeyName:
                    _keyName = value.ToString();
                    _state = ParsingState.ExpectKeyValue;
                    break;
                case ParsingState.ExpectArrayValue:
                    _currentArray.AddItem(value);
                    break;
                case ParsingState.ExpectKeyValue:
                    AppendKeyValue(value);
                    break;
                default: throw new NotSupportedException();
            }
        }
        protected override void OnInteger(int value)
        {
            switch (_state)
            {
                case ParsingState.ExpectKeyName:
                    _keyName = value.ToString();
                    _state = ParsingState.ExpectKeyValue;
                    break;
                case ParsingState.ExpectArrayValue:
                    _currentArray.AddItem(value);
                    break;
                case ParsingState.ExpectKeyValue:
                    AppendKeyValue(value);
                    break;
                default: throw new NotSupportedException();
            }
        }
        protected override void OnUInt64(ulong value)
        {
            switch (_state)
            {
                case ParsingState.ExpectKeyName:
                    _keyName = value.ToString();
                    _state = ParsingState.ExpectKeyValue;
                    break;
                case ParsingState.ExpectArrayValue:
                    _currentArray.AddItem(value);
                    break;
                case ParsingState.ExpectKeyValue:
                    AppendKeyValue(value);
                    break;
                default: throw new NotSupportedException();
            }
        }
        protected override void OnUtf8StringData(byte[] strdata)
        {
            switch (_state)
            {
                case ParsingState.ExpectKeyName:
                    _keyName = Encoding.UTF8.GetString(strdata);
                    _state = ParsingState.ExpectKeyValue;
                    break;
                case ParsingState.ExpectArrayValue:
                    _currentArray.AddItem(Encoding.UTF8.GetString(strdata));
                    break;
                case ParsingState.ExpectKeyValue:
                    AppendKeyValue(Encoding.UTF8.GetString(strdata));
                    break;
                default: throw new NotSupportedException();
            }
        }
        void AppendKeyValue(object keyValue)
        {
            _currentElement.AppendAttribute(_keyName, keyValue);
            _state = ParsingState.ExpectKeyName;
        }
    }
}