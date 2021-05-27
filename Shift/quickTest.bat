cd ../ >nul 2>&1
dotnet build  >nul 2>&1
cd ./Shift/bin./Debug./net5.0/ >nul 2>&1
dotnet Shift.dll >nul 2>&1
cd ./compile/bin./Debug./net5.0/ >nul 2>&1
dotnet ShiftExample.dll
cd ../../../../../../../ >nul 2>&1