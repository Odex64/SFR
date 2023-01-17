SET SFD="C:\Program Files (x86)\Steam\steamapps\common\Superfighters Deluxe"
cd ..\..\..\

copy SFR\bin\%1\SFR.exe.config %SFD%
copy SFR\bin\%1\SFR.exe %SFD%
copy SFR\bin\%1\0Harmony.dll %SFD%\SFR