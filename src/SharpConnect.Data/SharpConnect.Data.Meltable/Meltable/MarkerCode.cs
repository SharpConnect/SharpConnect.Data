//MIT 2015, brezza92, EngineKit and contributors
using System;


namespace SharpConnect.Data.Meltable
{
    public enum MarkerCode : byte
    {
        Unknown = 0,
        //---------------------
        //primitive value type
        //value 1-32: 
        //this value can be encode with strong typed array***
        //---------------------
        Byte = 1,  //1
        SByte = 2,  //1

        True = 3, //boolean
        False = 4, //boolean 

        Int16 = 5, //2
        UInt16 = 6,//2
        Char = 7,

        Int32 = 8, //4
        UInt32 = 9, //4

        Int64 = 10, //8
        UInt64 = 11,//8

        Float32 = 12, //float , 4
        Float64 = 13, //double, 8

        DateTime = 14, //8 bytes 
        Decimal = 15, //extened, 16 bytes (128 bits) 
        GUID = 16, // 16 bytes 
        //-------------------------------------------
        CustomType = 17, //2 bytes code to external object
        //-------------------------------------------
        //18-31 is reserved

        //-------------------------------------------
        //32-49: object and array description

        StartObject = 33, //{ no hint       
        StartObject_1 = 34, //followed byte_count of this object,fit in 1 byte
        StartObject_2 = 35,//followed byte_count  of this object,fit in 2 bytes
        StartObject_4 = 36,//followed byte_count  of this object,fit in 4 bytes
        StartObject_8 = 37,//followed byte_count  of this object,fit in 8 bytes
        EndObject = 38, //}

        StartArray = 39, //[ no hint
        StartArray_1 = 40,// followed byte_count of this array,fit in 1 byte
        StartArray_2 = 41,// followed byte_count of this array,fit in 1 byte
        StartArray_4 = 42,// followed byte_count of this array,fit in 1 byte
        StartArray_8 = 43,// followed byte_count of this array,fit in 1 byte

        StartArray_T = 44,//typed array (native,custom) followed byte_count of this array,fit in 1 byte
        StartArray_T_1 = 45,
        StartArray_T_2 = 46,
        StartArray_T_4 = 47,

        EndArray = 48, //]
        Sep = 49,      //optional,field separator, for marking ,

        //-------------------------------------------
        //50-short hand values, and predefined type
        Null = 50, //null object, array, string
        NullString = 51,
        EmptyObject = 52,
        EmptyArray = 53,
        EmptyString = 54,
        EmptyChar = 55,
        EmptyGuid = 56,
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