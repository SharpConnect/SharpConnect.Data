//MIT 2015, brezza92, EngineKit and contributors
using System;


namespace SharpConnect.Data.Meltable
{
    public enum MarkerCode : byte
    {
        Unknown = 0,

        StartObject = 15, //{ no hint       
        StartObject_1 = 16, //followed byte_count of this object,fit in 1 byte
        StartObject_2 = 17,//followed byte_count  of this object,fit in 2 bytes
        StartObject_4 = 18,//followed byte_count  of this object,fit in 2 bytes
        ObjectFieldSep = 19, //optional,field separator, for marking ,

        EndObject = 20, //}

        StartArray = 21, //[ no hint
        StartArray_1 = 22,// followed byte_count of this array,fit in 1 byte
        StartArray_2 = 23,// followed byte_count of this array,fit in 1 byte
        StartArray_4 = 24,// followed byte_count of this array,fit in 1 byte
        ArrayElementSep = 25, //optional,

        ArrayElementType = 26, //followed by 1 byte of native data type, eg. (boolean, number, datetime, string etc, except array type or object type)
        ArrayElementTypeCustom = 27, //followed by 2 byte of external user define type 

        EndArray = 28, //]

        //used with object or array, just for hint, optional
        MbCount1 = 29,//member count fit in 1 bytes
        MbCount2 = 30,//member count fit in 2 bytes
        MbCount4 = 31,//member count fit in 4 bytes

        Null = 32, //null object, array, string
        NullString = 33,
         

        True = 36, //boolean
        False = 37, //boolean 

        //number
        Byte = 38,  //1
        SByte = 39,  //1

        Int16 = 40, //2
        UInt16 = 41,//2
        Char = 42,

        Int32 = 43, //4
        UInt32 = 44, //4

        Int64 = 45, //8
        UInt64 = 46,//8

        Float32 = 47, //float , 4
        Float64 = 48, //double, 8

        DateTime = 49, //8 bytes 
        Decimal = 50, //extened, 16 bytes (128 bits)

        GUID = 51, // 16 bytes 
        //----------------
        //short hand values
        EmptyObject = 52,
        EmptyArray = 53,
        EmptyString = 54,
        EmptyChar = 55,
        EmptyGuid = 56,
        //-----------------
        DateTimeMin = 57,//0001-01-01 : 00:00:00
        //----------------
        Num0 = 60,
        Num1 = 61,
        Num2 = 62,
        Num3 = 63,
        Num4 = 64,
        Num5 = 65,
        Num6 = 66,
        Num7 = 67,
        Num8 = 68,
        Num9 = 69,
        NumM1 = 70, //-1, minus 1 
        //----------------
        //string, utf8 string
        STR_1 = 71, //str with length value fit in 1 byte 
        STR_2 = 72, //str with length value fit in 2 bytes ,unsigned
        STR_4 = 73, //str with length value fit in 4 bytes, signed 32
        //----------------
        //blob
        BLOB_1 = 74,  //blob with length value fit in 1 byte 
        BLOB_2 = 75,  //blob with length value fit in 2 bytes, unsigned 
        BLOB_4 = 76, //blob with length value fit in 4 bytes,signed
        BLOB_8 = 77, //blob with length value fit in 8 bytes  
        //---------------- 
        //system provide value from 0-127 *** (reserved)
        //---------------- 

    }
}