import Xnb;
import BinaryReader;

int main()
{
    BinaryReader reader{ std::filesystem::path{"Sandbags00A.xnb"} };
    Xnb::Read(reader);


    return 0;
}