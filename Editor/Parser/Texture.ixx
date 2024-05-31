module;

#include "../Dependencies/stb_image.h"
#include "../Dependencies/stb_image_write.h"

export module Texture;

import Converter;
import BinaryReader;
import BinaryWriter;

export class Texture final : public Converter {
public:
    inline const static std::string Reader{ "Microsoft.Xna.Framework.Content.Texture2DReader, Microsoft.Xna.Framework.Graphics, Version=4.0.0.0, Culture=neutral, PublicKeyToken=842cf8be1de50553" };
    const std::int32_t Format;
    const std::uint32_t Width;
    const std::uint32_t Height;
    const std::uint32_t MipCount;
    const std::uint32_t DataSize;
    const std::vector<std::uint8_t> Data;

    Texture(const std::int32_t format, const std::uint32_t width, const std::uint32_t height, const std::uint32_t mipCount, const std::uint32_t dataSize, const std::vector<std::uint8_t>& data) noexcept : Format{ format }, Width{ width }, Height{ height }, MipCount{ mipCount }, DataSize{ dataSize }, Data{ data } {}
    Texture() = delete;

    Texture(const Texture&) = delete;
    Texture& operator=(const Texture&) = delete;

    Texture(Texture&& other) noexcept : Format{ other.Format }, Width{ other.Width }, Height{ other.Height }, MipCount{ other.MipCount }, DataSize{ other.DataSize }, Data{ std::move(other.Data) } {}
    Texture& operator=(Texture&&) = delete;

    static [[nodiscard]] std::expected<Texture, std::string> Read(BinaryReader& stream) noexcept
    {
        const std::int32_t format{ stream.Read<std::int32_t>() };
        if (format != 0) return std::unexpected("Invalid texture format");

        const std::uint32_t width{ stream.Read<std::uint32_t>() };
        const std::uint32_t height{ stream.Read<std::uint32_t>() };

        const std::uint32_t mipCount{ stream.Read<std::uint32_t>() };
        if (mipCount > 1) return std::unexpected("Invalid mip count. Must be 1");

        const std::uint32_t dataSize{ stream.Read<std::uint32_t>() };

        std::vector<std::uint8_t> data(dataSize);
        for (std::size_t i{ 0 }; i < data.size(); i += 4) {
            data[i] = stream.Read<std::uint8_t>();
            data[i + 1] = stream.Read<std::uint8_t>();
            data[i + 2] = stream.Read<std::uint8_t>();
            data[i + 3] = stream.Read<std::uint8_t>();
        }

        return std::expected<Texture, std::string>(
            std::in_place,
            format,
            width,
            height,
            mipCount,
            dataSize,
            data
        );
    }

    static [[nodiscard]] std::expected<Texture, std::string> Read(const std::filesystem::path& file) noexcept
    {
        if (file.extension().string() != ".png") return std::unexpected("Wrong file format");

        std::int32_t width{};
        std::int32_t height{};
        std::int32_t comp{};
        stbi__result_info info{};

        stbi_uc* rawData{ stbi_load(file.string().c_str(), &width, &height, &comp, STBI_rgb_alpha) };
        std::vector<std::uint8_t> data(rawData, rawData + width * height * STBI_rgb_alpha);

        return std::expected<Texture, std::string>(
            std::in_place,
            0,
            width,
            height,
            1,
            static_cast<std::uint32_t>(data.size()),
            data
        );
    }

    void Export(const std::filesystem::path& file) const noexcept override
    {
        if (file.extension().string() != ".png") return;

        if (std::filesystem::exists(file)) {
            std::filesystem::remove(file);
        }

        stbi_write_png(file.string().c_str(), Width, Height, STBI_rgb_alpha, Data.data(), Width * STBI_rgb_alpha);
    }

    [[nodiscard]] std::string Binary() const noexcept override
    {
        BinaryWriter stream{};
        stream.Write<std::int32_t>(Format);
        stream.Write<std::uint32_t>(Width);
        stream.Write<std::uint32_t>(Height);
        stream.Write<std::uint32_t>(MipCount);
        stream.Write<std::uint32_t>(DataSize);

        for (const std::uint8_t pixel : Data) {
            stream.Write<std::uint8_t>(pixel);
        }

        return stream.ToString();
    }

    [[nodiscard]] std::string ReaderName() const noexcept override
    {
        return Texture::Reader;
    }
};