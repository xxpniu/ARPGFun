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

COPY --from=publish /src/enterpoint.sh ./enterpoint.sh


ENV DEV_HOST="192.168.1.29"
# 当前镜像可访问IP 对外Ip
ENV HOST_NAME=${DEV_HOST}
# ZK 服务器 注册发现
ENV ZK_SERVER_LIST="${DEV_HOST}:2181"
# kfk
ENV KFK_SERVER_LIST="${DEV_HOST}:9092"

# zk root 

ENV CHAT_ROOT="/chat"

ENV ZK_ROOT="/notify"

# 内部微服务访问IP 
ENV SERVICE_HOST="${DEV_HOST}"

# 数据库服务器链接 mongodb
ENV DBHOST="mongodb://${DEV_HOST}:27017/"

# 2 通知服务器  内部微服务端口
ENV MAP_NOTIFY_SERVICE_PORT="1300"

# 聊天数据库 
ENV DB_CHAT="chat"
#内网端口
EXPOSE  1300 
 

#CMD [ "dotnet" , "" ]


# ENTRYPOINT
ENTRYPOINT   [ "sh" , "enterpoint.sh" ]



