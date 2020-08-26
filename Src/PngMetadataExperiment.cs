using System.Diagnostics;
using System.IO;
using RT.Util.ExtensionMethods;
using RT.Util.Streams;

namespace Qoph
{
    static class PngMetadataExperiment
    {
        public static void Do()
        {
            var data = File.ReadAllBytes(@"D:\temp\metadata-test.png");
            if (!data.Subarray(data.Length - 12, 12).SequenceEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82 }))
                Debugger.Break();

            byte[] generateTextChunk(string key, string value)
            {
                using var m = new MemoryStream();
                void write32(uint val)
                {
                    m.WriteByte((byte) (val >> 24));
                    m.WriteByte((byte) ((val >> 16) & 0xff));
                    m.WriteByte((byte) ((val >> 8) & 0xff));
                    m.WriteByte((byte) (val & 0xff));
                }

                var payloadBytes = $"{key}\0{value}".ToUtf8();
                write32((uint) payloadBytes.Length);
                using var c = new CRC32Stream(m);
                var chunkName = "tEXt".ToUtf8();
                c.Write(chunkName);
                c.Write(payloadBytes);
                write32(c.CRC);
                return m.ToArray();
            }

            data = data.Insert(data.Length - 12, generateTextChunk("QUINOA", "SEQUOIA"));
            data = data.Insert(data.Length - 12, generateTextChunk("SEQUESTER", "AQUATIC"));

            File.WriteAllBytes(@"D:\temp\metadata-test-result.png", data);
        }
    }
}