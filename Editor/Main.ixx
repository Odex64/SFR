module;

#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <commdlg.h>

export module Main;
import std;
import Xnb;
import Texture;
import Sound;

LRESULT CALLBACK WndProc(HWND, UINT, WPARAM, LPARAM);
void AddMenus(HWND);
void OpenFileDialog(HWND hwnd);
void ShowError(HWND hwnd, const std::string& error);

constexpr std::uint16_t WIDTH{ 1024 };
constexpr std::uint16_t HEIGHT{ 768 };
constexpr std::uint8_t IDM_FILE_NEW{ 1 };
constexpr std::uint8_t IDM_FILE_OPEN{ 2 };
constexpr std::uint8_t IDM_FILE_CONVERT{ 3 };

int WINAPI wWinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, PWSTR pCmdLine, int nCmdShow)
{
    MSG msg;
    WNDCLASSW wc = { 0 };
    wc.lpszClassName = L"Editor";
    wc.hInstance = hInstance;
    wc.hbrBackground = GetSysColorBrush(COLOR_3DFACE);
    wc.lpfnWndProc = WndProc;
    wc.hCursor = LoadCursor(0, IDC_ARROW);

    RegisterClassW(&wc);

    const std::int32_t xPos{ (GetSystemMetrics(SM_CXSCREEN) - WIDTH) / 2 };
    const std::int32_t yPos{ (GetSystemMetrics(SM_CYSCREEN) - HEIGHT) / 2 };

    CreateWindowW(wc.lpszClassName, L"Editor", WS_MINIMIZEBOX | WS_VISIBLE | WS_CAPTION | WS_SYSMENU, xPos, yPos, WIDTH, HEIGHT, 0, 0, hInstance, 0);

    while (GetMessage(&msg, NULL, 0, 0)) {

        TranslateMessage(&msg);
        DispatchMessage(&msg);
    }

    return (int)msg.wParam;
}

LRESULT CALLBACK WndProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
    switch (msg) {
    case WM_CREATE:
        AddMenus(hwnd);
        break;
    case WM_COMMAND:
        switch (LOWORD(wParam)) {
        case IDM_FILE_NEW:
            MessageBeep(MB_ICONINFORMATION);
            break;
        case IDM_FILE_OPEN:
            MessageBeep(MB_ICONINFORMATION);
            break;
        case IDM_FILE_CONVERT:
            OpenFileDialog(hwnd);
            break;
        }
        break;

    case WM_DESTROY:
        PostQuitMessage(0);
        break;
    }

    return DefWindowProcW(hwnd, msg, wParam, lParam);
}

void AddMenus(HWND hwnd)
{
    HMENU hMenubar;
    HMENU hMenu;

    hMenubar = CreateMenu();
    hMenu = CreateMenu();

    AppendMenuW(hMenu, MF_STRING, IDM_FILE_NEW, L"&New");
    AppendMenuW(hMenu, MF_STRING, IDM_FILE_OPEN, L"&Open");
    AppendMenuW(hMenu, MF_STRING, IDM_FILE_CONVERT, L"&Convert");
    AppendMenuW(hMenu, MF_SEPARATOR, 0, NULL);

    AppendMenuW(hMenubar, MF_POPUP, (UINT_PTR)hMenu, L"&File");
    SetMenu(hwnd, hMenubar);
}

void OpenFileDialog(HWND hwnd)
{
    OPENFILENAME ofn;
    wchar_t szFile[255]{};

    const std::wstring currentPath{ std::filesystem::current_path().wstring() };

    ZeroMemory(&ofn, sizeof(ofn));
    ofn.lStructSize = sizeof(ofn);
    ofn.hwndOwner = hwnd;
    ofn.lpstrFile = szFile;
    ofn.lpstrFile[0] = '\0';
    ofn.nMaxFile = sizeof(szFile);
    ofn.lpstrFilter = L"XNA (.xnb)\0*.xnb\0Audio (.wav)\0*.wav\0Images (.png)\0*.png\0";
    ofn.nFilterIndex = 1;
    ofn.lpstrFileTitle = 0;
    ofn.nMaxFileTitle = 0;
    ofn.lpstrInitialDir = currentPath.c_str();
    ofn.Flags = OFN_PATHMUSTEXIST | OFN_FILEMUSTEXIST;

    if (GetOpenFileName(&ofn))
    {
        std::filesystem::path file{ szFile };
        const std::filesystem::path& extension{ file.extension() };
        if (extension == ".xnb") {
            std::expected<Xnb, std::string> xnb{ Xnb::Read(file) };
            if (xnb.has_value()) {
                xnb->Type->Export(file);
                MessageBox(hwnd, L"XNB file has been converted", L"Success", MB_OK);
            }
            else {
                ShowError(hwnd, "Invalid .xnb file");
            }
        }
        else {
            if (extension == ".wav") {
                std::expected<Xnb, std::string> sound{ Xnb::Write<Sound>(file) };
                if (sound.has_value()) {
                    MessageBox(hwnd, L"Sound file has been converted", L"Success", MB_OK);
                }
                else {
                    ShowError(hwnd, sound.error());
                }
            }
            else if (extension == ".png") {
                std::expected<Xnb, std::string> texture{ Xnb::Write<Texture>(file) };
                if (texture.has_value()) {
                    MessageBox(hwnd, L"Texture file has been converted", L"Success", MB_OK);
                }
                else {
                    ShowError(hwnd, texture.error());
                }
            }
        }
    }
}

void ShowError(HWND hwnd, const std::string& error) {
    const std::wstring wError{ error.begin(), error.end() };
    MessageBox(hwnd, wError.c_str(), L"Failed to read file", MB_OK);
}