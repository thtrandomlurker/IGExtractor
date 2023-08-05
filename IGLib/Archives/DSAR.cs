using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using IGLib.Compression;
using IGLib.IO.Extensions;
using K4os.Compression.LZ4;
using K4os.Compression.LZ4.Encoders;

namespace IGLib.Archives
{
    public enum DSARCompressionMethod : byte
    {
        GDeflate = 2,
        LZ4 = 3
    }
    public class DSARChunk
    {
        public long DecompressedDataPosition { get; set; }
        public long CompressedDataOffset { get; set; }
        public int DecompressedDataLength { get; set; }
        public int CompressedDataLength { get; set; }
        public DSARCompressionMethod CompressionMethod { get; set; }

        public void Read(BinaryReader reader)
        {
            DecompressedDataPosition = reader.ReadInt64();
            CompressedDataOffset = reader.ReadInt64();
            DecompressedDataLength = reader.ReadInt32();
            CompressedDataLength = reader.ReadInt32();
            CompressionMethod = (DSARCompressionMethod)reader.ReadByte();
            // Padding
            reader.BaseStream.Seek(7, SeekOrigin.Current);
        }
    }

    /// <summary>
    /// Class for reading from (D)irect (S)torage (AR)chives.
    /// </summary>
    public class DSARReader
    {
        public short VersionMinor { get; }
        public short VersionMajor { get; }
        public string GetVersion => $"{VersionMajor}.{VersionMinor}";
        public long DecompressedSize { get; }
        public long Position { get; set; } = 0;
        private List<DSARChunk> mDataChunks;
        private BinaryReader mBaseStream;
        private byte[] mActiveBuffer;
        /// <summary>
        /// Active buffer contents
        /// </summary>
        private int mActiveBufferIndex;
        /// <summary>
        /// Index of the currently active buffer
        /// </summary>
        private int mActiveBufferPosition;
        /// <summary>
        /// Size of the currently active buffer
        /// </summary>
        private int mActiveBufferSize;
        /// <summary>
        /// Used when reading between buffers
        /// </summary>
        private long mDistanceToNextBuffer;

        public string ReadNullTerminatedString()
        {
            byte[] sBuf = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                byte b = ReadByte();
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

        public byte[] ReadBytes(int count)
        {
            byte[] oBuf = new byte[count];
            for (int i = 0; i < count; i++)
            {
                if (Position == DecompressedSize)
                {
                    throw new EndOfStreamException("Unable to read beyond the end of the stream.");
                }
                if (mDistanceToNextBuffer != 0)
                {
                    oBuf[i] = 0;
                    mDistanceToNextBuffer--;
                    Position++;
                    if (mDistanceToNextBuffer == 0)
                    {
                        AdvanceBuffer();
                    }
                }
                else
                {
                    if (mActiveBufferPosition >= mActiveBufferSize)
                    {
                        AdvanceBuffer();
                    }
                    oBuf[i] = mActiveBuffer[mActiveBufferPosition];
                    mActiveBufferPosition++;
                    Position++;
                }
            }
            return oBuf;
        }

        public sbyte ReadSByte()
        {
            sbyte ret;
            if (Position == DecompressedSize)
            {
                throw new EndOfStreamException("Unable to read beyond the end of the stream.");
            }
            if (mDistanceToNextBuffer != 0)
            {
                ret = 0;
                mDistanceToNextBuffer--;
                Position++;
                if (mDistanceToNextBuffer == 0)
                {
                    AdvanceBuffer();
                }
            }
            else
            {
                if (mActiveBufferPosition >= mActiveBufferSize)
                {
                    AdvanceBuffer();
                }
                ret = (sbyte)mActiveBuffer[mActiveBufferPosition];
                mActiveBufferPosition++;
                Position++;
            }
            return ret;
        }

        public byte ReadByte()
        {
            byte ret;
            if (Position == DecompressedSize)
            {
                throw new EndOfStreamException("Unable to read beyond the end of the stream.");
            }
            if (mDistanceToNextBuffer != 0)
            {
                ret = 0;
                mDistanceToNextBuffer--;
                Position++;
                if (mDistanceToNextBuffer == 0)
                {
                    AdvanceBuffer();
                }
            }
            else
            {
                if (mActiveBufferPosition >= mActiveBufferSize)
                {
                    AdvanceBuffer();
                }
                ret = mActiveBuffer[mActiveBufferPosition];
                mActiveBufferPosition++;
                Position++;
            }
            return ret;
        }

        public short ReadInt16()
        {
            byte[] oBuf = new byte[2];
            for (int i = 0; i < 2; i++)
            {
                if (Position == DecompressedSize)
                {
                    throw new EndOfStreamException("Unable to read beyond the end of the stream.");
                }
                if (mDistanceToNextBuffer != 0)
                {
                    oBuf[i] = 0;
                    mDistanceToNextBuffer--;
                    Position++;
                    if (mDistanceToNextBuffer == 0)
                    {
                        AdvanceBuffer();
                    }
                }
                else
                {
                    if (mActiveBufferPosition >= mActiveBufferSize)
                    {
                        AdvanceBuffer();
                    }
                    oBuf[i] = mActiveBuffer[mActiveBufferPosition];
                    mActiveBufferPosition++;
                    Position++;
                }
            }
            return BitConverter.ToInt16(oBuf);
        }

        public ushort ReadUInt16()
        {
            byte[] oBuf = new byte[2];
            for (int i = 0; i < 2; i++)
            {
                if (Position == DecompressedSize)
                {
                    throw new EndOfStreamException("Unable to read beyond the end of the stream.");
                }
                if (mDistanceToNextBuffer != 0)
                {
                    oBuf[i] = 0;
                    mDistanceToNextBuffer--;
                    Position++;
                    if (mDistanceToNextBuffer == 0)
                    {
                        AdvanceBuffer();
                    }
                }
                else
                {
                    if (mActiveBufferPosition >= mActiveBufferSize)
                    {
                        AdvanceBuffer();
                    }
                    oBuf[i] = mActiveBuffer[mActiveBufferPosition];
                    mActiveBufferPosition++;
                    Position++;
                }
            }
            return BitConverter.ToUInt16(oBuf);
        }

        public int ReadInt32()
        {
            byte[] oBuf = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                if (Position == DecompressedSize)
                {
                    throw new EndOfStreamException("Unable to read beyond the end of the stream.");
                }
                if (mDistanceToNextBuffer != 0)
                {
                    oBuf[i] = 0;
                    mDistanceToNextBuffer--;
                    Position++;
                    if (mDistanceToNextBuffer == 0)
                    {
                        AdvanceBuffer();
                    }
                }
                else
                {
                    if (mActiveBufferPosition >= mActiveBufferSize)
                    {
                        AdvanceBuffer();
                    }
                    oBuf[i] = mActiveBuffer[mActiveBufferPosition];
                    mActiveBufferPosition++;
                    Position++;
                }
            }
            return BitConverter.ToInt32(oBuf);
        }

