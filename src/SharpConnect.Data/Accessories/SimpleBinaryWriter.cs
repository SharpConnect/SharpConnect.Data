//This contains some code from dotnet framework reference source
//with MIT license
//-------------------------------------
//2016,MIT EngineKit
//and 
//The MIT License(MIT)
//Copyright(c) Microsoft Corporation 
//-------------------------------------

using System;
using System.Text;
using System.IO;
namespace SharpConnect.Data
{

    public class SimpleBinaryWriter
    {
        Stream outStream = null;
        byte[] buffer = new byte[16];
        public SimpleBinaryWriter(Stream stream)
        {
            this.outStream = stream;
        }

        public void Flush()
        {
            this.outStream.Flush();
        }
        public void Close()
        {

            outStream.Flush();
            outStream = null;
            buffer = null;
#if DEBUG

            if (dbug_EnableLog)
            {
                dbugClose();
            }
#endif
        }

        public void Write(byte[] buffer)
        {

#if DEBUG
            if (dbug_EnableBreak)
            {
                dbugCheckBreak();
            }
            if (dbug_EnableLog)
            {
                dbugWriteInfo(Position + " (byte[" + buffer.Length + "]:");
            }
#endif
            outStream.Write(buffer, 0, buffer.Length);
        }

        public void Write(char[] utf8Char)
        {
            Write(Encoding.UTF8.GetBytes(utf8Char));
        }
        public void Write(int data)
        {

            this.buffer[0] = (byte)data;
            this.buffer[1] = (byte)(data >> 8);
            this.buffer[2] = (byte)(data >> 16);
            this.buffer[3] = (byte)(data >> 24);

#if DEBUG
            if (dbug_EnableBreak)
            {
                dbugCheckBreak();
            }
            if (dbug_EnableLog)
            {
                dbugWriteInfo(Position + " (int):" + data);
            }
#endif

            outStream.Write(this.buffer, 0, 4);
        }
        public void Write(uint data)
        {
            this.buffer[0] = (byte)data;
            this.buffer[1] = (byte)(data >> 8);
            this.buffer[2] = (byte)(data >> 16);
            this.buffer[3] = (byte)(data >> 24);
#if DEBUG
            if (dbug_EnableBreak)
            {
                dbugCheckBreak();
            }
            if (dbug_EnableLog)
            {
                dbugWriteInfo(Position + " (uint):" + data);
            }
#endif

            outStream.Write(this.buffer, 0, 4);
        }
        public unsafe void Write(double value)
        {
            ulong num = *((ulong*)&value);
            this.buffer[0] = (byte)num;
            this.buffer[1] = (byte)(num >> 8);
            this.buffer[2] = (byte)(num >> 0x10);
            this.buffer[3] = (byte)(num >> 0x18);
            this.buffer[4] = (byte)(num >> 0x20);
            this.buffer[5] = (byte)(num >> 40);
            this.buffer[6] = (byte)(num >> 0x30);
            this.buffer[7] = (byte)(num >> 0x38);

#if DEBUG
            if (dbug_EnableBreak)
            {
                dbugCheckBreak();
            }
            if (dbug_EnableLog)
            {
                dbugWriteInfo(Position + " (double):" + value);
            }
#endif

            this.outStream.Write(this.buffer, 0, 8);
        }
        public unsafe void Write(float value)
        {
            uint num = *((uint*)&value);
            this.buffer[0] = (byte)num;
            this.buffer[1] = (byte)(num >> 8);
            this.buffer[2] = (byte)(num >> 0x10);
            this.buffer[3] = (byte)(num >> 0x18);
#if DEBUG
            if (dbug_EnableBreak)
            {
                dbugCheckBreak();
            }
            if (dbug_EnableLog)
            {
                dbugWriteInfo(Position + " (float):" + value);
            }
#endif
            this.outStream.Write(this.buffer, 0, 4);
        }
        public void Write(long data)
        {
            this.buffer[0] = (byte)data;
            this.buffer[1] = (byte)(data >> 8);
            this.buffer[2] = (byte)(data >> 16);
            this.buffer[3] = (byte)(data >> 24);
            this.buffer[4] = (byte)(data >> 32);
            this.buffer[5] = (byte)(data >> 40);
            this.buffer[6] = (byte)(data >> 48);
            this.buffer[7] = (byte)(data >> 56);
#if DEBUG
            if (dbug_EnableBreak)
            {
                dbugCheckBreak();
            }
            if (dbug_EnableLog)
            {
                dbugWriteInfo(Position + " (long):" + data);
            }
#endif
            outStream.Write(this.buffer, 0, 8);
        }
        public void Write(ulong data)
        {
            this.buffer[0] = (byte)data;
            this.buffer[1] = (byte)(data >> 8);
            this.buffer[2] = (byte)(data >> 16);
            this.buffer[3] = (byte)(data >> 24);
            this.buffer[4] = (byte)(data >> 32);
            this.buffer[5] = (byte)(data >> 40);
            this.buffer[6] = (byte)(data >> 48);
            this.buffer[7] = (byte)(data >> 56);
#if DEBUG
            if (dbug_EnableBreak)
            {
                dbugCheckBreak();
            }
            if (dbug_EnableLog)
            {
                dbugWriteInfo(Position + " (ulong):" + data);
            }
#endif
            outStream.Write(this.buffer, 0, 8);
        }
        public void Write(byte data)
        {
#if DEBUG
            if (dbug_EnableBreak)
            {
                dbugCheckBreak();
            }
            if (dbug_EnableLog)
            {
                dbugWriteInfo(Position + " (byte):" + data);
            }
#endif
            outStream.WriteByte(data);
        }
        public void Write(short data)
        {
            this.buffer[0] = (byte)data;
            this.buffer[1] = (byte)(data >> 8);
#if DEBUG
            if (dbug_EnableBreak)
            {
                dbugCheckBreak();
            }
            if (dbug_EnableLog)
            {
                dbugWriteInfo(Position + " (short):" + data);
            }
#endif
            outStream.Write(this.buffer, 0, 2);
        }
        public void Write(ushort data)
        {
            this.buffer[0] = (byte)data;
            this.buffer[1] = (byte)(data >> 8);

#if DEBUG
            if (dbug_EnableBreak)
            {
                dbugCheckBreak();
            }
            if (dbug_EnableLog)
            {
                dbugWriteInfo(Position + " (ushort):" + data);
            }
#endif

            outStream.Write(this.buffer, 0, 2);

        }



#if DEBUG


