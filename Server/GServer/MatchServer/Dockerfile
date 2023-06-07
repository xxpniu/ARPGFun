FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["MatchServer/MatchServer.csproj", "MatchServer/"]
COPY ["ServerUtility/ServerUtility.csproj", "ServerUtility/"]
COPY ["../../GameCore/XNet/XNet.Libs.csproj", "../../GameCore/XNet/"]
RUN dotnet restore "MatchServer/MatchServer.csproj"
COPY . .
WORKDIR "/src/MatchServer"
RUN dotnet build "MatchServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MatchServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MatchServer.dll"]
