export module BinaryReader;

import Binary;

export class BinaryReader final : public Binary<std::ifstream> {
public:
    explicit BinaryReader(const std::filesystem::path& file) : Binary{ std::ifstream{file, std::ios::binary} } {}
    BinaryReader() = delete;

    BinaryReader(const BinaryReader&) = delete;
    BinaryReader& operator=(const BinaryReader&) = delete;

    BinaryReader(BinaryReader&&) = delete;
    BinaryReader& operator=(BinaryReader&&) = delete;

    template<BinaryType T>
    [[nodiscard]] T Read()
    {
        T data{};
        Stream.read(reinterpret_cast<char*>(&data), sizeof(T));

        return data;
    }

    template<>
    [[nodiscard]] std::string Read()
    {
        std::size_t size{};
        Stream.read(reinterpret_cast<char*>(&size), sizeof(std::uint32_t));

        std::string data{};
        data.resize(size);
        Stream.read(&data[0], size);

        return data;
    }
};