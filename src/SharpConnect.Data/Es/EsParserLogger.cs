//MIT, 2015-2016, brezza92, EngineKit and contributors
using System;
using System.IO;
namespace SharpConnect.Es
{
#if DEBUG
    static class dbugEsParserLogger
    {
        static FileStream dbugFs;
        static StreamWriter writer;
        public static void Init(string outputfile)
        {
            if (writer != null)
            {
                writer.Close();
                writer.Dispose();
                writer = null;
            }
            if (dbugFs != null)
            {
                dbugFs.Close();
                dbugFs = null;
            }
            //-------------------------
            dbugFs = new FileStream(outputfile, FileMode.Create);
            writer = new StreamWriter(dbugFs);
            writer.AutoFlush = true;
        }
        public static void WriteLine(string text)
        {
            writer.WriteLine(text);
        }
    }
#endif
}