        FileStream dbug_fs;
        StreamWriter dbug_fsWriter;
        bool dbug_EnableBreak = false;
        bool dbug_EnableLog = false;

        void dbugCheckBreak()
        {
            if (dbug_EnableBreak)
            {
                //if (Position == 37)
                //{

                //}
            }
        }
        void dbugWriteInfo(string info)
        {
            if (dbug_EnableLog)
            {
                dbug_fsWriter.WriteLine(info);
                dbug_fsWriter.Flush();
            }
        }

        public void dbugInit(string dbugOutputFileName)
        {
            if (dbug_EnableLog)
            {
                if (this.outStream.Position > 0)
                {
                 
                    dbug_fs = new FileStream(dbugOutputFileName + ".w_bin_debug", FileMode.Append);
                    dbug_fsWriter = new StreamWriter(dbug_fs);
                }
                else
                {
                    dbug_fs = new FileStream(dbugOutputFileName + ".w_bin_debug", FileMode.Create);
                    dbug_fsWriter = new StreamWriter(dbug_fs);
                }

            }
        }
        void dbugClose()
        {
            if (dbug_EnableLog)
            {
                dbug_fsWriter.Close();
                dbug_fs.Close();
                dbug_fs.Dispose();
                dbug_fsWriter = null;
                dbug_fs = null;
            }

        }

#endif
        
        public long Position
        {
            get
            {
                return outStream.Position;
            }
            set
            {
                outStream.Position = value;
            }
        } 
    }

    public class SimpleBinaryReader
    {

        Stream stream = null;
        byte[] buffer = new byte[16];

