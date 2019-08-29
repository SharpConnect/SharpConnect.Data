//MIT, 2015-2019, brezza92, EngineKit and contributors
using System;
using System.IO;
namespace SharpConnect.Data
{
#if DEBUG
    static class dbugEsParserLogger
    {
        static FileStream s_dbugFs;
        static StreamWriter s_writer;
        public static void Init(string outputfile)
        {
            if (s_writer != null)
            {
                s_writer.Close();
                s_writer.Dispose();
                s_writer = null;
            }
            if (s_dbugFs != null)
            {
                s_dbugFs.Close();
                s_dbugFs = null;
            }
            //-------------------------
            s_dbugFs = new FileStream(outputfile, FileMode.Create);
            s_writer = new StreamWriter(s_dbugFs);
            s_writer.AutoFlush = true;
        }
        public static void WriteLine(string text)
        {
            s_writer.WriteLine(text);
        }
    }
#endif
}