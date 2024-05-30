export module Binary;

import std;

export template <typename T>
concept BinaryType = std::integral<T> || std::floating_point<T>;

template<typename T>
concept DerivedStream = std::derived_from<T, std::ios>;

export template<DerivedStream T>
class Binary {
protected:
    std::unique_ptr<T> Stream;

public:
    explicit Binary(std::unique_ptr<T> stream) noexcept : Stream{ std::move(stream) } {}
    Binary() = delete;

    Binary(const Binary&) = delete;
    Binary& operator=(const Binary&) = delete;

    Binary(Binary&&) = delete;
    Binary& operator=(Binary&&) = delete;

    [[nodiscard]] virtual std::string ToString() const noexcept = 0;
};