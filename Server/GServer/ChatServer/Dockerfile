FROM mcr.microsoft.com/dotnet/runtime:3.1  as builder


ENV DEV_HOST="192.168.1.29"
# 当前镜像可访问IP 对外Ip
ENV HOST_NAME=${DEV_HOST}
# ZK 服务器 注册发现
ENV ZK_SERVER_LIST="${DEV_HOST}:2181"
# zk root 
ENV ZK_ROOT="/chat"
ENV ZK_LOGIN="/login"

# kfk
ENV KFK_SERVER_LIST="${DEV_HOST}:9092"

# 内部微服务访问IP 
ENV SERVICE_HOST="${DEV_HOST}"

# 数据库服务器链接 mongodb
ENV DBHOST="mongodb://${DEV_HOST}:27017/"

# 聊天服务器端口 zk 发现 用户访问端口 
ENV MAP_CHAT_PORT="2200"

#---对内微服务---

# 5 聊天服务器端口 内部微服务端口
ENV MAP_CHAT_SERVICE_PORT="1522"

# 聊天数据库 
ENV DB_CHAT="chat"
# 最大用户数
ENV MAX_PLAYER=4000
ENV SERVERID=1000


EXPOSE 2200  1522

 

#CMD [ "dotnet" , "" ]

# 启动类型  1 


COPY . /game/chat
WORKDIR /game/chat


# ENTRYPOINT
ENTRYPOINT   [ "sh" , "enterpoint.sh" ]

#CMD ["sh" , "taillog.sh"]

