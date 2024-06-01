export module Converter;

import std;

export class Converter {
public:
    virtual void Export(std::filesystem::path& file) const noexcept = 0;
    virtual [[nodiscard]] std::string Binary() const noexcept = 0;
    virtual [[nodiscard]] std::string ReaderName() const noexcept = 0;
    virtual ~Converter() noexcept = default;
};

export template<typename T>
concept Convertible = std::derived_from<T, Converter>;