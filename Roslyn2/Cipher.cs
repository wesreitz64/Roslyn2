using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Roslyn2
{
    public static class Cipher
    {
        public static string Compress(string uncompressedString)
        {
            if (String.IsNullOrEmpty(uncompressedString))
            {
                return uncompressedString;
            }

            using (var compressedStream = new MemoryStream())
            {
                using (var uncompressedStream = new MemoryStream(Encoding.UTF8.GetBytes(uncompressedString)))
                {
                    using (var compressorStream = new DeflateStream(compressedStream, CompressionMode.Compress, true))
                    {
                        uncompressedStream.CopyTo(compressorStream);
                    }

                    return Convert.ToBase64String(compressedStream.ToArray());
                }
            }
        }
    }
}
