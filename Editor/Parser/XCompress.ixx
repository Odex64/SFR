export module XCompress;

import std;

extern "C" {
    // ------------------------
    // STRUCTS AND ENUMS
    // ------------------------
    export enum XMEMCODEC_TYPE {
        XMEMCODEC_DEFAULT = 0,
        XMEMCODEC_LZX = 1
    };

    export struct XMEMCODEC_PARAMETERS_LZX {
        std::uint32_t Flags;
        std::uint32_t WindowSize;
        std::uint32_t CompressionPartitionSize;
    };


    // ------------------------
    // DECOMPRESSION FUNCTIONS
    // ------------------------
    export typedef void* XMEMDECOMPRESSION_CONTEXT;

    export std::int32_t __stdcall XMemCreateDecompressionContext(XMEMCODEC_TYPE codecType, const void* codecParams, std::uint32_t flags, XMEMDECOMPRESSION_CONTEXT* context);
    export void __stdcall XMemDestroyDecompressionContext(XMEMDECOMPRESSION_CONTEXT context);
    export std::int32_t __stdcall XMemDecompress(XMEMDECOMPRESSION_CONTEXT context, void* destination, std::size_t* destinationSize, const void* source, std::size_t sourceSize);


    // ------------------------
    // COMPRESSION FUNCTIONS
    // ------------------------
    export typedef void* XMEMCOMPRESSION_CONTEXT;

    export std::int32_t __stdcall XMemCreateCompressionContext(XMEMCODEC_TYPE codecType, const void* codecParams, std::uint32_t flags, XMEMCOMPRESSION_CONTEXT* context);
    export void __stdcall XMemDestroyCompressionContext(XMEMCOMPRESSION_CONTEXT context);
    export std::int32_t __stdcall XMemCompress(XMEMCOMPRESSION_CONTEXT context, void* destination, std::size_t* destinationSize, const void* source, std::size_t sourceSize);
}