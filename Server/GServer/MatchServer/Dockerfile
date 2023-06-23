FROM mcr.microsoft.com/dotnet/runtime:3.1  as builder


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

ENV JENKINS_URL="http://andew25a.synology.me:2080/generic-webhook-trigger/invoke"


#内网端口
EXPOSE 1500 
 

#CMD [ "dotnet" , "" ]

# 启动类型  1 


COPY . /game/match
WORKDIR /game/match

# ENTRYPOINT
ENTRYPOINT   [ "sh" , "enterpoint.sh" ]

#CMD ["sh" , "taillog.sh"]

