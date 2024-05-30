export module BinaryWriter;

import Binary;

export class BinaryWriter final : public Binary<std::ostream> {
public:
    explicit BinaryWriter(const std::filesystem::path& file) noexcept : Binary{ std::make_unique<std::ofstream>(file, std::ios::binary) } {}
    explicit BinaryWriter(const std::string& data) noexcept : Binary{ std::make_unique<std::ostringstream>(data, std::ios::binary) } {}
    explicit BinaryWriter() noexcept : Binary{ std::make_unique<std::ostringstream>(std::ios::binary) } {}

    BinaryWriter(const BinaryWriter&) = delete;
    BinaryWriter& operator=(const BinaryWriter&) = delete;

    BinaryWriter(BinaryWriter&&) = delete;
    BinaryWriter& operator=(BinaryWriter&&) = delete;

    template<BinaryType T>
    void Write(T data)
    {
        Stream->write(reinterpret_cast<char*>(&data), sizeof(T));
    }

    void Write(const std::string& data, bool writeSize = true)
    {
        std::size_t size{ data.size() };
        if (writeSize) {
            Stream->write(reinterpret_cast<char*>(&size), sizeof(std::uint32_t));
        }

        Stream->write(data.c_str(), size);
    }

    void Write7BitEncodedInteger(std::int32_t value)
    {
        while (value > 0x7Fu) {
            Write<std::uint8_t>(static_cast<std::uint8_t>(value | ~0x7Fu));
            value >>= 7;
        }

        Write<std::uint8_t>(static_cast<std::uint8_t>(value));
    }

    [[nodiscard]] std::string ToString() const noexcept override
    {
        if (std::ostringstream* d = dynamic_cast<std::ostringstream*>(Stream.get()); d != nullptr) {
            return d->str();
        }

        return std::string{};
    }
};