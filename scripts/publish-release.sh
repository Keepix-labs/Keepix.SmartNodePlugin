# OSX
dotnet publish -c Release --runtime osx-x64
dotnet publish -c Release --runtime osx-arm64
# Linux
dotnet publish -c Release --runtime linux-x64
dotnet publish -c Release --runtime linux-arm64
# Windows
dotnet publish -c Release --runtime win-x64
dotnet publish -c Release --runtime win-arm64
rm -rf ./dist
cp -r ./bin/Release/net7.0 ./dist