export module Converter;

import std;
import BinaryWriter;
import BinaryReader;

export class Converter {
public:
    virtual [[nodiscard]] bool Read(BinaryReader& stream) = 0;
    virtual [[nodiscard]] bool Write(BinaryWriter& stream) = 0;
};