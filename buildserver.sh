
echo "Build Login server"
dotnet publish Server/GServer/LoginServer/LoginServer.csproj -o publish/LoginServer 
echo "Build gate server"
dotnet publish Server/GServer/GServer/GateServer.csproj -o publish/GateServer  
echo "Build chat server"
dotnet publish Server/GServer/ChatServer/ChatServer.csproj -o publish/ChatServer 
echo "Build match server"
dotnet publish Server/GServer/MatchServer/MatchServer.csproj -o publish/MatchServer 
echo "Build notify server"
dotnet publish Server/GServer/NotifyServer/NotifyServer.csproj -o publish/NotifyServer 