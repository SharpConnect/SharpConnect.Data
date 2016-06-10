//MIT 2015- 2016, brezza92, EngineKit and contributors
using System;
using System.IO;
using System.Text;

namespace SharpConnect.Data.Meltable
{
    public class LiquidSerializer
    {
        BinaryWriter writer;
        Encoding enc = Encoding.UTF8;
        public void SetBinaryWriter(BinaryWriter writer)
        {
            this.writer = writer;
        }
        public void WriteDocument(LiquidDoc doc)
        {
            //write liquid document
            LiquidElement docRoot = doc.DocumentElement;
            WriteElement(docRoot);
        }
        public void WriteElement(LiquidElement elem)
        {
            //object marker         
            WriteStartObject();
            //write number of child 
            foreach (LiquidAttribute attr in elem.GetAttributeIterForward())
            {
                //key
                WriteString(attr.Name);
                //value
                WriteObject(attr.Value);
            }
            //--------
            //special element
            int j = elem.ChildCount;
            //this is special member name
            if (j > 0)
            {

            }
            //--------
            WriteEndObject();
        }
        public void WriteArray(LiquidArray arr)
        {
            WriteStartArray();
            int count = arr.Count;
            for (int i = 0; i < count; ++i)
            {
                WriteObject(arr[i]);
            }
            WriteEndArray();
        }
        void WriteStartObject()
        {
            writer.Write((byte)MarkerCode.StartObject);
        }
        long WriteStartObject_1(byte objLen = 0)
        {
            writer.Write((byte)MarkerCode.StartObject_1);
            long pos = writer.BaseStream.Position;
            writer.Write(objLen); //write blank byte
            return pos;
        }
        long WriteStartObject_2(ushort objLen = 0)
        {
            writer.Write((byte)MarkerCode.StartObject_2);
            long pos = writer.BaseStream.Position;
            writer.Write(objLen); //write blank, 
            return pos;
        }
        long WriteStartObject_4(int objLen = 0)
        {
            writer.Write((byte)MarkerCode.StartObject_4);
            long pos = writer.BaseStream.Position;
            writer.Write(objLen); //write blank, 
            return pos;
        }
        void WriteEndObject()
        {
            writer.Write((byte)MarkerCode.EndObject);
        }
        //------------------------------------------------------------
        void WriteStartArray()
        {
            writer.Write((byte)MarkerCode.StartArray);
        }
        long WriteStartArray_1(byte byteCount = 0)
        {
            writer.Write((byte)MarkerCode.StartArray_1);
            long pos = writer.BaseStream.Position;
            writer.Write(byteCount);
            return pos;
        }
        long WriteStartArray_2(ushort byteCount = 0)
        {
            writer.Write((byte)MarkerCode.StartArray_2);
            long pos = writer.BaseStream.Position;
            writer.Write(byteCount);
            return pos;
        }
        long WriteStartArray_4(int byteCount = 0)
        {
            writer.Write((byte)MarkerCode.StartArray_4);
            long pos = writer.BaseStream.Position;
            writer.Write(byteCount);
            return pos;
        }
        void WriteEndArray()
        {
            writer.Write((byte)MarkerCode.EndArray);
        }
        //------------------------------------------------------------

        void WriteString(string str)
        {
            if (str == null)
            {
                writer.Write((byte)MarkerCode.NullString);
                return;
            }
            //--------------------------------------------------
            if (str == "")
            {
                //empty string 
                writer.Write((byte)MarkerCode.EmptyString);
                return;
            }
            //--------------------------------------------------
            byte[] buffer = enc.GetBytes(str.ToCharArray());
            //switch proper string byte count
            int len = buffer.Length;
            if (len < 256)
            {
                writer.Write((byte)MarkerCode.STR_1);
                writer.Write((byte)len);
            }
            else if (len < ushort.MaxValue)
            {
                writer.Write((byte)MarkerCode.STR_2);
                writer.Write((ushort)len);
            }
            else
            {
                //use 4 byte
                writer.Write((byte)MarkerCode.STR_4);
                writer.Write(len);
            }
            writer.Write(buffer);
            //--------------------------------------------------
        }
        void WriteBlob(byte[] buffer)
        {
            if (buffer == null)
            {
                writer.Write((byte)MarkerCode.Null);
                return;
            }

            int len = buffer.Length;
            if (len < 256)
            {
                writer.Write((byte)MarkerCode.BLOB_1);
                writer.Write((byte)len);
            }
            else if (len < ushort.MaxValue)
            {
                writer.Write((byte)MarkerCode.BLOB_2);
                writer.Write((ushort)len);
            }
            else
            {
                //use 4 byte
                writer.Write((byte)MarkerCode.BLOB_4);
                writer.Write(len);
            }
            writer.Write(buffer);
            //--------------------------------------------------
        }