        public SimpleBinaryReader(Stream stream)
        {
            this.stream = stream;
#if  DEBUG

            if (dbug_EnableLog)
            {
                dbugInit();
            }
#endif
        }
        public bool IsEndOfStream
        {
            get
            {
                return stream.Position == stream.Length;
            }
        }
        public long Position
        {
            get
            {
                return stream.Position;
            }
            set
            {
                stream.Position = value;
            }
        }
        public void Close()
        {
            this.stream = null;
            buffer = null;
        }

        public bool EndOfStream
        {
            get
            {
                return stream.Position == stream.Length;
            }
        }
        public byte ReadByte()
        {

#if DEBUG
            if (dbug_enableBreak)
            {
                dbugCheckBreakPoint();
            }
            if (dbug_EnableLog)
            {
                int b = stream.ReadByte();
                dbugWriteInfo(Position - 1 + " (byte) " + b);
                return (byte)b;
            }
            else
            {
                return (byte)stream.ReadByte();
            }
#else
            return (byte)stream.ReadByte();
#endif


        }

        public UInt32 ReadUInt32()
        {
#if DEBUG
            if (dbug_enableBreak)
            {
                dbugCheckBreakPoint();
            }
            if (dbug_EnableLog)
            {
                dbugWriteInfo(Position + " (uint32)");
            }

            byte[] mybuffer = this.buffer;
            stream.Read(mybuffer, 0, 4);


            if (dbug_EnableLog)
            {
                uint u = (uint)(mybuffer[0] | mybuffer[1] << 8 |
                    mybuffer[2] << 16 | mybuffer[3] << 24);
                dbugWriteInfo(Position - 4 + " (uint32) " + u);
                return u;
            }
            else
            {
                return (uint)(mybuffer[0] | mybuffer[1] << 8 |
                      mybuffer[2] << 16 | mybuffer[3] << 24);
            }

#else
            byte[] mybuffer = this.buffer;
            stream.Read(mybuffer, 0, 4); 
            return (uint)(mybuffer[0] | mybuffer[1] << 8 |
                mybuffer[2] << 16 | mybuffer[3] << 24);

#endif
        }

