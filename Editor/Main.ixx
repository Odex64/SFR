import std;
import Xnb;
import BinaryReader;
import Texture;

int main()
{
    //std::expected<Xnb, std::string> file{ Xnb::Read("Images/Misc/profile_bg2-copy.xnb") };
    //if (!file.has_value()) return 1;
    //file.value().Type->Export("Images/Misc/profile_bg2.png");
    //std::expected<Xnb, std::string> file{ Xnb::Write<Texture>("Images/Misc/profile_bg2.png") };

    //for (const auto& entry : std::filesystem::recursive_directory_iterator("Images/Misc/SFDLogo")) {
    //    if (entry.is_regular_file() && entry.path().extension() == ".png") {
    //        std::expected<Xnb, std::string> file{ Xnb::Write<Texture>(entry.path()) };
    //    }
    //}

    return 0;
}