export module BinaryReader;

import Binary;

export class BinaryReader final : public Binary<std::istream> {
public:
    explicit BinaryReader(const std::filesystem::path& file) : Binary{ std::make_unique<std::ifstream>(file, std::ios::binary) } {}
    explicit BinaryReader(const std::string& data) : Binary{ std::make_unique<std::istringstream>(data, std::ios::binary) } {}
    BinaryReader() = delete;

    BinaryReader(const BinaryReader&) = delete;
    BinaryReader& operator=(const BinaryReader&) = delete;

    BinaryReader(BinaryReader&&) = delete;
    BinaryReader& operator=(BinaryReader&&) = delete;

    template<BinaryType T>
    [[nodiscard]] T Read()
    {
        T data{};
        Stream->read(reinterpret_cast<char*>(&data), sizeof(T));

        return data;
    }

    std::int32_t Read7BitEncodedInteger()
    {
        char current{};
        int32_t index = 0, result = 0;
        do
        {
            Stream->read((char*)&current, sizeof(char));
            result |= (current & 127) << index;
            index += 7;
        } while ((current & 128) != 0);

        return result;
    }

    template<BinaryType T>
    [[nodiscard]] T Read(std::size_t size) {}

    template<>
    [[nodiscard]] std::string Read()
    {
        std::size_t size{};
        Stream->read(reinterpret_cast<char*>(&size), sizeof(std::uint32_t));

        std::string data{};
        data.resize(size);
        Stream->read(&data[0], size);

        return data;
    }

    template<>
    [[nodiscard]] std::string Read(std::size_t size)
    {
        std::string data{};
        data.resize(size);
        Stream->read(&data[0], size);

        return data;
    }
};