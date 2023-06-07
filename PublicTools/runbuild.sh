
## copy 

IMAGE=toolsbuilder:latest

docker build . -t ${IMAGE} 

if [ "$?" -ne "0" ]; then
  echo "Failure  check log"
  exit 1
fi


OUT_DIR=`pwd`/output

echo "OutDir:${OUT_DIR}"
echo "image: ${IMAGE}" 
sleep 1
docker run -v $OUT_DIR:/var/output --rm  $IMAGE
if [ "$?" -ne "0" ]; then
  echo "Failure  check log"
  docker rmi ${IMAGE}
  exit 1
fi

echo "Docker build finished, begin copy files"

PROTOC_OUT_DIR=../GameCore/dll
cp -af output/dll/Xsoft.Proto.* $PROTOC_OUT_DIR

CLIENT_OUT_DIR=../client/Packages/com.xsoft.core/Runtime/plugins/CoreDll

cp -af output/dll/Xsoft.Proto.* $CLIENT_OUT_DIR

docker rmi ${IMAGE}