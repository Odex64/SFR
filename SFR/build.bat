SET SFD="C:\Program Files (x86)\Steam\steamapps\common\Superfighters Deluxe"
cd ..

copy %1\SFR.exe.config %SFD%
copy %1\SFR.exe %SFD%
copy %1\0Harmony.dll %SFD%\SFR
copy %1\Microsoft.CodeAnalysis.CSharp.dll %SFD%\SFR
copy %1\Microsoft.CodeAnalysis.dll %SFD%\SFR
copy %1\System.Collections.Immutable.dll %SFD%\SFR
copy %1\System.Memory.dll %SFD%\SFR
copy %1\System.Numerics.Vectors.dll %SFD%\SFR
copy %1\System.Reflection.Metadata.dll %SFD%\SFR
copy %1\System.Runtime.CompilerServices.Unsafe.dll %SFD%\SFR