        void WriteByte(byte b)
        {
            writer.Write((byte)MarkerCode.Byte);
            writer.Write(b);
        }
        void WriteChar(char c)
        {
            writer.Write((byte)MarkerCode.Char);
            writer.Write(c);
        }
        void WriteInt16(short number)
        {
            writer.Write((byte)MarkerCode.Int16);
            writer.Write(number);
        }
        void WriteUInt16(ushort number)
        {
            writer.Write((byte)MarkerCode.UInt16);
            writer.Write(number);
        }
        void WriteInt32(int number)
        {
            writer.Write((byte)MarkerCode.Int32);
            writer.Write(number);
        }
        void WriteUInt32(uint number)
        {
            writer.Write((byte)MarkerCode.UInt32);
            writer.Write(number);
        }
        void WriteInt64(long number)
        {
            writer.Write((byte)MarkerCode.Int64);
            writer.Write(number);
        }
        void WriteUInt64(ulong number)
        {
            writer.Write((byte)MarkerCode.UInt64);
            writer.Write(number);
        }

        void WriteFloat32(float number)
        {
            writer.Write((byte)MarkerCode.Float32);
            writer.Write(number);
        }
        void WriteFloat64(double number)
        {
            writer.Write((byte)MarkerCode.Float64);
            writer.Write(number);
        }
        void WriteDateTime(DateTime d)
        {
            if (d == DateTime.MinValue)
            {
                writer.Write((byte)MarkerCode.DateTimeMin);
            }
            else
            {
                writer.Write((byte)MarkerCode.DateTime);
                writer.Write(d.ToBinary());
            }
        }
        void WriteGuid(Guid guid)
        {
            if (guid == Guid.Empty)
            {
                writer.Write((byte)MarkerCode.EmptyGuid);
            }
            else
            {
                writer.Write((byte)MarkerCode.GUID);
                writer.Write(guid.ToByteArray());
            }
        }
        //------------------------------------------------------------
        void WriteNull()
        {
            writer.Write((byte)MarkerCode.Null);
        }
        void WriteBool(bool value)
        {
            if (value)
            {
                writer.Write((byte)MarkerCode.True);
            }
            else
            {
                writer.Write((byte)MarkerCode.False);
            }
        }
        void WriteDecimal(decimal value)
        {
            writer.Write((byte)MarkerCode.Decimal);
            writer.Write(value);
        }
        void WriteObject(object o)
        {
            if (o == null)
            {
                WriteNull();
                return;
            }
            //----------------------------
            //write value
            var elem = o as LiquidElement;
            if (elem != null)
            {
                WriteElement(elem);
                return;
            }
            var arr = o as LiquidArray;
            if (arr != null)
            {
                WriteArray(arr);
                return;
            }


            //----------------------------
            Type otype = o.GetType();
            TypeCode tcode = Type.GetTypeCode(otype);
            switch (tcode)
            {
                case TypeCode.Boolean:
                    WriteBool((bool)o);
                    return;
                case TypeCode.Byte:
                    WriteByte((byte)o);
                    return;
                case TypeCode.Char:
                    WriteChar((char)o);
                    return;
                case TypeCode.DateTime:
                    WriteDateTime((DateTime)o);
                    return;
                case TypeCode.DBNull:
                    throw new NotSupportedException();
                case TypeCode.Decimal:
                    WriteDecimal((decimal)o);
                    return;
                case TypeCode.Double:
                    WriteFloat64((double)o);
                    return;
                case TypeCode.Empty:
                    throw new NotSupportedException();
                    return;
                case TypeCode.Int16:
                    WriteInt16((short)o);
                    return;
                case TypeCode.Int32:
                    WriteInt32((int)o);
                    return;
                case TypeCode.Int64:
                    WriteInt64((long)o);
                    return;
                case TypeCode.Object:
                    //other object
                    break;
                case TypeCode.SByte:
                    writer.Write((byte)MarkerCode.SByte);
                    writer.Write((sbyte)o);
                    return;
                case TypeCode.Single:
                    WriteFloat32((float)o);
                    return;
                case TypeCode.String:
                    WriteString((string)o);
                    return;
                case TypeCode.UInt16:
                    WriteUInt16((ushort)o);
                    return;
                case TypeCode.UInt32:
                    WriteUInt32((uint)o);
                    return;
                case TypeCode.UInt64:
                    WriteUInt64((ulong)o);
                    return;
                default:
                    throw new NotSupportedException();
            }
            //----------------------------
            //other object
            //1. wellknown object

            if (o is Array)
            {
                var nativeArray = (Array)o;
                //get element type
                int len = nativeArray.Length;
                WriteStartArray();
                foreach (var e in nativeArray)
                {
                    WriteObject(e);
                }
                WriteEndArray();
            }
            else
            {
                //native dic

            }

        }

    }
}