        public uint ReadUInt32()
        {
            byte[] oBuf = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                if (Position == DecompressedSize)
                {
                    throw new EndOfStreamException("Unable to read beyond the end of the stream.");
                }
                if (mDistanceToNextBuffer != 0)
                {
                    oBuf[i] = 0;
                    mDistanceToNextBuffer--;
                    Position++;
                    if (mDistanceToNextBuffer == 0)
                    {
                        AdvanceBuffer();
                    }
                }
                else
                {
                    if (mActiveBufferPosition >= mActiveBufferSize)
                    {
                        AdvanceBuffer();
                    }
                    oBuf[i] = mActiveBuffer[mActiveBufferPosition];
                    mActiveBufferPosition++;
                    Position++;
                }
            }
            return BitConverter.ToUInt32(oBuf);
        }

        public long ReadInt64()
        {
            byte[] oBuf = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                if (Position == DecompressedSize)
                {
                    throw new EndOfStreamException("Unable to read beyond the end of the stream.");
                }
                if (mDistanceToNextBuffer != 0)
                {
                    oBuf[i] = 0;
                    mDistanceToNextBuffer--;
                    Position++;
                    if (mDistanceToNextBuffer == 0)
                    {
                        AdvanceBuffer();
                    }
                }
                else
                {
                    if (mActiveBufferPosition >= mActiveBufferSize)
                    {
                        AdvanceBuffer();
                    }
                    oBuf[i] = mActiveBuffer[mActiveBufferPosition];
                    mActiveBufferPosition++;
                    Position++;
                }
            }
            return BitConverter.ToInt64(oBuf);
        }

        public ulong ReadUInt64()
        {
            byte[] oBuf = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                if (Position == DecompressedSize)
                {
                    throw new EndOfStreamException("Unable to read beyond the end of the stream.");
                }
                if (mDistanceToNextBuffer != 0)
                {
                    oBuf[i] = 0;
                    mDistanceToNextBuffer--;
                    Position++;
                    if (mDistanceToNextBuffer == 0)
                    {
                        AdvanceBuffer();
                    }
                }
                else
                {
                    if (mActiveBufferPosition >= mActiveBufferSize)
                    {
                        AdvanceBuffer();
                    }
                    oBuf[i] = mActiveBuffer[mActiveBufferPosition];
                    mActiveBufferPosition++;
                    Position++;
                }
            }
            return BitConverter.ToUInt64(oBuf);
        }

        public float ReadSingle()
        {
            byte[] oBuf = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                if (Position == DecompressedSize)
                {
                    throw new EndOfStreamException("Unable to read beyond the end of the stream.");
                }
                if (mDistanceToNextBuffer != 0)
                {
                    oBuf[i] = 0;
                    mDistanceToNextBuffer--;
                    Position++;
                    if (mDistanceToNextBuffer == 0)
                    {
                        AdvanceBuffer();
                    }
                }
                else
                {
                    if (mActiveBufferPosition >= mActiveBufferSize)
                    {
                        AdvanceBuffer();
                    }
                    oBuf[i] = mActiveBuffer[mActiveBufferPosition];
                    mActiveBufferPosition++;
                    Position++;
                }
            }
            return BitConverter.ToSingle(oBuf);
        }
        public double ReadDouble()
        {
            byte[] oBuf = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                if (Position == DecompressedSize)
                {
                    throw new EndOfStreamException("Unable to read beyond the end of the stream.");
                }
                if (mDistanceToNextBuffer != 0)
                {
                    oBuf[i] = 0;
                    mDistanceToNextBuffer--;
                    Position++;
                    if (mDistanceToNextBuffer == 0)
                    {
                        AdvanceBuffer();
                    }
                }
                else
                {
                    if (mActiveBufferPosition >= mActiveBufferSize)
                    {
                        AdvanceBuffer();
                    }
                    oBuf[i] = mActiveBuffer[mActiveBufferPosition];
                    mActiveBufferPosition++;
                    Position++;
                }
            }
            return BitConverter.ToDouble(oBuf);
        }

        public void DumpUncompressedArchive(string outPath)
        {
            using (Stream oFile = File.Create(outPath))
            {
                Seek(0, SeekOrigin.Begin);
                for (int i = 0; i < mDataChunks.Count; i++)
                {

                    oFile.Write(mActiveBuffer);
                    Position += mActiveBufferSize;
                    if (i != mDataChunks.Count - 1)
                    {
                        if (mDataChunks[i + 1].DecompressedDataPosition > Position)
                        {
                            byte[] padBuf = new byte[mDataChunks[i + 1].DecompressedDataPosition - Position];
                            oFile.Write(padBuf);
                            Position = mDataChunks[i + 1].DecompressedDataPosition;
                        }
                    }
                    if (i != mDataChunks.Count - 1)
                    {
                        AdvanceBuffer();
                    }
                }
            }
        }

        public long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    GetBuffer(Position);
                    break;
                case SeekOrigin.Current:
                    Position += offset;
                    GetBuffer(Position);
                    break;
                case SeekOrigin.End:
                    Position = DecompressedSize + offset;
                    GetBuffer(Position);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported seek mode {origin}");
            }
            return Position;
        }
        /// <summary>
        /// Gets the next buffer in line from the active buffer
        /// </summary>
        private void AdvanceBuffer()
        {
            // get the buffer
            mActiveBufferIndex++;
            if (mDataChunks[mActiveBufferIndex].DecompressedDataPosition > Position)
            {
                mDistanceToNextBuffer = mDataChunks[mActiveBufferIndex].DecompressedDataPosition - Position;
                return;
            }
            mBaseStream.BaseStream.Seek(mDataChunks[mActiveBufferIndex].CompressedDataOffset, SeekOrigin.Begin);
            byte[] tCompressedBuffer = mBaseStream.ReadBytes(mDataChunks[mActiveBufferIndex].CompressedDataLength);
            mActiveBuffer = new byte[mDataChunks[mActiveBufferIndex].DecompressedDataLength];
            mActiveBufferSize = mDataChunks[mActiveBufferIndex].DecompressedDataLength;
            if (mDataChunks[mActiveBufferIndex].CompressionMethod == DSARCompressionMethod.LZ4)
                LZ4Codec.Decode(tCompressedBuffer, 0, mDataChunks[mActiveBufferIndex].CompressedDataLength, mActiveBuffer, 0, mDataChunks[mActiveBufferIndex].DecompressedDataLength);
            else if (mDataChunks[mActiveBufferIndex].CompressionMethod == DSARCompressionMethod.GDeflate)
                GDeflate.Decompress(mActiveBuffer, (ulong)mDataChunks[mActiveBufferIndex].DecompressedDataLength, tCompressedBuffer, (ulong)mDataChunks[mActiveBufferIndex].CompressedDataLength, 1);
            mActiveBufferPosition = 0;
        }

        /// <summary>
        /// Gets the buffer whose data is within the specified offset
        /// </summary>
        /// <param name="offset">Decompressed data position to get the buffer of</param>
        private void GetBuffer(long offset)
        {
            for (int i = 0; i < mDataChunks.Count; i++)
            {
                if ((offset < (mDataChunks[i].DecompressedDataPosition + mDataChunks[i].DecompressedDataLength)) && (offset >= mDataChunks[i].DecompressedDataPosition))
                {
                    mActiveBufferSize = mDataChunks[i].DecompressedDataLength;
                    mActiveBufferPosition = (int)(Position - mDataChunks[i].DecompressedDataPosition);
                    // only do anything if the buffer is actually changed
                    if (i != mActiveBufferIndex)
                    {
                        mActiveBufferIndex = i;

                        // get buffer
                        mBaseStream.BaseStream.Seek(mDataChunks[mActiveBufferIndex].CompressedDataOffset, SeekOrigin.Begin);
                        byte[] tCompressedBuffer = mBaseStream.ReadBytes(mDataChunks[mActiveBufferIndex].CompressedDataLength);
                        mActiveBuffer = new byte[mDataChunks[mActiveBufferIndex].DecompressedDataLength];
                        mActiveBufferSize = mDataChunks[mActiveBufferIndex].DecompressedDataLength;
                        if (mDataChunks[mActiveBufferIndex].CompressionMethod == DSARCompressionMethod.LZ4)
                            LZ4Codec.Decode(tCompressedBuffer, 0, mDataChunks[mActiveBufferIndex].CompressedDataLength, mActiveBuffer, 0, mDataChunks[mActiveBufferIndex].DecompressedDataLength);
                        else if (mDataChunks[mActiveBufferIndex].CompressionMethod == DSARCompressionMethod.GDeflate)
                            GDeflate.Decompress(mActiveBuffer, (ulong)mDataChunks[mActiveBufferIndex].DecompressedDataLength, tCompressedBuffer, (ulong)mDataChunks[mActiveBufferIndex].CompressedDataLength, 1);
                    }
                    break;
                }
                else if (offset > mDataChunks[i].DecompressedDataPosition && offset < mDataChunks[i + 1].DecompressedDataPosition)
                {
                    mDistanceToNextBuffer = mDataChunks[i + 1].DecompressedDataPosition - offset;
                    break;
                }
            }
        }

        public DSARReader(string filePath)
        {
            mDataChunks = new List<DSARChunk>();
            mBaseStream = new BinaryReader(File.OpenRead(filePath));

            string magic = new string(mBaseStream.ReadChars(4));
            if (magic != "DSAR")
            {
                throw new InvalidDataException("Not a DSAR archive!");
            }

            VersionMinor = mBaseStream.ReadInt16();
            VersionMajor = mBaseStream.ReadInt16();
            int numChunks = mBaseStream.ReadInt32();
            mDataChunks.Capacity = numChunks;
            int headerSize = mBaseStream.ReadInt32();
            DecompressedSize = mBaseStream.ReadInt64();
            mBaseStream.BaseStream.Seek(8, SeekOrigin.Current);

            // load the chunks
            for (int i = 0; i < numChunks; i++)
            {
                DSARChunk chunk = new DSARChunk();
                chunk.Read(mBaseStream);
                mDataChunks.Add(chunk);
            }

            // get the initial buffer
            mActiveBufferIndex = 0;
            mBaseStream.BaseStream.Seek(mDataChunks[0].CompressedDataOffset, SeekOrigin.Begin);
            byte[] tCompressedBuffer = mBaseStream.ReadBytes(mDataChunks[0].CompressedDataLength);
            mActiveBuffer = new byte[mDataChunks[0].DecompressedDataLength];
            mActiveBufferPosition = 0;
            Position = mDataChunks[0].DecompressedDataPosition;
            mActiveBufferSize = mDataChunks[0].DecompressedDataLength;
            if (mDataChunks[mActiveBufferIndex].CompressionMethod == DSARCompressionMethod.LZ4)
                LZ4Codec.Decode(tCompressedBuffer, 0, mDataChunks[mActiveBufferIndex].CompressedDataLength, mActiveBuffer, 0, mDataChunks[mActiveBufferIndex].DecompressedDataLength);
            else if (mDataChunks[mActiveBufferIndex].CompressionMethod == DSARCompressionMethod.GDeflate)
                GDeflate.Decompress(mActiveBuffer, (ulong)mDataChunks[mActiveBufferIndex].DecompressedDataLength, tCompressedBuffer, (ulong)mDataChunks[mActiveBufferIndex].CompressedDataLength, 1);
        }
    }
}
