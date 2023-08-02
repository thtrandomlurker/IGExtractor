// IGExtractor.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include <GDeflate.h>
#include <cstdio>
#include <stdint.h>
#include <sys/stat.h>
#include <lz4.h>
#include "DSARArchive.h"

int main(int argc, char** argv)
{
	/*struct stat fstat;
	stat(argv[1], &fstat);

	FILE* file = fopen(argv[1], "rb");

	void* datBuf = malloc(fstat.st_size);
	void* outBuf = malloc(0x800000);

	fread(datBuf, 1, fstat.st_size, file);

	//bool decSuccess = GDeflate::Decompress((uint8_t*)outBuf, 0x800000, (uint8_t*)datBuf, fstat.st_size, 1);
	LZ4_decompress_safe((char*)datBuf, (char*)outBuf, fstat.st_size, 0x800000);

	if (true) {
		printf("I can't believe it worked");
		char* out = strcat(argv[1], "_dec");
		FILE* ofile = fopen(out, "wb");
		fwrite(outBuf, 1, 0x800000, ofile);
		fclose(ofile);
	}
	fclose(file);*/

	DSARFile dsar = DSARFile("tex_char_hero");

	dsar.DumpDecompressedArchive("tex_char_hero_undsar");
	dsar.Close();

}

// Run program: Ctrl + F5 or Debug > Start Without Debugging menu
// Debug program: F5 or Debug > Start Debugging menu

// Tips for Getting Started: 
//   1. Use the Solution Explorer window to add/manage files
//   2. Use the Team Explorer window to connect to source control
//   3. Use the Output window to see build output and other messages
//   4. Use the Error List window to view errors
//   5. Go to Project > Add New Item to create new code files, or Project > Add Existing Item to add existing code files to the project
//   6. In the future, to open this project again, go to File > Open > Project and select the .sln file
