VERSION=$1
OUTPUT=$2

if [ -z "$VERSION" ]; then
    echo "VERSION is empty use lastest"
    VERSION=latest
fi

if [ -z "$OUTPUT" ]; then
    echo "OUTPUT is empty use publish"
    OUTPUT=../publish 
fi


echo "version:$VERSION oupt:${OUTPUT}"

echo "Build Login server"
dotnet publish GServer/LoginServer/LoginServer.csproj -o ${OUTPUT}/LoginServer
echo "Build gate server"
dotnet publish GServer/GServer/GateServer.csproj -o ${OUTPUT}/GateServer
echo "Build chat server"
dotnet publish GServer/ChatServer/ChatServer.csproj -o ${OUTPUT}/ChatServer 
echo "Build match server"
dotnet publish GServer/MatchServer/MatchServer.csproj -o ${OUTPUT}/MatchServer
echo "Build notify server"
dotnet publish GServer/NotifyServer/NotifyServer.csproj -o ${OUTPUT}/NotifyServer


# local test

