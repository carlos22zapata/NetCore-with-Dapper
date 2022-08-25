
cmd.exe /c dotnet restore
cmd.exe /c dotnet publish --configuration Release -o ../../ARIS_Net_Build


cd ../

cd ./ARIS_Net_Build
del appsettings.json
cd ..


set /p DUMMY=Hit ENTER to continue...