        public unsafe double ReadDouble()
        {

#if DEBUG
            if (dbug_enableBreak)
            {
                dbugCheckBreakPoint();
            }
            if (dbug_EnableLog)
            {
                dbugWriteInfo(Position + " (double)");
            }

            byte[] mybuffer = this.buffer;
            stream.Read(mybuffer, 0, 8);

            uint num = (uint)(((mybuffer[0] | (mybuffer[1] << 8)) | (mybuffer[2] << 0x10)) | (mybuffer[3] << 0x18));
            uint num2 = (uint)(((mybuffer[4] | (mybuffer[5] << 8)) | (mybuffer[6] << 0x10)) | (mybuffer[7] << 0x18));
            ulong num3 = (num2 << 0x20) | num;

            if (dbug_EnableLog)
            {

                double value = *(((double*)&num3));

                dbugWriteInfo(Position - 8 + " (double) " + value);

                return value;
            }
            else
            {

                return *(((double*)&num3));
            }
#else 


            byte[] mybuffer = this.buffer;
            stream.Read(mybuffer, 0, 8);

            uint num = (uint)(((mybuffer[0] | (mybuffer[1] << 8)) | (mybuffer[2] << 0x10)) | (mybuffer[3] << 0x18));
            uint num2 = (uint)(((mybuffer[4] | (mybuffer[5] << 8)) | (mybuffer[6] << 0x10)) | (mybuffer[7] << 0x18));
            ulong num3 = (num2 << 0x20) | num;
            return *(((double*)&num3));
#endif
        }
        public unsafe float ReadFloat()
        {

#if DEBUG


            if (dbug_enableBreak)
            {
                dbugCheckBreakPoint();
            }
            if (dbug_EnableLog)
            {
                dbugWriteInfo(Position + " (float)");
            }



            byte[] mybuffer = this.buffer;
            stream.Read(mybuffer, 0, 4);
            uint num = (uint)(((mybuffer[0] | (mybuffer[1] << 8)) | (mybuffer[2] << 0x10)) | (mybuffer[3] << 0x18));

            if (dbug_EnableLog)
            {
                float value = *(((float*)&num));
                dbugWriteInfo(Position - 4 + " (float) " + value);
                return value;
            }
            else
            {
                return *(((float*)&num));
            }


#else
            byte[] mybuffer = this.buffer;
            stream.Read(mybuffer, 0, 4);
            uint num = (uint)(((mybuffer[0] | (mybuffer[1] << 8)) | (mybuffer[2] << 0x10)) | (mybuffer[3] << 0x18));
            return *(((float*)&num));
#endif
        }
        public Int32 ReadInt32()
        {
#if DEBUG
            if (dbug_enableBreak)
            {
                dbugCheckBreakPoint();
            }


            byte[] mybuffer = this.buffer;
            stream.Read(mybuffer, 0, 4);
            if (dbug_EnableLog)
            {
                int i32 = (mybuffer[0] | mybuffer[1] << 8 |
                    mybuffer[2] << 16 | mybuffer[3] << 24);
                dbugWriteInfo(Position - 4 + " (int32) " + i32);

                return i32;
            }
            else
            {
                return (mybuffer[0] | mybuffer[1] << 8 |
                  mybuffer[2] << 16 | mybuffer[3] << 24);
            }
#else
            byte[] mybuffer = this.buffer;
            stream.Read(mybuffer, 0, 4); 
            return (mybuffer[0] | mybuffer[1] << 8 |
                mybuffer[2] << 16 | mybuffer[3] << 24);
#endif

        }
        public Int16 ReadInt16()
        {
#if DEBUG
            if (dbug_enableBreak)
            {
                dbugCheckBreakPoint();
            }
            byte[] mybuffer = this.buffer;
            stream.Read(mybuffer, 0, 2);
            if (dbug_EnableLog)
            {
                Int16 i16 = (Int16)(mybuffer[0] | mybuffer[1] << 8);
                dbugWriteInfo(Position - 2 + " (int16) " + i16);
                return i16;
            }
            else
            {
                return (Int16)(mybuffer[0] | mybuffer[1] << 8);
            }
#else 
            byte[] mybuffer = this.buffer;
            stream.Read(mybuffer, 0, 2); 
            return (Int16)(mybuffer[0] | mybuffer[1] << 8);

#endif

        }
        public UInt16 ReadUInt16()
        {
#if DEBUG
            if (dbug_enableBreak)
            {
                dbugCheckBreakPoint();
            }

            byte[] mybuffer = this.buffer;
            stream.Read(mybuffer, 0, 2);
            if (dbug_EnableLog)
            {
                UInt16 ui16 = (UInt16)(mybuffer[0] | mybuffer[1] << 8);
                dbugWriteInfo(Position - 2 + " (uint16) " + ui16);
                return ui16;
            }
            else
            {
                return (UInt16)(mybuffer[0] | mybuffer[1] << 8);
            }
#else       
            byte[] mybuffer = this.buffer;
            stream.Read(mybuffer, 0, 2); 
            return (UInt16)(mybuffer[0] | mybuffer[1] << 8);


#endif
        }
        public long ReadInt64()
        {
#if DEBUG
            if (dbug_enableBreak)
            {
                dbugCheckBreakPoint();
            }

            byte[] mybuffer = this.buffer;
            stream.Read(mybuffer, 0, 8);
            uint num = (uint)(((mybuffer[0] | (mybuffer[1] << 8)) | (mybuffer[2] << 0x10)) | (mybuffer[3] << 0x18));
            uint num2 = (uint)(((mybuffer[4] | (mybuffer[5] << 8)) | (mybuffer[6] << 0x10)) | (mybuffer[7] << 0x18));

            if (dbug_EnableLog)
            {
                long l = ((long)num2 << 0x20) | num;
                dbugWriteInfo(Position - 8 + " (int64) " + l);
                return l;
            }
            else
            {
                return ((long)num2 << 0x20) | num;
            }
#else
            byte[] mybuffer = this.buffer;
            stream.Read(mybuffer, 0, 8); 
            uint num = (uint)(((mybuffer[0] | (mybuffer[1] << 8)) | (mybuffer[2] << 0x10)) | (mybuffer[3] << 0x18));
            uint num2 = (uint)(((mybuffer[4] | (mybuffer[5] << 8)) | (mybuffer[6] << 0x10)) | (mybuffer[7] << 0x18));
            return ((long)num2 << 0x20) | num;
#endif

        }
        public UInt64 ReadUInt64()
        {
#if DEBUG
            if (dbug_enableBreak)
            {
                dbugCheckBreakPoint();
            }


            byte[] mybuffer = this.buffer;
            stream.Read(mybuffer, 0, 8);
            uint num = (uint)(((mybuffer[0] | (mybuffer[1] << 8)) | (mybuffer[2] << 0x10)) | (mybuffer[3] << 0x18));
            uint num2 = (uint)(((mybuffer[4] | (mybuffer[5] << 8)) | (mybuffer[6] << 0x10)) | (mybuffer[7] << 0x18));
            if (dbug_EnableLog)
            {
                UInt64 ui64 = ((UInt64)num2 << 0x20) | num;
                dbugWriteInfo(Position - 8 + " (uint64) " + ui64);
                return ui64;
            }
            else
            {
                return ((UInt64)num2 << 0x20) | num;
            }
#else
             byte[] mybuffer = this.buffer;
            stream.Read(mybuffer, 0, 8);
          
            uint num = (uint)(((mybuffer[0] | (mybuffer[1] << 8)) | (mybuffer[2] << 0x10)) | (mybuffer[3] << 0x18));
            uint num2 = (uint)(((mybuffer[4] | (mybuffer[5] << 8)) | (mybuffer[6] << 0x10)) | (mybuffer[7] << 0x18));
            return ((UInt64)num2 << 0x20) | num;

#endif

        }
        public char[] ReadChars(int num)
        {

            return Encoding.UTF8.GetChars(ReadBytes(num));
        }
        public byte[] ReadBytes(int num)
        {
#if DEBUG
            if (dbug_enableBreak)
            {
                dbugCheckBreakPoint();
            }

            byte[] buffer = new byte[num];
            stream.Read(buffer, 0, num);
            if (dbug_EnableLog)
            {
                dbugWriteInfo(Position - num + " (byte[" + num + "]");
                return buffer;
            }
            else
            {
                return buffer;
            }

#else
            byte[] buffer = new byte[num];
            stream.Read(buffer, 0, num);
            return buffer;
#endif

        }
        public Stream BaseStream
        {
            get
            {
                return stream;
            }
        }
#if DEBUG

