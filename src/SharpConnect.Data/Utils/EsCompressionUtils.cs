//MIT, 2016-2017, MoonTrip Project
using System;
using System.IO;
using System.IO.Compression;
namespace SharpConnect.Data
{

    public static class CompressionUtils
    {
        public static byte[] GetCompressData(byte[] orgBuffer)
        {

            using (MemoryStream ms = new MemoryStream())
            using (GZipStream compressedzipStream = new GZipStream(ms, CompressionMode.Compress, true))
            {
                //Console.WriteLine("Compression");
                compressedzipStream.Write(orgBuffer, 0, orgBuffer.Length);
                // Close the stream.
                compressedzipStream.Close();
                //Console.WriteLine("Original size: {0}, Compressed size: {1}", orgBuffer.Length, ms.Length);

                // Reset the memory stream position to begin decompression.
                ms.Position = 0;
                byte[] compressedData = ms.ToArray();
                ms.Close();
                return compressedData;
            }
        }
        public static byte[] DecompressData(byte[] compressedBuffer)
        {

            using (MemoryStream ms2 = new MemoryStream())
            using (MemoryStream decompressedMs = new MemoryStream())
            using (GZipStream zipStream = new GZipStream(ms2, CompressionMode.Decompress))
            {
                ms2.Write(compressedBuffer, 0, compressedBuffer.Length);
                ms2.Position = 0;
                //Console.WriteLine("Decompression");
                // Use the ReadAllBytesFromStream to read the stream.
                int totalCount = ReadAllBytesFromStream(zipStream, decompressedMs);
                // Console.WriteLine("Decompressed {0} bytes", totalCount);
                byte[] decompressedBuffer = decompressedMs.ToArray();

                return decompressedBuffer;
            }
        }
        static int ReadAllBytesFromStream(Stream compressStream, MemoryStream outputStream)
        {
            // Use this method is used to read all bytes from a stream.
            int offset = 0;
            int totalCount = 0;
            byte[] buffer = new byte[256];

            while (true)
            {
                //read into buffer
                int bytesRead = compressStream.Read(buffer, 0, 100);
                if (bytesRead == 0)
                {
                    break;
                }
                outputStream.Write(buffer, 0, bytesRead);
                offset += bytesRead;
                totalCount += bytesRead;
            }
            return totalCount;
        }



    }
}