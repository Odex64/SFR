export module Xnb;

import std;
import BinaryReader;
import BinaryWriter;
import TargetPlatform;
import FlagBits;
import XCompress;
import Converter;
import Reader;

export class Xnb final {
private:
    constexpr static std::uint8_t _headerSize{ 14 };

public:
    const TargetPlatform Platform;
    const std::uint8_t FormatVersion;
    const FlagBits Flags;
    const std::string CompressedData;
    const std::string DecompressedData;
    const std::vector<Reader> Readers;
    const std::unique_ptr<Converter> Type;

    Xnb(const TargetPlatform platform, const std::uint8_t formatVersion, const FlagBits flags, const std::string& compressedData, const std::string& decompressedData, const std::vector<Reader>& readers, std::unique_ptr<Converter> type)
        : Platform{ platform }, FormatVersion{ formatVersion }, Flags{ flags }, CompressedData{ compressedData }, DecompressedData{ std::move(decompressedData) }, Readers{ readers }, Type{ std::move(type) } {}

    Xnb() = delete;

    Xnb(const Xnb&) = delete;
    Xnb& operator=(const Xnb&) = delete;

    Xnb(Xnb&&) = delete;
    Xnb& operator=(Xnb&&) = delete;

    static std::optional<Xnb> Read(BinaryReader& stream)
    {
        const std::string header{ stream.Read<std::string>(3) };
        if (header != "XNB") return std::nullopt;

        const char platform{ stream.Read<char>() };
        TargetPlatform targetPlatform{ TargetPlatform::None };
        switch (platform) {
        case 'w':
            targetPlatform = TargetPlatform::Windows;
            break;
        case 'm':
            targetPlatform = TargetPlatform::Phone;
            break;
        case 'x':
            targetPlatform = TargetPlatform::Xbox;
            break;
        }

        if (targetPlatform == TargetPlatform::None) return std::nullopt;

        std::uint8_t formatVersion{ stream.Read<std::uint8_t>() };
        if (formatVersion != 5) return std::nullopt;

        std::uint8_t bits{ stream.Read<std::uint8_t>() };
        FlagBits flagBits{ FlagBits::None };
        switch (bits) {
        case 0:
            flagBits = FlagBits::Reach;
            break;
        case 1:
            flagBits = FlagBits::HiDef;
            break;
        case 128:
            flagBits = FlagBits::Compressed;
            break;
        }

        if (flagBits == FlagBits::None) return std::nullopt;

        std::size_t compressedSize{ stream.Read<std::uint32_t>() };
        std::size_t decompressedSize{ 0 };
        if (flagBits == FlagBits::Compressed) {
            decompressedSize = stream.Read<std::uint32_t>();
        }

        std::string compressedData{ stream.Read<std::string>(static_cast<std::size_t>(compressedSize) - _headerSize) };

        XMEMDECOMPRESSION_CONTEXT decompressionContext{ 0 };

        XMEMCODEC_PARAMETERS_LZX codecParams{};
        codecParams.Flags = 0;
        codecParams.WindowSize = 64 * 1024;
        codecParams.CompressionPartitionSize = 256 * 1024;

        if (XMemCreateDecompressionContext(XMEMCODEC_TYPE::XMEMCODEC_LZX, &codecParams, 0, &decompressionContext) != 0) {
            return std::nullopt;
        }

        std::string decompressedData{};
        decompressedData.resize(decompressedSize);
        if (XMemDecompress(decompressionContext, &decompressedData[0], &decompressedSize, &compressedData[0], compressedData.length()) != 0) {
            return std::nullopt;
        }

        XMemDestroyDecompressionContext(decompressionContext);

        BinaryReader memoryStream{ decompressedData };

        const std::int32_t readersCount{ memoryStream.Read7BitEncodedInteger() };
        if (readersCount > 1) return std::nullopt;

        std::vector<Reader> readers;
        for (std::int32_t i{ 0 }; i < readersCount; i++) {
            const std::int32_t typeLength{ memoryStream.Read7BitEncodedInteger() };
            const std::string type{ memoryStream.Read<std::string>(typeLength) };
            const std::int32_t version{ memoryStream.Read<std::int32_t>() };
            readers.emplace_back(type, version);
        }

        const std::int32_t resourcesCount{ memoryStream.Read7BitEncodedInteger() };
        if (resourcesCount != 0) return std::nullopt;

        const std::string& firstReader{ readers[0].Type };

        // TODO:
        // 1. Add readers for texture and SFD types
        // 2. Check type here and call the correct one
        // 3. Return parsed XNB

        //return Xnb{
        //    targetPlatform,
        //    formatVersion,
        //    flagBits,
        //    compressedData,
        //    decompressedData,
        //    readers,
        //}

        return std::nullopt;
    }

    static bool Write(BinaryWriter& stream)
    {

    }
};