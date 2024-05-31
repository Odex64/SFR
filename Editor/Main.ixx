import std;
import Xnb;
import Texture;
import Sound;

int main()
{
    //std::expected<Xnb, std::string> file{ Xnb::Read("MenuMove.xnb") };
    //if (!file.has_value()) return 1;
    //file.value().Type->Export("MenuMove.wav");
    //std::expected<Xnb, std::string> file{ Xnb::Write<Sound>("MenuMove.wav") };

    for (const auto& entry : std::filesystem::recursive_directory_iterator("Weapons")) {
        if (entry.is_regular_file() && entry.path().extension() == ".wav") {
            std::expected<Xnb, std::string> file{ Xnb::Write<Sound>(entry.path()) };
        }
    }

    return 0;
}