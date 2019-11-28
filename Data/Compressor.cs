using System.IO.Compression;
using System.IO;

namespace MySermonsWPF.Data
{
    public static class Compressor
    {
        /// <summary>
        /// Compress a byte array. Null checking enabled.
        /// </summary>
        /// <param name="input">Input bytes</param>
        /// <returns></returns>
        public static byte[] Compress(byte[] input)
        {
            using (MemoryStream outputStream = new MemoryStream())
            {
                using (GZipStream zipStream = new GZipStream(outputStream, CompressionMode.Compress))
                {
                    zipStream.Write(input, 0, input.Length);
                    zipStream.Close();
                    return outputStream.ToArray();
                }
            }
        }
        /// <summary>
        /// Decompress a byte array. Null checking enabled.
        /// </summary>
        /// <param name="input">Input bytes</param>
        /// <returns></returns>
        public static byte[] Decompress(byte[] input)
        {
            using (MemoryStream inputStream = new MemoryStream(input))
            {
                using (GZipStream zipStream = new GZipStream(inputStream, CompressionMode.Decompress))
                {
                    using (MemoryStream outputStream = new MemoryStream())
                    {
                        zipStream.CopyTo(outputStream);
                        return outputStream.ToArray();
                    }
                }
            }
        }
    }
}