        void dbugCheckBreakPoint()
        {
            if (dbug_enableBreak)
            {
                //if (Position == 35)
                //{
                //}
            }
        }

        bool dbug_EnableLog = false;
        bool dbug_enableBreak = false;
        FileStream dbug_fs;
        StreamWriter dbug_fsWriter;


        void dbugWriteInfo(string info)
        {
            if (dbug_EnableLog)
            {
                dbug_fsWriter.WriteLine(info);
                dbug_fsWriter.Flush();
            }
        }
        void dbugInit()
        {
            if (dbug_EnableLog)
            {
                if (this.stream.Position > 0)
                {

                    dbug_fs = new FileStream(((FileStream)stream).Name + ".r_bin_debug", FileMode.Append);
                    dbug_fsWriter = new StreamWriter(dbug_fs);
                }
                else
                {
                    dbug_fs = new FileStream(((FileStream)stream).Name + ".r_bin_debug", FileMode.Create);
                    dbug_fsWriter = new StreamWriter(dbug_fs);
                }

            }
        }
        void dbugClose()
        {
            if (dbug_EnableLog)
            {
                dbug_fsWriter.Close();
                dbug_fs.Close();
                dbug_fs.Dispose();
                dbug_fsWriter = null;
                dbug_fs = null;
            }

        }

#endif
    }

}