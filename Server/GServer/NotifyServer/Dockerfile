FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["NotifyServer/NotifyServer.csproj", "NotifyServer/"]
COPY ["ServerUtility/ServerUtility.csproj", "ServerUtility/"]
COPY ["../../GameCore/XNet/XNet.Libs.csproj", "../../GameCore/XNet/"]
COPY ["ChatTool/ChatTool.csproj", "ChatTool/"]
RUN dotnet restore "NotifyServer/NotifyServer.csproj"
COPY . .
WORKDIR "/src/NotifyServer"
RUN dotnet build "NotifyServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NotifyServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NotifyServer.dll"]
