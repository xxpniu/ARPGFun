
## copy 

IMAGE=toolsbuilder:latest

docker build . -f dockerfile -t ${IMAGE} 

if [ "$?" -ne "0" ]; then
  echo "Failure  check log"
  exit 1
fi


OUT_DIR=`pwd`/output

echo $OUT_DIR 

docker run -v $OUT_DIR:/var/output  -t  -i --rm ${IMAGE} 
if [ "$?" -ne "0" ]; then
  echo "Failure  check log"
  exit 1
fi

PROTOC_OUT_DIR=../GameCore/dll
cp -af output/dll/* $PROTOC_OUT_DIR

CLIENT_OUT_DIR=../client/Packages/com.xsoft.core/plugins/CoreDll

cp -af output/dll/Xsoft.Proto.* $CLIENT_OUT_DIR

docker rmi ${IMAGE}