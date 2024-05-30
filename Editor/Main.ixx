import std;
import Xnb;
import Texture;

int main()
{
    //std::expected<Xnb, std::string> file{ Xnb::Read("profile_bg.xnb") };
    //if (!file.has_value()) return 1;
    //file.value().Type->Export("profile_bg.png");
    std::expected<Xnb, std::string> file{ Xnb::Write<Texture>("profile_bg.png") };

    //for (const auto& entry : std::filesystem::recursive_directory_iterator("Images/Misc/SFDLogo")) {
    //    if (entry.is_regular_file() && entry.path().extension() == ".png") {
    //        std::expected<Xnb, std::string> file{ Xnb::Write<Texture>(entry.path()) };
    //    }
    //}

    return 0;
}