using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SharpConnect.Data;
using SharpConnect.Data.Meltable;

namespace Test01
{
    class Program
    {
        static void Main(string[] args)
        {
            TestLqDoc();
        }
        static void TestLqDoc()
        {
            LiquidDoc doc = new LiquidDoc();
            var elem = doc.CreateElement("user_info");
            doc.DocumentElement = elem;
            elem.AppendAttribute("first_name", "A");
            elem.AppendAttribute("last_name", "B");
            elem.AppendAttribute("age", 20);

            //test native array object            
            elem.AppendAttribute("memberlist1", new string[] { "x", "y", "z" });
            elem.AppendAttribute("memberlist2", new object[] { 1, "y", "z" });

            Dictionary<string, int> memberlist3 = new Dictionary<string, int>();
            memberlist3.Add("score1", 10);
            memberlist3.Add("score2", 20);
            memberlist3.Add("score3", 30);
            memberlist3.Add("score4", 40);
            elem.AppendAttribute("memberlist3", memberlist3);

            List<int> memberlist4 = new List<int>() { 1, 2, 3, 4, 5 };
            elem.AppendAttribute("memberlist4", memberlist4);


            byte[] output = null;
            using (var ms = new MemoryStream())
            {
                var ser = new LiquidSerializer();
                var binWriter = new BinaryWriter(ms);
                ser.SetBinaryWriter(binWriter);

                ser.WriteDocument(doc);

                output = ms.ToArray();
                ms.Close();
            }

            using (var ms = new MemoryStream(output))
            {
                var docDeser = new LiquidDocumentDeserializer();

                var reader = new BinaryReader(ms);
                docDeser.SetBinaryReader(reader);
                docDeser.ReadDocument();
                LiquidDoc result = docDeser.Result;
            }
        }
    }
}
