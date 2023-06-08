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
COPY --from=publish /src/enterpoint.sh ./enterpoint.sh

ENV DEV_HOST="192.168.1.29"
# 当前镜像可访问IP 对外Ip
ENV HOST_NAME=${DEV_HOST}
# ZK 服务器 注册发现
ENV ZK_SERVER_LIST="${DEV_HOST}:2181"
# kfk
ENV KFK_SERVER_LIST="${DEV_HOST}:9092"

# zk

ENV ZKROOT="/gate"
ENV ZKLOINGROOT="/login"
ENV ZKNOTIFYROOT="/notify"
ENV ZKMATCHROOT="/match"
ENV ZKCONFIG="/configs"

ENV SERVERGROUPID="1"

# 内部微服务访问IP 
ENV SERVICE_HOST="${DEV_HOST}"

# 数据库服务器链接 mongodb
ENV DBHOST="mongodb://${DEV_HOST}:27017/"

# 网关服务器 端口 ZK 发现 用户访问端口 
ENV MAP_GATE_PORT="2001"


#---对内微服务---
# 4 网关服务器 内部微服务端口
ENV MAP_GATE_SERVICE_PORT="1700"

# DB Gate 网关数据库 GATE 
ENV DB_GATE="gate"

ENV MAXPLAYER=100000

ENV GMENABLE=YES

EXPOSE 2001  1700

#CMD [ "dotnet" , "" ]

# 启动类型  1 


# ENTRYPOINT
ENTRYPOINT   [ "sh" , "enterpoint.sh" ]



#ENTRYPOINT ["dotnet", "GateServer.dll"]
