FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["GServer/GateServer.csproj", "GServer/"]
COPY ["ServerUtility/ServerUtility.csproj", "ServerUtility/"]
COPY ["../../GameCore/XNet/XNet.Libs.csproj", "../../GameCore/XNet/"]
RUN dotnet restore "GServer/GateServer.csproj"
COPY . .
WORKDIR "/src/GServer"
RUN dotnet build "GateServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GateServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GateServer.dll"]
