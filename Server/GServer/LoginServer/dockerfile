FROM mcr.microsoft.com/dotnet/runtime:3.1  as builder


ENV DEV_HOST="192.168.1.29"
# 当前镜像可访问IP 对外Ip
ENV HOST_NAME=${DEV_HOST}
# ZK 服务器 注册发现
ENV ZK_SERVER_LIST="${DEV_HOST}:2181"
# kfk
ENV KFK_SERVER_LIST="${DEV_HOST}:9092"

#zk 
ENV ZKROOT="/login"
ENV ZKGATE="/gate"
ENV ZKCHAT="/chat"

# 内部微服务访问IP 
ENV SERVICE_HOST="${DEV_HOST}"

# 数据库服务器链接 mongodb
ENV DBHOST="mongodb://${DEV_HOST}:27017/"

# 中心 登录/注册 服务器 Port zk 用户访问端口
ENV MAP_LOGIN_PORT="9000"


#---对内微服务---

# 3 中心服务器登陆验证 内部微服务端口
ENV MAP_LOGIN_SERVICE_PORT="1800"



# Account 账号数据库 
ENV DB_ACCOUNT="CenterAccount"


EXPOSE 9000 1800


COPY . /game/login
WORKDIR /game/login


#game 
ENTRYPOINT   [ "sh" , "enterpoint.sh" ]

#ENTRYPOINT [ "tail", "-f", "dev/null" ]

