export module Xnb;

import std;
import BinaryReader;
import BinaryWriter;
import TargetPlatform;
import FlagBits;
import XCompress;
import Converter;
import Texture;

export class Xnb final {
private:
    inline constexpr static std::uint8_t _headerSize{ 14 };

public:
    const TargetPlatform Platform;
    const std::uint8_t FormatVersion;
    const FlagBits Flags;
    const std::string CompressedData;
    const std::string DecompressedData;
    const std::unique_ptr<Converter> Type;

    Xnb(const TargetPlatform platform, const std::uint8_t formatVersion, const FlagBits flags, const std::string& compressedData, const std::string& decompressedData, std::unique_ptr<Converter> type) noexcept : Platform{ platform }, FormatVersion{ formatVersion }, Flags{ flags }, CompressedData{ compressedData }, DecompressedData{ std::move(decompressedData) }, Type{ std::move(type) } {}
    Xnb() = delete;

    Xnb(const Xnb&) = delete;
    Xnb& operator=(const Xnb&) = delete;

    Xnb(Xnb&&) = delete;
    Xnb& operator=(Xnb&&) = delete;

    static [[nodiscard]] std::expected<Xnb, std::string> Read(const std::filesystem::path& file) noexcept
    {
        if (file.extension().string() != ".xnb" || !std::filesystem::exists(file)) return std::unexpected("Wrong file format");

        BinaryReader stream{ file };

        const std::string header{ stream.Read(3) };
        if (header != "XNB") return std::unexpected("Invalid header");

        const char platform{ static_cast<char>(stream.Read<std::uint8_t>()) };
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

        if (targetPlatform == TargetPlatform::None) return std::unexpected("Invalid platform");

        std::uint8_t formatVersion{ stream.Read<std::uint8_t>() };
        if (formatVersion != 5) return std::unexpected("Invalid format. Must be XNA 4.0");

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

        if (flagBits == FlagBits::None) return std::unexpected("Invalid flags");

        std::size_t compressedSize{ stream.Read<std::uint32_t>() };
        std::size_t decompressedSize{ 0 };
        if (flagBits == FlagBits::Compressed) {
            decompressedSize = stream.Read<std::uint32_t>();
        }

        std::string compressedData{ stream.Read(static_cast<std::size_t>(compressedSize) - _headerSize) };

        XMEMDECOMPRESSION_CONTEXT decompressionContext{ 0 };

        XMEMCODEC_PARAMETERS_LZX codecParams{};
        codecParams.Flags = 0;
        codecParams.WindowSize = 64 * 1024;
        codecParams.CompressionPartitionSize = 256 * 1024;

        if (XMemCreateDecompressionContext(XMEMCODEC_TYPE::XMEMCODEC_LZX, &codecParams, 0, &decompressionContext) != 0) {
            return std::unexpected("Could not create decompression context");
        }

        std::string decompressedData{};
        decompressedData.resize(decompressedSize);
        if (XMemDecompress(decompressionContext, &decompressedData[0], &decompressedSize, &compressedData[0], compressedData.size()) != 0) {
            XMemDestroyDecompressionContext(decompressionContext);
            return std::unexpected("Failed decompression");
        }

        XMemDestroyDecompressionContext(decompressionContext);

        BinaryReader memoryStream{ decompressedData };

        const std::uint8_t readersCount{ memoryStream.Read7BitEncodedInteger() };
        if (readersCount > 1) return std::unexpected("Only 1 reader supported");

        const std::uint8_t typeLength{ memoryStream.Read7BitEncodedInteger() };
        const std::string typeName{ memoryStream.Read(typeLength) };
        const std::int32_t version{ memoryStream.Read<std::int32_t>() };

        const std::uint8_t resourcesCount{ memoryStream.Read7BitEncodedInteger() };
        if (resourcesCount != 0) std::unexpected("Invalid resources count");

        const std::uint8_t index{ memoryStream.Read7BitEncodedInteger() };
        if (index > readersCount) std::unexpected("Resources count and readers count do not match");

        std::unique_ptr<Converter> type{ nullptr };
        if (typeName == Texture::Reader) {
            if (std::expected<Texture, std::string> texture = Texture::Read(memoryStream); texture.has_value()) {
                type = std::make_unique<Texture>(std::move(texture.value()));
            }
        }

        if (type == nullptr) return std::unexpected("File type not supported or there was an error while reading it");

        return std::expected<Xnb, std::string>(
            std::in_place,
            targetPlatform,
            formatVersion,
            flagBits,
            compressedData,
            decompressedData,
            std::move(type)
        );
    }

    template<Convertible T>
    static [[nodiscard]] std::expected<Xnb, std::string> Write(const std::filesystem::path& file) noexcept
    {
        if (!std::filesystem::exists(file)) return std::unexpected("File does not exist");

        BinaryWriter stream{ file.relative_path().replace_extension(".xnb") };
        stream.Write("XNB", false);
        stream.Write<std::uint8_t>(static_cast<std::uint8_t>('w'));
        stream.Write<std::uint8_t>(5);
        stream.Write<std::uint8_t>(128);

        std::unique_ptr<Converter> type{ nullptr };
        const std::string extension{ file.extension().string() };
        if constexpr (std::is_same<T, Texture>::value) {
            if (extension != ".png") return std::unexpected("Wrong file extension");
            if (std::expected<Texture, std::string> texture = Texture::Read(file); texture.has_value()) {
                type = std::make_unique<Texture>(std::move(texture.value()));
            }
        }

        if (type == nullptr) return std::unexpected("File type not supported or there was an error while reading it");

        BinaryWriter memoryStream{};
        memoryStream.Write7BitEncodedInteger(1);
        memoryStream.Write7BitEncodedInteger(type->ReaderName().size());
        memoryStream.Write(type->ReaderName(), false);
        memoryStream.Write<std::int32_t>(0);
        memoryStream.Write7BitEncodedInteger(0);
        memoryStream.Write7BitEncodedInteger(1);
        memoryStream.Write(type->Binary(), false);

        const std::string decompressedData{ memoryStream.ToString() };
        const std::size_t decompressedSize{ decompressedData.size() };

        XMEMCOMPRESSION_CONTEXT compressionContext{ 0 };

        XMEMCODEC_PARAMETERS_LZX codecParams{};
        codecParams.Flags = 0;
        codecParams.WindowSize = 64 * 1024;
        codecParams.CompressionPartitionSize = 256 * 1024;

        if (XMemCreateCompressionContext(XMEMCODEC_TYPE::XMEMCODEC_LZX, &codecParams, 0, &compressionContext) != 0) {
            return std::unexpected("Could not create decompression context");
        }

        std::size_t compressedSize{ decompressedSize };
        std::string compressedData{};
        compressedData.resize(compressedSize);
        if (XMemCompress(compressionContext, &compressedData[0], &compressedSize, &decompressedData[0], decompressedSize) != 0) {
            XMemDestroyCompressionContext(compressionContext);
            return std::unexpected("Failed compression");
        }

        XMemDestroyCompressionContext(compressionContext);

        compressedData.resize(compressedSize);

        stream.Write<std::uint32_t>(compressedSize + _headerSize);
        stream.Write<std::uint32_t>(decompressedSize);
        stream.Write(compressedData, false);

        return std::expected<Xnb, std::string>(
            std::in_place,
            TargetPlatform::Windows,
            static_cast<std::uint8_t>(5),
            FlagBits::Compressed,
            compressedData,
            decompressedData,
            std::move(type)
        );
    }
};