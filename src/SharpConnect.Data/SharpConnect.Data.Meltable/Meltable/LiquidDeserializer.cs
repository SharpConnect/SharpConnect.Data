//MIT 2015- 2016, brezza92, EngineKit and contributors
using System;
using System.IO;
using System.Text;

namespace SharpConnect.Data.Meltable
{
    public class LiquidDeserializer
    {
        BinaryReader reader;

        public void SetBinaryReader(BinaryReader reader)
        {
            this.reader = reader;
        }
        protected virtual void OnBeginObject()
        {
        }
        protected virtual void OnEndObject()
        {

        }
        protected virtual void OnBeginArray()
        {

        }
        protected virtual void OnEndArray()
        {

        }
        //----------------------------------------
        protected virtual void OnBlob(byte[] binaryBlobData)
        {

        }
        protected virtual void OnUtf8StringData(byte[] strdata)
        {

        }
        protected virtual void OnEmptyString()
        {
        }
        protected virtual void OnEmptyGuid()
        {
        }
        protected virtual void OnNullObject() { }
        protected virtual void OnNullString() { }
        protected virtual void OnBoolean(bool value) { }
        protected virtual void OnGuidData(byte[] guid)
        {
        }
        protected virtual void OnByte(byte value)
        {
        }
        protected virtual void OnSByte(sbyte value)
        {
        }
        protected virtual void OnInt16(short value)
        {
        }
        protected virtual void OnUInt16(ushort value)
        {
        }
        protected virtual void OnChar(char value)
        {
        }
        protected virtual void OnInt32(int value)
        {
        }
        protected virtual void OnUInt32(uint value)
        {
        }
        protected virtual void OnInt64(long value)
        {
        }
        protected virtual void OnUInt64(ulong value)
        {
        }
        protected virtual void OnDateTime(DateTime value)
        {
        }
        protected virtual void OnInteger(int value)
        {
        }
        protected virtual void OnFloat32(float value)
        {
        }
        protected virtual void OnFloat64(double value)
        {
        }
        protected virtual void OnDecimal(decimal value)
        {
        }
        protected virtual void OnKey()
        {
        }
        protected virtual void OnKeyValue()
        {
        }
        bool ReadObjectKey()
        {
            //---------------------
            //then read key and value
            OnKey();
            MarkerCode marker = (MarkerCode)reader.ReadByte();
            switch (marker)
            {
                case MarkerCode.EndObject:
                    //stop current object
                    return false;
                case MarkerCode.STR_1:
                    {
                        int len = reader.ReadByte();
                        OnUtf8StringData(reader.ReadBytes(len));
                    }
                    break;
                case MarkerCode.STR_2:
                    {
                        int len = reader.ReadUInt16();
                        OnUtf8StringData(reader.ReadBytes(len));
                    }
                    break;
                case MarkerCode.STR_4:
                    {
                        int len = reader.ReadInt32();
                        OnUtf8StringData(reader.ReadBytes(len));
                    }
                    break;
                case MarkerCode.EmptyString:
                    OnEmptyString();
                    break;
                case MarkerCode.GUID:
                    OnGuidData(reader.ReadBytes(8));
                    break;
                case MarkerCode.EmptyGuid:
                    OnEmptyGuid();
                    break;
                case MarkerCode.Int16:
                    OnInt16(reader.ReadInt16());
                    break;
                case MarkerCode.Int32:
                    OnInt32(reader.ReadInt32());
                    break;
                case MarkerCode.Char:
                    OnChar(reader.ReadChar());
                    break;
                case MarkerCode.UInt16:
                    OnUInt16(reader.ReadUInt16());
                    break;
                case MarkerCode.UInt32:
                    OnUInt32(reader.ReadUInt32());
                    break;
                case MarkerCode.UInt64:
                    OnUInt64(reader.ReadUInt64());
                    break;
                case MarkerCode.Int64:
                    OnInt64(reader.ReadInt64());
                    break;
                case MarkerCode.Byte:
                    OnByte(reader.ReadByte());
                    break;
                case MarkerCode.SByte:
                    OnSByte(reader.ReadSByte());
                    break;
                case MarkerCode.Num0:
                    OnInteger(0);
                    break;
                case MarkerCode.Num1:
                    OnInteger(1);
                    break;
                case MarkerCode.Num2:
                    OnInteger(2);
                    break;
                case MarkerCode.Num3:
                    OnInteger(3);
                    break;
                case MarkerCode.Num4:
                    OnInteger(4);
                    break;
                case MarkerCode.Num5:
                    OnInteger(5);
                    break;
                case MarkerCode.Num6:
                    OnInteger(6);
                    break;
                case MarkerCode.Num7:
                    OnInteger(7);
                    break;
                case MarkerCode.Num8:
                    OnInteger(8);
                    break;
                case MarkerCode.Num9:
                    OnInteger(9);
                    break;
                case MarkerCode.NumM1:
                    OnInteger(-1);
                    break;
                default:
                    throw new NotSupportedException();
            }

            OnKeyValue();
            return true;
        }
        void ReadObject(int sizeHintInByteCount)
        {
            //start obj no hint
            //then read next byte
            //may be mbcount hint
            OnBeginObject();
            while (ReadObjectKey())
            {
                MarkerCode marker;

                ReadValue(out marker);
            }
            OnEndObject();
        }
        void ReadArray(int sizeHintInByteCount)
        {
            OnBeginArray();

            for (;;)
            {
                MarkerCode marker;
                ReadValue(out marker);
                if (marker == MarkerCode.EndArray)
                {
                    break;
                }
                else
                {
                    //push read value to array
                }
            }
            OnEndArray();
        }


