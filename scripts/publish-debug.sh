dotnet publish -c Debug
rm -rf ./dist
cp -r ./bin/Debug/net7.0 ./dist