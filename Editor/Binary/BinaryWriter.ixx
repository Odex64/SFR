export module BinaryWriter;

import Binary;

export class BinaryWriter final : public Binary<std::ofstream> {
public:
    explicit BinaryWriter(const std::filesystem::path& file) : Binary{ std::ofstream{file, std::ios::binary} } {}
    BinaryWriter() = delete;

    BinaryWriter(const BinaryWriter&) = delete;
    BinaryWriter& operator=(const BinaryWriter&) = delete;

    BinaryWriter(BinaryWriter&&) = delete;
    BinaryWriter& operator=(BinaryWriter&&) = delete;

    template<BinaryType T>
    void Write(const T& data)
    {
        Stream.write(reinterpret_cast<char*>(&data), sizeof(T));
    }

    template<>
    void Write(const std::string& data)
    {
        std::size_t size{ data.size() };
        Stream.write(reinterpret_cast<char*>(&size), sizeof(std::uint32_t));
        Stream.write(data.c_str(), size);
    }
};