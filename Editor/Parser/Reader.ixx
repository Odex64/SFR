export module Reader;

import std;

export struct Reader final {
    const std::string Type;
    const std::int32_t Version;

    Reader(const std::string& type, std::int32_t version) : Type{ type }, Version{ version } {}
};