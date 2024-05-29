export module Binary;

import std;

export template <typename T>
concept BinaryType = std::integral<T> || std::floating_point<T> || std::same_as<T, std::string> || std::same_as<T, bool> || std::same_as<T, char>;

template<typename T>
concept DerivedStream = std::derived_from<T, std::ios>;

export template<DerivedStream T>
class Binary {
protected:
    std::unique_ptr<T> Stream;

public:
    explicit Binary(std::unique_ptr<T> stream) : Stream{ std::move(stream) } {}
    Binary() = delete;

    Binary(const Binary&) = delete;
    Binary& operator=(const Binary&) = delete;

    Binary(Binary&&) = delete;
    Binary& operator=(Binary&&) = delete;
};