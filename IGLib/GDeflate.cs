using System.Runtime.InteropServices;

namespace IGLib.Compression
{
    public static class GDeflate
    {
        [DllImport("GDeflateHelper.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool Decompress([In,Out][MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U8)] byte[] output, ulong outputSize, [In, Out][MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U8)] byte[] input, ulong inputSize, uint numWorkers);
    }
}