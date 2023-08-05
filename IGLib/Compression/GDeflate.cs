using System.IO.Compression;
using System.Runtime.InteropServices;

namespace IGLib.Compression
{
    public static class GDeflate
    {
        [DllImport("GDeflateHelper.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Decompress([In,Out][MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U8)] byte[] output, ulong outputSize, [In, Out][MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U8)] byte[] input, ulong inputSize, uint numWorkers);

        // i don't know how to use the compression func so i'm just not going to use it, need it, or include it
        /*[DllImport("GDeflateHelper.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Compress([In, Out][MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U8)] byte[] output, ref ulong outputSize, [In, Out][MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U8)] byte[] input, ulong inputSize, int compressionLevel, int flags);*/
    }
}