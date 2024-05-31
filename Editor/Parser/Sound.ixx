export module Sound;

import Converter;
import BinaryReader;
import BinaryWriter;

export class Sound final : public Converter {
public:
    inline const static std::string Reader{ "Microsoft.Xna.Framework.Content.SoundEffectReader" };
    const std::uint32_t FormatSize;
    const std::vector<std::uint8_t> Format;
    const std::uint32_t DataSize;
    const std::vector<std::uint8_t> Data;
    const std::int32_t LoopStart;
    const std::int32_t LoopLength;
    const std::int32_t Duration;

    Sound(const std::uint32_t formatSize, const std::vector<std::uint8_t>& format, const std::uint32_t dataSize, const std::vector<std::uint8_t>& data, const std::int32_t loopStart, const std::int32_t loopLength, const std::int32_t duration) noexcept : FormatSize{ formatSize }, Format{ format }, DataSize{ dataSize }, Data{ data }, LoopStart{ loopStart }, LoopLength{ loopLength }, Duration{ duration } {}
    Sound() = delete;

    Sound(const Sound&) = delete;
    Sound& operator=(const Sound&) = delete;

    Sound(Sound&& other) noexcept : FormatSize{ other.FormatSize }, Format{ std::move(other.Format) }, DataSize{ other.DataSize }, Data{ std::move(other.Data) }, LoopStart{ other.LoopStart }, LoopLength{ other.LoopLength }, Duration{ other.Duration } {}
    Sound& operator=(Sound&&) = delete;

    static [[nodiscard]] std::expected<Sound, std::string> Read(BinaryReader& stream) noexcept
    {
        const std::uint32_t formatSize{ stream.Read<std::uint32_t>() };
        if (formatSize != 18) return std::unexpected("Audio format not supported");

        std::vector<std::uint8_t> format{};
        for (std::size_t i{ 0 }; i < formatSize; i++) {
            format.emplace_back(stream.Read<std::uint8_t>());
        }

        const std::uint32_t dataSize{ stream.Read<std::uint32_t>() };
        std::vector<std::uint8_t> data{};
        for (std::size_t i{ 0 }; i < dataSize; i++) {
            data.emplace_back(stream.Read<std::uint8_t>());
        }

        const std::int32_t loopStart{ stream.Read<std::int32_t>() };
        const std::int32_t loopLength{ stream.Read<std::int32_t>() };
        const std::int32_t duration{ stream.Read<std::int32_t>() };

        return std::expected<Sound, std::string>(
            std::in_place,
            formatSize,
            format,
            dataSize,
            data,
            loopStart,
            loopLength,
            duration
        );
    }

    static [[nodiscard]] std::expected<Sound, std::string> Read(const std::filesystem::path& file) noexcept
    {
        if (file.extension().string() != ".wav") return std::unexpected("Wrong file format");

        BinaryReader stream{ file };

        stream.Seek(16);
        const std::uint32_t formatSize{ stream.Read<std::uint32_t>() };

        std::vector<std::uint8_t> format;
        for (std::size_t i{ 0 }; i < formatSize; i++) {
            format.emplace_back(stream.Read<std::uint8_t>());
        }

        stream.Seek(4);
        const std::uint32_t dataSize{ stream.Read<std::uint32_t>() };

        std::vector<std::uint8_t> data;
        for (std::size_t i{ 0 }; i < dataSize; i++) {
            data.emplace_back(stream.Read<std::uint8_t>());
        }

        return std::expected<Sound, std::string>(
            std::in_place,
            formatSize,
            format,
            dataSize,
            data,
            0,
            0,
            0
        );
    }

    void Export(const std::filesystem::path& file) const noexcept override
    {
        if (file.extension().string() != ".wav") return;

        if (std::filesystem::exists(file)) {
            std::filesystem::remove(file);
        }

        const std::size_t size{ Data.size() + Format.size() + 20 };

        BinaryWriter stream{ file };
        stream.Write<std::string>("RIFF");
        stream.Write<std::int32_t>(static_cast<std::int32_t>(size - 8));
        stream.Write<std::string>("WAVE");
        stream.Write<std::string>("fmt ");

        const std::int16_t wavTypeFormat{ static_cast<std::int16_t>((Format[1] << 8) | Format[0]) };
        const std::int16_t flags{ static_cast<std::int16_t>((Format[3] << 8) | Format[2]) };
        const std::int32_t sampleRate{ static_cast<std::int32_t>((Format[7] << 24) | (Format[6] << 16) | (Format[5] << 8) | Format[4]) };
        const std::int32_t bytesPerSec{ static_cast<std::int32_t>((Format[11] << 24) | (Format[10] << 16) | (Format[9] << 8) | Format[8]) };
        const std::int16_t blockAlignment{ static_cast<std::int16_t>((Format[13] << 8) | Format[12]) };
        const std::int16_t bitsPerSample{ static_cast<std::int16_t>((Format[15] << 8) | Format[14]) };

        stream.Write<std::int32_t>(FormatSize);
        stream.Write<std::int16_t>(wavTypeFormat);
        stream.Write<std::int16_t>(flags);
        stream.Write<std::int32_t>(sampleRate);
        stream.Write<std::int32_t>(bytesPerSec);
        stream.Write<std::int16_t>(blockAlignment);
        stream.Write<std::int16_t>(bitsPerSample);

        stream.Write<std::uint8_t>(0);
        stream.Write<std::uint8_t>(0);

        stream.Write<std::string>("data");
        stream.Write<std::int32_t>(DataSize);
        for (const std::uint8_t data : Data) {
            stream.Write<std::uint8_t>(data);
        }
    }

    [[nodiscard]] std::string Binary() const noexcept override
    {
        BinaryWriter stream{};
        stream.Write<std::uint32_t>(FormatSize);
        for (const std::uint8_t format : Format) {
            stream.Write<std::uint8_t>(format);
        }

        stream.Write<std::uint32_t>(DataSize);
        for (const std::uint8_t data : Data) {
            stream.Write<std::uint8_t>(data);
        }

        stream.Write<std::int32_t>(LoopStart);
        stream.Write<std::int32_t>(LoopLength);
        stream.Write<std::int32_t>(Duration);

        return stream.ToString();
    }

    [[nodiscard]] std::string ReaderName() const noexcept override
    {
        return Sound::Reader;
    }
};