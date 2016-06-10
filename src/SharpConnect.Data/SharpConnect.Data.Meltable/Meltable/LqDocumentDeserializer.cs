//MIT 2015- 2016, brezza92, EngineKit and contributors
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SharpConnect.Data.Meltable
{
    public class LiquidDocumentDeserializer : LiquidDeserializer
    {

        enum ParsingState
        {
            Init,
            BeginObject,
            KeyName,
            KeyValue,
            EndObject,
            BeginArray,
            ArrayValue,
            EndArray
        }

        ParsingState _state;
        LiquidDoc _doc;
        LiquidElement _currentElement;
        LiquidArray _currentArray;
        Stack<object> _objStack = new Stack<object>();
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
            _objStack.Clear();

            MarkerCode marker;
            ReadValue(out marker);

            _doc.DocumentElement = _currentElement;

        }
        public LiquidDoc Result
        {
            get { return this._doc; }
        }

        protected override void OnBeginArray()
        {
            _state = ParsingState.BeginArray;
            _objStack.Push(_currentArray);
            //create array here
            _currentArray = _doc.CreateArray();
            _state = ParsingState.ArrayValue;

        }
        protected override void OnEndArray()
        {
            switch (_state)
            {
                case ParsingState.ArrayValue:
                    _currentArray.AddItem(_currentArray);
                    _keyName = null;
                    break;
                case ParsingState.KeyValue:
                    _currentElement.AppendAttribute(_keyName, _currentArray);
                    _keyName = null;
                    break;
                default: throw new NotSupportedException();
            }

            //------------------------------------
            _state = ParsingState.EndArray;
            _currentArray = (LiquidArray)_objStack.Pop();
        }
        protected override void OnBeginObject()
        {
            //create new Object
            _state = ParsingState.BeginObject; 
            _objStack.Push(_currentElement); 
            _currentElement = _doc.CreateElement("");

        }
        protected override void OnKey()
        {
            //switch to key state
            _state = ParsingState.KeyName;
        }
        protected override void OnKeyValue()
        {
            _state = ParsingState.KeyValue;
        }
        protected override void OnEndObject()
        {
            //end current object
            switch (_state)
            {
                case ParsingState.KeyName:
                    break; 
                default: throw new NotSupportedException();
            }
            //------------------------------------
            _state = ParsingState.EndObject;
            if (_objStack.Count > 1)
            {
                _currentElement = (LiquidElement)_objStack.Pop();
            }
        }
        protected override void OnBlob(byte[] binaryBlobData)
        {
            switch (_state)
            {
                case ParsingState.ArrayValue:
                    _currentArray.AddItem(binaryBlobData);
                    break;
                case ParsingState.KeyValue:
                    _currentElement.AppendAttribute(_keyName, binaryBlobData);
                    _keyName = null;
                    break;
                default: throw new NotSupportedException();
            }
        }
        protected override void OnBoolean(bool value)
        {
            switch (_state)
            {
                case ParsingState.ArrayValue:
                    _currentArray.AddItem(value);
                    break;
                case ParsingState.KeyValue:
                    _currentElement.AppendAttribute(_keyName, value);
                    _keyName = null;
                    break;
                default: throw new NotSupportedException();
            }
        }
        protected override void OnDateTime(DateTime value)
        {
            switch (_state)
            {
                case ParsingState.ArrayValue:
                    _currentArray.AddItem(value);
                    break;
                case ParsingState.KeyValue:
                    _currentElement.AppendAttribute(_keyName, value);
                    _keyName = null;
                    break;
                default: throw new NotSupportedException();
            }
        }
        protected override void OnByte(byte value)
        {
            switch (_state)
            {
                case ParsingState.KeyName:
                    _keyName = value.ToString();
                    break;
                case ParsingState.ArrayValue:
                    _currentArray.AddItem(value);
                    break;
                case ParsingState.KeyValue:
                    _currentElement.AppendAttribute(_keyName, value);
                    _keyName = null;
                    break;

                default: throw new NotSupportedException();
            }
        }
        protected override void OnChar(char value)
        {
            switch (_state)
            {
                case ParsingState.KeyName:
                    _keyName = value.ToString();
                    break;
                case ParsingState.ArrayValue:
                    _currentArray.AddItem(value);
                    break;
                case ParsingState.KeyValue:
                    _currentElement.AppendAttribute(_keyName, value);
                    _keyName = null;
                    break;

                default: throw new NotSupportedException();
            }
        }
        protected override void OnNullObject()
        {
            switch (_state)
            {
                case ParsingState.ArrayValue:
                    _currentArray.AddItem(null);
                    break;
                case ParsingState.KeyValue:
                    _currentElement.AppendAttribute(_keyName, null);
                    _keyName = null;
                    break;

                default: throw new NotSupportedException();
            }
        }
        protected override void OnNullString()
        {
            switch (_state)
            {
                case ParsingState.ArrayValue:
                    _currentArray.AddItem(null);
                    break;
                case ParsingState.KeyValue:
                    _currentElement.AppendAttribute(_keyName, null);
                    _keyName = null;
                    break;

                default: throw new NotSupportedException();
            }
        }
        protected override void OnEmptyGuid()
        {
            switch (_state)
            {
                case ParsingState.KeyName:
                    _keyName = Guid.Empty.ToString();
                    break;
                case ParsingState.ArrayValue:
                    _currentArray.AddItem(Guid.Empty);
                    break;
                case ParsingState.KeyValue:
                    _currentElement.AppendAttribute(_keyName, Guid.Empty);
                    _keyName = null;
                    break;

                default: throw new NotSupportedException();
            }
        }
        protected override void OnEmptyString()
        {
            switch (_state)
            {
                case ParsingState.KeyName:
                    _keyName = "";
                    break;
                case ParsingState.ArrayValue:
                    _currentArray.AddItem("");
                    break;
                case ParsingState.KeyValue:
                    _currentElement.AppendAttribute(_keyName, "");
                    _keyName = null;
                    break;

                default: throw new NotSupportedException();
            }
        }
        protected override void OnDecimal(decimal value)
        {
            switch (_state)
            {
                case ParsingState.ArrayValue:
                    _currentArray.AddItem(value);
                    break;
                case ParsingState.KeyValue:
                    _currentElement.AppendAttribute(_keyName, value);
                    _keyName = null;
                    break;

                default: throw new NotSupportedException();
            }
        }
        protected override void OnUInt16(ushort value)
        {
            switch (_state)
            {
                case ParsingState.KeyName:
                    _keyName = value.ToString();
                    break;
                case ParsingState.ArrayValue:
                    _currentArray.AddItem(value);
                    break;
                case ParsingState.KeyValue:
                    _currentElement.AppendAttribute(_keyName, value);
                    _keyName = null;
                    break;

                default: throw new NotSupportedException();
            }
        }
        protected override void OnFloat32(float value)
        {
            switch (_state)
            {
                case ParsingState.KeyName:
                    _keyName = value.ToString();
                    break;
                case ParsingState.ArrayValue:
                    _currentArray.AddItem(value);
                    break;
                case ParsingState.KeyValue:
                    _currentElement.AppendAttribute(_keyName, value);
                    _keyName = null;
                    break;

                default: throw new NotSupportedException();
            }
        }
        protected override void OnInt16(short value)
        {
            switch (_state)
            {
                case ParsingState.KeyName:
                    _keyName = value.ToString();
                    break;
                case ParsingState.ArrayValue:
                    _currentArray.AddItem(value);
                    break;
                case ParsingState.KeyValue:
                    _currentElement.AppendAttribute(_keyName, value);
                    _keyName = null;
                    break;

                default: throw new NotSupportedException();
            }
        }
        protected override void OnFloat64(double value)
        {
            switch (_state)
            {
                case ParsingState.KeyName:
                    _keyName = value.ToString();
                    break;
                case ParsingState.ArrayValue:
                    _currentArray.AddItem(value);
                    break;
                case ParsingState.KeyValue:
                    _currentElement.AppendAttribute(_keyName, value);
                    _keyName = null;
                    break;

                default: throw new NotSupportedException();
            }
        }
        protected override void OnGuidData(byte[] guid)
        {
            switch (_state)
            {
                case ParsingState.KeyName:
                    _keyName = (new Guid(guid)).ToString();
                    break;
                case ParsingState.ArrayValue:
                    _currentArray.AddItem(new Guid(guid));
                    break;
                case ParsingState.KeyValue:
                    _currentElement.AppendAttribute(_keyName, new Guid(guid));
                    _keyName = null;
                    break;

                default: throw new NotSupportedException();
            }
        }
        protected override void OnInt32(int value)
        {
            switch (_state)
            {
                case ParsingState.KeyName:
                    _keyName = value.ToString();
                    break;
                case ParsingState.ArrayValue:
                    _currentArray.AddItem(value);
                    break;
                case ParsingState.KeyValue:
                    _currentElement.AppendAttribute(_keyName, value);
                    _keyName = null;
                    break;
                default: throw new NotSupportedException();
            }
        }
        protected override void OnSByte(sbyte value)
        {
            switch (_state)
            {
                case ParsingState.KeyName:
                    _keyName = value.ToString();
                    break;
                case ParsingState.ArrayValue:
                    _currentArray.AddItem(value);
                    break;
                case ParsingState.KeyValue:
                    _currentElement.AppendAttribute(_keyName, value);
                    _keyName = null;
                    break;
                default: throw new NotSupportedException();
            }
        }
        protected override void OnInt64(long value)
        {
            switch (_state)
            {
                case ParsingState.KeyName:
                    _keyName = value.ToString();
                    break;
                case ParsingState.ArrayValue:
                    _currentArray.AddItem(value);
                    break;
                case ParsingState.KeyValue:
                    _currentElement.AppendAttribute(_keyName, value);
                    _keyName = null;
                    break;
                default: throw new NotSupportedException();
            }
        }
        protected override void OnUInt32(uint value)
        {
            switch (_state)
            {
                case ParsingState.KeyName:
                    _keyName = value.ToString();
                    break;
                case ParsingState.ArrayValue:
                    _currentArray.AddItem(value);
                    break;
                case ParsingState.KeyValue:
                    _currentElement.AppendAttribute(_keyName, value);
                    _keyName = null;
                    break;
                default: throw new NotSupportedException();
            }
        }
        protected override void OnInteger(int value)
        {
            switch (_state)
            {
                case ParsingState.KeyName:
                    _keyName = value.ToString();
                    break;
                case ParsingState.ArrayValue:
                    _currentArray.AddItem(value);
                    break;
                case ParsingState.KeyValue:
                    _currentElement.AppendAttribute(_keyName, value);
                    _keyName = null;
                    break;
                default: throw new NotSupportedException();
            }
        }
        protected override void OnUInt64(ulong value)
        {
            switch (_state)
            {
                case ParsingState.KeyName:
                    _keyName = value.ToString();
                    break;
                case ParsingState.ArrayValue:
                    _currentArray.AddItem(value);
                    break;
                case ParsingState.KeyValue:
                    _currentElement.AppendAttribute(_keyName, value);
                    _keyName = null;
                    break;
                default: throw new NotSupportedException();
            }
        }
        protected override void OnUtf8StringData(byte[] strdata)
        {
            switch (_state)
            {
                case ParsingState.KeyName:
                    _keyName = Encoding.UTF8.GetString(strdata);
                    break;
                case ParsingState.ArrayValue:

                    _currentArray.AddItem(Encoding.UTF8.GetString(strdata));
                    break;
                case ParsingState.KeyValue:
                    _currentElement.AppendAttribute(_keyName, Encoding.UTF8.GetString(strdata));
                    _keyName = null;
                    break;
                default: throw new NotSupportedException();
            }
        }
    }
}