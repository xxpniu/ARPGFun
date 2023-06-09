FROM mcr.microsoft.com/dotnet/runtime:3.1  as builder


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

# 启动类型  1 


COPY . /game/notify
WORKDIR /game/notify

# ENTRYPOINT
ENTRYPOINT   [ "sh" , "enterpoint.sh" ]


