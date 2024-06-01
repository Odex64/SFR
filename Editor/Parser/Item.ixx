module;

#include "../Dependencies/stb_image.h"
#include "../Dependencies/stb_image_write.h"

export module Item;

import std;
import Converter;
import BinaryReader;
import BinaryWriter;

struct Texture {
    const std::int32_t Width;
    const std::int32_t Height;
    const std::vector<std::uint8_t> Data;

    Texture(const std::int32_t width, const std::int32_t height, const  std::vector<std::uint8_t>& data) : Width{ width }, Height{ height }, Data{ data } {}
};

struct ItemPart {
    const std::int32_t Type;
    const std::vector<Texture> Textures;

    ItemPart(const std::int32_t type, const std::vector<Texture>& textures) : Type{ type }, Textures{ textures } {}
};

export class Item final : public Converter {
public:
    inline const static std::string Reader{ "SFD.Content.ItemsContentTypeReader, SFD.Content" };
    const std::string GameName;
    const std::string FileName;
    const std::uint32_t EquipmentLayer;
    const std::string Id;
    const bool JackUnderBelt;
    const bool CanEquip;
    const bool CanScript;
    const std::string ColorPalette;
    const std::int32_t Width;
    const std::int32_t Height;
    const std::vector<ItemPart> Parts;

    Item(const std::string& gameName, const std::string& fileName, const std::uint32_t equipmentLayer, const std::string& id, const bool jackUnderBelt, const bool canEquip, const bool canScript, const std::string& colorPalette, const std::int32_t width, const std::int32_t height, const std::vector<ItemPart>& parts) noexcept : GameName{ gameName }, FileName{ fileName }, EquipmentLayer{ equipmentLayer }, Id{ id }, JackUnderBelt{ jackUnderBelt }, CanEquip{ canEquip }, CanScript{ canScript }, ColorPalette{ colorPalette }, Width{ width }, Height{ height }, Parts{ parts } {}
    Item() = delete;

    Item(const Item&) = delete;
    Item& operator=(const Item&) = delete;

    Item(Item&& other) noexcept : GameName{ std::move(other.GameName) }, FileName{ std::move(other.FileName) }, EquipmentLayer{ other.EquipmentLayer }, Id{ std::move(other.Id) }, JackUnderBelt{ other.JackUnderBelt }, CanEquip{ other.CanEquip }, CanScript{ other.CanScript }, ColorPalette{ std::move(other.ColorPalette) }, Width{ other.Width }, Height{ other.Height }, Parts{ std::move(other.Parts) } {}
    Item& operator=(Item&&) = delete;

    static [[nodiscard]] std::expected<Item, std::string> Read(BinaryReader& stream) noexcept
    {
        const std::uint8_t fileNameLen{ stream.Read7BitEncodedInteger() };
        const std::string fileName{ stream.Read<std::string>(fileNameLen) };

        const std::uint8_t gameNameLen{ stream.Read7BitEncodedInteger() };
        const std::string gameName{ stream.Read<std::string>(gameNameLen) };

        const std::int32_t equipmentLayer{ stream.Read<std::int32_t>() };

        const std::uint8_t idLen{ stream.Read7BitEncodedInteger() };
        const std::string id{ stream.Read<std::string>(idLen) };

        const bool jackUnderBelt{ stream.Read<bool>() };
        const bool canEquip{ stream.Read<bool>() };
        const bool canScript{ stream.Read<bool>() };

        const std::uint8_t colorPaletteLen{ stream.Read7BitEncodedInteger() };
        const std::string colorPalette{ stream.Read<std::string>(colorPaletteLen) };

        const std::int32_t width{ stream.Read<std::int32_t>() };
        const std::int32_t height{ stream.Read<std::int32_t>() };

        const std::uint8_t colorsCount{ stream.Read<std::uint8_t>() };
        std::vector<std::uint8_t> dynamicColorTable(colorsCount * 4);

        for (std::size_t i{ 0 }; i < dynamicColorTable.size(); i += 4) {
            dynamicColorTable[i] = stream.Read<std::uint8_t>();
            dynamicColorTable[i + 1] = stream.Read<std::uint8_t>();
            dynamicColorTable[i + 2] = stream.Read<std::uint8_t>();
            dynamicColorTable[i + 3] = stream.Read<std::uint8_t>();
        }

        const std::int32_t outerLoop{ stream.Read<std::int32_t>() };
        stream.Seek(1);

        std::vector<ItemPart> parts{};
        for (std::size_t i{ 0 }; i < outerLoop; i++) {
            const std::int32_t type{ stream.Read<std::int32_t>() };
            const std::int32_t innerLoop{ stream.Read<std::int32_t>() };
            const std::int32_t size{ width * height };
            std::vector<Texture> textures{};

            for (std::size_t j{ 0 }; j < innerLoop; j++) {
                const bool flag{ stream.Read<bool>() };
                if (flag) {
                    std::array<std::uint8_t, 4> color{};
                    std::vector<std::uint8_t> data{};
                    bool emptyImage{ true };

                    for (std::size_t k{ 0 }; k < size; k++) {
                        const bool newColor{ stream.Read<bool>() };
                        if (newColor) {
                            data.emplace_back(color[0]);
                            data.emplace_back(color[1]);
                            data.emplace_back(color[2]);
                            data.emplace_back(color[3]);
                        }
                        else {
                            const std::int32_t index{ stream.Read<std::uint8_t>() * 4 };
                            color[0] = dynamicColorTable[index];
                            color[1] = dynamicColorTable[index + 1];
                            color[2] = dynamicColorTable[index + 2];
                            color[3] = dynamicColorTable[index + 3];

                            data.emplace_back(color[0]);
                            data.emplace_back(color[1]);
                            data.emplace_back(color[2]);
                            data.emplace_back(color[3]);

                            emptyImage = false;
                        }
                    }

                    stream.Seek(1);

                    if (!emptyImage) {
                        textures.emplace_back(width, height, data);
                    }
                }
            }

            parts.emplace_back(type, textures);
        }

        return std::expected<Item, std::string>(
            std::in_place,
            gameName,
            fileName,
            equipmentLayer,
            id,
            jackUnderBelt,
            canEquip,
            canScript,
            colorPalette,
            width,
            height,
            parts
        );
    }

    static [[nodiscard]] std::expected<Item, std::string> Read(const std::filesystem::path& file) noexcept
    {

    }

    void Export(std::filesystem::path& file) const noexcept override
    {
        const std::string fileName{ file.replace_extension().string() };
        for (const ItemPart& part : Parts) {
            for (std::size_t i{ 0 }; i < part.Textures.size(); i++) {
                std::string exportedFile{ std::format("{}_{}_{}.png", fileName, part.Type, i) };
                stbi_write_png(exportedFile.c_str(), Width, Height, STBI_rgb_alpha, part.Textures[i].Data.data(), Width * STBI_rgb_alpha);
            }
        }

    }

    [[nodiscard]] std::string Binary() const noexcept override
    {
        return std::string{};
    }

    [[nodiscard]] std::string ReaderName() const noexcept override
    {
        return Item::Reader;
    }
};