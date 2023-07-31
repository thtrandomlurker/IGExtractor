using Silk.NET.DirectStorage;

namespace IGExtractor
{
    internal class Program
    {
        static unsafe void Main(string[] args)
        {
            DStorage api = DStorage.GetApi();

            IDStorageFactory g_dsFactory = new IDStorageFactory();
            IDStorageQueue1 g_dsSystemMemoryQueue = new IDStorageQueue1();
            IDStorageQueue1 g_dsGpuQueue = new IDStorageQueue1();

            IDStorageCustomDecompressionQueue g_customDecompressionQueue = new IDStorageCustomDecompressionQueue();

            using (Stream fstream = File.OpenRead(args[0]))
            {
                using (BinaryReader reader = new BinaryReader(fstream))
                {
                    byte[] data = reader.ReadBytes((int)reader.BaseStream.Length);
                    byte[] uncDat = new byte[0x798C];
                }
            }
        }
    }
}