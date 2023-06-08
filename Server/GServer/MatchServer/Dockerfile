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
COPY --from=publish /src/enterpoint.sh ./enterpoint.sh

ENV DEV_HOST="192.168.1.29"
# 当前镜像可访问IP 对外Ip
ENV HOST_NAME=${DEV_HOST}
# ZK 服务器 注册发现
ENV ZK_SERVER_LIST="${DEV_HOST}:2181"
# kfk
ENV KFK_SERVER_LIST="${DEV_HOST}:9092"

#zk
ENV ZKROOT="/match"
ENV ZKNOTIFY="/notify"
ENV ZKBATTLE="/battle"

# 内部微服务访问IP 
ENV SERVICE_HOST="${DEV_HOST}"

# 数据库服务器链接 mongodb
ENV DBHOST="mongodb://${DEV_HOST}:27017/"


# 1 匹配服务器端口   内部微服务端口 
ENV MAP_MATCH_SERVICE_PORT="1500"


# db mathc
ENV DB_MATCH="match"


#内网端口
EXPOSE 1500 
 

# ENTRYPOINT
ENTRYPOINT   [ "sh" , "enterpoint.sh" ]

#ENTRYPOINT ["dotnet", "MatchServer.dll"]
