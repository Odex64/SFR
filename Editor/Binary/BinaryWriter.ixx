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
    void Write(const T& data)
    {
        Stream->write(reinterpret_cast<const char*>(&data), sizeof(T));
    }

    template<>
    void Write(const std::string& data)
    {
        std::size_t size{ data.size() };
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
        if (std::ostringstream* stream = dynamic_cast<std::ostringstream*>(Stream.get()); stream != nullptr) {
            return stream->str();
        }

        return std::string{};
    }
};