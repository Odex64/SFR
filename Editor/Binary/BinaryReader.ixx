export module BinaryReader;

import Binary;

export class BinaryReader final : public Binary<std::istream> {
public:
    explicit BinaryReader(const std::filesystem::path& file) noexcept : Binary{ std::make_unique<std::ifstream>(file, std::ios::binary) } {}
    explicit BinaryReader(const std::string& data) noexcept : Binary{ std::make_unique<std::istringstream>(data, std::ios::binary) } {}
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

    template<BinaryType T>
    [[nodiscard]] T Read(std::size_t size) {}

    template<>
    [[nodiscard]] std::string Read(std::size_t size)
    {
        std::string data{};
        data.resize(size);
        Stream->read(&data[0], size);

        return data;
    }

    [[nodiscard]] std::uint8_t Read7BitEncodedInteger()
    {
        std::uint8_t result{};
        std::uint8_t bitsRead{};
        std::uint8_t value{};

        do {
            value = Read<std::uint8_t>();
            result |= (value & 0x7f) << bitsRead;
            bitsRead += 7;
        } while (value & 0x80);

        return result;
    }

    [[nodiscard]] std::string ToString() const noexcept override
    {
        if (std::istringstream* stream = dynamic_cast<std::istringstream*>(Stream.get()); stream != nullptr) {
            return stream->str();
        }

        return std::string{};
    }

    void Seek(std::streamsize count) const noexcept
    {
        Stream->ignore(count);
    }
};