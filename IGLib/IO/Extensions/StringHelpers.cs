using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace IGLib.IO.Extensions
{
    static class BinaryReaderExtensions
    {
        public static string ReadNullTerminatedString(this BinaryReader reader)
        {
            byte[] sBuf = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                byte b = reader.ReadByte();
                if (b != '\0')
                {
                    sBuf[i] = b;
                }
                else
                {
                    break;
                }
            }
            return Encoding.UTF8.GetString(sBuf);
        
        }
    }
}
