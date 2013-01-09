
namespace CsvIndexer
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;

    /// <summary>
    /// The CsvIndexer program
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The Utf8 encoding
        /// </summary>
        private static readonly UTF8Encoding Encoding = new UTF8Encoding();


        /// <summary>
        /// Main entry point
        /// </summary>
        /// <param name="args">The args.</param>
        public static void Main(string[] args)
        {
            var testFile = "Apex_BB_Boys_Mites.csv";

            TestOffsets(testFile);
            
            var writer = new Indexer(testFile);
            writer.Create("TeamName", 1);
            var idx1 = writer.Seek("BISONS");

            var loader = new Indexer(testFile);

            loader.Load("TeamName");
            var idx2 = loader.Seek("BISONS");

            using (var reader = new FileStream(testFile, FileMode.Open))
            {
                if (reader.CanSeek)
                {
                    foreach (var ix in idx2)
                    {
                        var buffer = new byte[ix[1]];
                        var seek = reader.Seek(ix[0], SeekOrigin.Begin);
                        var read = reader.Read(buffer, 0, ix[1]);
                        string text = DecodeData(buffer);
                        Trace.WriteLine(text);
                    }
                }
            }
        }

        /// <summary>
        /// Tests the offsets.
        /// </summary>
        /// <param name="csv">The CSV.</param>
        private static void TestOffsets(string csv)
        {
            var offsets = new List<int>();
            using (var reader = new StreamReader(csv))
            {
                int offset = 0;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    offsets.Add(offset);
                    offset += line.Length + 2; // The 2 is for NewLine(\r\n)
                }

                offsets.Add(offset); // pick up the last one
            }

            using (var reader = new FileStream(csv, FileMode.Open))
            {
                if (reader.CanSeek)
                {
                    int i = 0;

                    while (i + 1 < offsets.Count)
                    {
                        int length = offsets[i + 1] - offsets[i];
                        var buffer = new byte[length];
                        var seek = reader.Seek(offsets[i], SeekOrigin.Begin);
                        var read = reader.Read(buffer, 0, length);
                        string text = DecodeData(buffer);
                        Trace.WriteLine(text);
                        i++;
                    }
                }
            }
        }

        /// <summary>
        /// Decodes the data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The decoded string.</returns>
        private static string DecodeData(byte[] data)
        {                        
            return Encoding.GetString(data);
        }
    }
}
