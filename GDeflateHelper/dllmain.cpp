// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

extern "C" {
    __declspec(dllexport) bool __cdecl Decompress(uint8_t* output, size_t outputSize, uint8_t* in, size_t inSize, uint32_t numWorkers) {
        printf("in the C++ func\n");
        printf("%d\n", *output);
        return GDeflate::Decompress(output, outputSize, in, inSize, numWorkers);
    }
    __declspec(dllexport) bool __cdecl Compress(uint8_t* output, size_t* outputSize, const uint8_t* in, size_t inSize, uint32_t level, uint32_t flags) {
        return GDeflate::Compress(output, outputSize, in, inSize, level, flags);
    }
}