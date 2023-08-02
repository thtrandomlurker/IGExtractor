#pragma once

#include <stdint.h>
#include <memory>
#include <GDeflate.h>
#include <lz4.h>

typedef enum {
	DSAR_COMPRESSION_GDEFLATE = 2,
	DSAR_COMPRESSION_LZ4 = 3
} DSAREntryCompressionType;

typedef struct {
	char Magic[4];
	int32_t Version;
	int32_t ChunkCount;
	int32_t DataStartPos;
	size_t DecompressedSize;
	char Padding[8];
} DSARHeader;

typedef struct {
	size_t DecompressedStartPosition;
	size_t CompressedDataOffset;
	int32_t DecompressedDataSize;
	int32_t CompressedDataSize;
	uint8_t Flags[8];
} DSAREntry;


// Helper class for the sake of handling data as a stream
class DSARFile {
public:
	DSARHeader Header;
	DSAREntry* Entries;
	size_t Position;

	DSARFile(const char* dsarPath) {
		m_SourceFile = fopen(dsarPath, "rb");
		fread(&Header, sizeof(Header), 1, m_SourceFile);
		Entries = (DSAREntry*)malloc(sizeof(DSAREntry) * Header.ChunkCount);
		for (int i = 0; i < Header.ChunkCount; i++) {
			fread(&Entries[i], sizeof(DSAREntry), 1, m_SourceFile);
		}

		// initialize the buffers
		m_ActiveEntry = 0;
		GetBuffer();
	}

	inline void DumpDecompressedArchive(const char* path) {
		FILE* outFile = fopen(path, "wb");
		for (int i = 0; i < Header.ChunkCount; i++) {
			printf("Writing chunk %d of %d\n", i, Header.ChunkCount);
			m_ActiveEntry = i;
			printf("BeforeGetBuffer\n");
			GetBuffer();
			printf("AfterGetBuffer\n");
			fwrite(m_DSARActiveBuffer, 1, Entries[i].DecompressedDataSize, outFile);
			printf("AfterWrite\n");
			char* padBuf = new char[ftell(outFile) < Entries[i + 1].DecompressedStartPosition ? Entries[i + 1].DecompressedStartPosition - ftell(outFile) : 0] {'\0'};
			fwrite(padBuf, 1, ftell(outFile) < Entries[i + 1].DecompressedStartPosition ? Entries[i + 1].DecompressedStartPosition - ftell(outFile) : 0, outFile);
		}
	}

	inline int Read(void* buffer, size_t size) {
		int32_t curChunkIdx = -1;
		int32_t aheadChunkIdx = 0;
		int32_t bufPos = 0;
		uint8_t* tBuf = (uint8_t*)malloc(size);
		for (int i = 0; i < Header.ChunkCount; i++) {
			if ((Position < (Entries[i].DecompressedStartPosition + Entries[i].DecompressedDataSize)) && Position >= Entries[i].DecompressedStartPosition) {
				curChunkIdx = i;
				break;
			}
			if (Entries[i].DecompressedStartPosition > Position) {
				aheadChunkIdx = i;
			}
		}

		size_t remainingSize = size;
		if (curChunkIdx == -1) {
			int32_t nullWriteCount = remainingSize < (Entries[aheadChunkIdx].DecompressedStartPosition - Position) ? remainingSize : (Entries[aheadChunkIdx].DecompressedStartPosition - Position);
			for (int i = 0; i < nullWriteCount; i++) {
				tBuf[bufPos] = 0x00;
				bufPos++;
			}
			curChunkIdx = aheadChunkIdx + 1;
			Position = Entries[curChunkIdx].DecompressedStartPosition;
			remainingSize -= nullWriteCount;
		}
		m_ActiveEntry = curChunkIdx;

		GetBuffer();
		m_DSARActiveBufferPos = Position - Entries[m_ActiveEntry].DecompressedStartPosition;
		// now copy the data to the buffer
		for (int i = 0; i < remainingSize; i++) {
			tBuf[bufPos] = ((uint8_t*)m_DSARActiveBuffer)[m_DSARActiveBufferPos];
			bufPos++;
			m_DSARActiveBufferPos++;
			if (m_DSARActiveBufferPos == Entries[curChunkIdx].DecompressedDataSize) {
				curChunkIdx++;
				m_ActiveEntry++;
				// refresh the buffer
				GetBuffer();
			}
		}

		// finally copy

		memcpy(buffer, tBuf, size);
		// and free
		free(tBuf);
		Position += size;
		return Position;
	}

	inline int Seek(size_t offset, int32_t whence) {
		switch (whence) {
		case SEEK_SET:
			Position = offset;
			break;
		case SEEK_CUR:
			Position += offset;
			break;
		case SEEK_END:
			printf("SEEK_END not supported.\n");
			abort();
		default:
			printf("Invalid seek mode %d.\n", whence);
			abort();
		}
	}
	inline void Close() {
		fclose(m_SourceFile);
	}
private:
	// Current position of the "reader"
	int32_t m_DSARActiveBufferPos;
	void* m_DSARActiveBuffer;
	FILE* m_SourceFile;
	int32_t m_ActiveEntry;

	inline void GetBuffer(int32_t setPos = 0) {
		if (m_DSARActiveBuffer != nullptr) {
			free(m_DSARActiveBuffer);
		}
		// then allocate the new buffer
		m_DSARActiveBuffer = malloc(Entries[m_ActiveEntry].DecompressedDataSize);

		// now load the new buffer into memory
		void* compressedBuffer = malloc(Entries[m_ActiveEntry].CompressedDataSize);
		fseek(m_SourceFile, Entries[m_ActiveEntry].CompressedDataOffset, SEEK_SET);
		fread(compressedBuffer, 1, Entries[m_ActiveEntry].CompressedDataSize, m_SourceFile);
		switch (Entries[m_ActiveEntry].Flags[0]) {
		case DSAR_COMPRESSION_GDEFLATE:
			GDeflate::Decompress((uint8_t*)m_DSARActiveBuffer, Entries[m_ActiveEntry].DecompressedDataSize, (uint8_t*)compressedBuffer, Entries[m_ActiveEntry].CompressedDataSize, 1);
			break;
		case DSAR_COMPRESSION_LZ4:
			LZ4_decompress_safe((char*)compressedBuffer, (char*)m_DSARActiveBuffer, Entries[m_ActiveEntry].CompressedDataSize, Entries[m_ActiveEntry].DecompressedDataSize);
			break;
		default:
			printf("Unsupported compression type %d. Exiting\n", Entries[m_ActiveEntry].Flags[0]);
			abort();
		}
		m_DSARActiveBufferPos = setPos;

		//test
		void* test = malloc((size_t)5);
		memcpy(test, m_DSARActiveBuffer, 4);
		((char*)test)[4] = '\0';
		printf("%s\n", (char*)test);
	}
};