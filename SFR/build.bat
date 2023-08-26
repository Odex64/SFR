SET SFD="C:\Steam\steamapps\common\Superfighters Deluxe"
cd ..

copy %1\SFR.exe.config %SFD%
copy %1\SFR.exe %SFD%
copy %1\0Harmony.dll %SFD%\SFR
copy %1\Mono.Cecil.dll %SFD%\SFR
copy %1\MonoMod.Backports.dll %SFD%\SFR
copy %1\MonoMod.Core.dll %SFD%\SFR
copy %1\MonoMod.Iced.dll %SFD%\SFR
copy %1\MonoMod.ILHelpers.dll %SFD%\SFR
copy %1\MonoMod.Utils.dll %SFD%\SFR