        protected void ReadValue(out MarkerCode marker)
        {
            //all object
            marker = (MarkerCode)reader.ReadByte();
            switch (marker)
            {
                default:
                    throw new NotSupportedException();
                case MarkerCode.StartObject:
                    ReadObject(0);
                    break;
                case MarkerCode.StartObject_1:
                    ReadObject(reader.ReadByte());
                    break;
                case MarkerCode.StartObject_2:
                    ReadObject(reader.ReadUInt16());
                    break;
                case MarkerCode.StartObject_4:
                    ReadObject(reader.ReadInt32());
                    break;
                case MarkerCode.ObjectFieldSep:
                    throw new NotSupportedException();
                case MarkerCode.EndObject:
                    break;
                //--------------------------
                case MarkerCode.StartArray:
                    ReadArray(0);
                    break;
                case MarkerCode.StartArray_1:
                    ReadArray(reader.ReadByte());
                    break;
                case MarkerCode.StartArray_2:
                    ReadArray(reader.ReadUInt16());
                    break;
                case MarkerCode.StartArray_4:
                    ReadArray(reader.ReadInt32());
                    break;
                case MarkerCode.ArrayElementSep:
                    throw new NotSupportedException();
                case MarkerCode.ArrayElementType:
                case MarkerCode.ArrayElementTypeCustom:
                    throw new Exception("should not be found here!");
                case MarkerCode.EndArray:
                    //end arr
                    break;
                //--------------------------
                case MarkerCode.Null:
                    OnNullObject();
                    break;
                case MarkerCode.NullString:
                    OnNullString();
                    break;
                case MarkerCode.MbCount1:
                case MarkerCode.MbCount2:
                case MarkerCode.MbCount4:
                    throw new Exception("should not be found here!");
                //--------------------------
                case MarkerCode.True:
                    OnBoolean(true);
                    break;
                case MarkerCode.False:
                    OnBoolean(false);
                    break;
                //--------------------------
                case MarkerCode.STR_1:
                    {
                        int len = reader.ReadByte();
                        OnUtf8StringData(reader.ReadBytes(len));
                    }
                    break;
                case MarkerCode.STR_2:
                    {
                        int len = reader.ReadUInt16();
                        OnUtf8StringData(reader.ReadBytes(len));
                    }
                    break;
                case MarkerCode.STR_4:
                    {
                        int len = reader.ReadInt32();
                        OnUtf8StringData(reader.ReadBytes(len));
                    }
                    break;
                case MarkerCode.EmptyString:
                    OnEmptyString();
                    break;
                case MarkerCode.GUID:
                    OnGuidData(reader.ReadBytes(8));
                    break;
                case MarkerCode.EmptyGuid:
                    OnEmptyGuid();
                    break;
                case MarkerCode.Int16:
                    OnInt16(reader.ReadInt16());
                    break;
                case MarkerCode.Int32:
                    OnInt32(reader.ReadInt32());
                    break;
                case MarkerCode.Char:
                    OnChar(reader.ReadChar());
                    break;
                case MarkerCode.UInt16:
                    OnUInt16(reader.ReadUInt16());
                    break;
                case MarkerCode.UInt32:
                    OnUInt32(reader.ReadUInt32());
                    break;
                case MarkerCode.UInt64:
                    OnUInt64(reader.ReadUInt64());
                    break;
                case MarkerCode.Int64:
                    OnInt64(reader.ReadInt64());
                    break;
                case MarkerCode.Byte:
                    OnByte(reader.ReadByte());
                    break;
                case MarkerCode.SByte:
                    OnSByte(reader.ReadSByte());
                    break;
                case MarkerCode.Num0:
                    OnInteger(0);
                    break;
                case MarkerCode.Num1:
                    OnInteger(1);
                    break;
                case MarkerCode.Num2:
                    OnInteger(2);
                    break;
                case MarkerCode.Num3:
                    OnInteger(3);
                    break;
                case MarkerCode.Num4:
                    OnInteger(4);
                    break;
                case MarkerCode.Num5:
                    OnInteger(5);
                    break;
                case MarkerCode.Num6:
                    OnInteger(6);
                    break;
                case MarkerCode.Num7:
                    OnInteger(7);
                    break;
                case MarkerCode.Num8:
                    OnInteger(8);
                    break;
                case MarkerCode.Num9:
                    OnInteger(9);
                    break;
                case MarkerCode.NumM1:
                    OnInteger(-1);
                    break;
                //--------------------------
                case MarkerCode.Float32:
                    OnFloat32(reader.ReadSingle());
                    break;
                case MarkerCode.Float64:
                    OnFloat64(reader.ReadDouble());
                    break;
                case MarkerCode.Decimal:
                    OnDecimal(reader.ReadDecimal());
                    break;
                case MarkerCode.DateTime:
                    DateTime d = DateTime.FromBinary(reader.ReadInt64());
                    OnDateTime(d);
                    break;
                case MarkerCode.BLOB_1:
                    {
                        int len = reader.ReadByte();
                        byte[] blob = reader.ReadBytes(len);
                        OnBlob(blob);
                    }
                    break;
                case MarkerCode.BLOB_2:
                    {
                        int len = reader.ReadUInt16();
                        byte[] blob = reader.ReadBytes(len);
                        OnBlob(blob);
                    }
                    break;
                case MarkerCode.BLOB_4:
                    {
                        int len = reader.ReadInt32();
                        byte[] blob = reader.ReadBytes(len);
                        OnBlob(blob);
                    }
                    break;
                case MarkerCode.BLOB_8:
                    throw new NotSupportedException();
                    //--------------------------
            }
        }
    }
}