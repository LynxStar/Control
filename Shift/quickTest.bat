cd ../
dotnet build  >nul
cd ./Shift/bin./Debug./net5.0/
dotnet Shift.dll >nul
cd ./compile/bin./Debug./net5.0/
dotnet ShiftExample.dll
cd ../../../../../../../