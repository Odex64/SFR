export module Binary;

import std;

template<typename T>
concept DerivedStream = std::derived_from<T, std::ios>;

export template <typename T>
concept BinaryType = std::integral<T> || std::floating_point<T> || std::same_as<T, std::string> || std::same_as<T, bool>;

export template<DerivedStream T>
class Binary {
protected:
    T Stream;

public:
    explicit Binary(T&& stream) : Stream{ std::move(stream) } {}
    Binary() = delete;

    Binary(const Binary&) = delete;
    Binary& operator=(const Binary&) = delete;

    Binary(Binary&&) = delete;
    Binary& operator=(Binary&&) = delete;

    virtual ~Binary()
    {
        Stream.close();
    }
};