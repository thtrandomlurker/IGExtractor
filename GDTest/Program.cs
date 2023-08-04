using IGLib.Compression;

namespace GDTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(args[0]);
            Console.WriteLine(args[1]);

            if (args[0] == "decompress")
            {
                Console.WriteLine("Decompressing");
                using (Stream file = File.OpenRead(args[1]))
                {
                    byte[] data = new byte[file.Length];
                    file.Read(data);
                    Console.WriteLine(data[0]);

                    byte[] output = new byte[0x800000];
                    GDeflate.Decompress(output, 0x800000, data, (ulong)file.Length, 1);

                    using (Stream ofile = File.OpenWrite(args[1] + "-dec"))
                    {
                        ofile.Write(output);
                    }
                }
            }
        }
    }
}