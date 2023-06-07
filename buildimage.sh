VERSION=$1
OUTPUT=$2

if [ -z "$VERSION" ]; then
    
    VERSION=latest
    echo "VERSION is empty use ${VERSION}"
fi
if [ -z "$OUTPUT" ]; then
   
    OUTPUT=publish 
    echo "OUTPUT is empty use ${OUTPUT}"
fi

docker rmi game/login:$VERSION 
docker rmi game/gate:$VERSION 
docker rmi game/chat:$VERSION 
docker rmi game/match:$VERSION 
docker rmi game/notify:$VERSION
docker rmi game/battle:$VERSION

echo 'Begin Building'

docker build --no-cache -t game/login:$VERSION ${OUTPUT}/LoginServer
docker build --no-cache -t game/gate:$VERSION ${OUTPUT}/GateServer
docker build --no-cache -t game/chat:$VERSION ${OUTPUT}/ChatServer
docker build --no-cache -t game/match:$VERSION ${OUTPUT}/MatchServer
docker build --no-cache -t game/notify:$VERSION ${OUTPUT}/NotifyServer
docker build --no-cache -t game/battle:$VERSION ${OUTPUT}/BattleServer

#export 

#docker save game/login:$VERSION>${OUTDIR}/login.tar.gz
#docker save game/gate:$VERSION>${OUTDIR}/gate.tar.gz 
#docker save game/chat:$VERSION>${OUTDIR}/chat.tar.gz
#docker save game/match:$VERSION>${OUTDIR}/match.tar.gz 
#docker save game/notify:$VERSION>${OUTDIR}/notify.tar.gz 

