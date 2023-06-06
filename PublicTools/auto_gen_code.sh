cd ./ToolBin

#cp -afv /Users/xiexiongping/Google\ 云端硬盘/MultplayerGame/Excel/*.xlsx /Users/xiexiongping/Documents/github/version/PublicTools/econfigs

IMPORT_PATH=../proto/
CSHARP_PATH=../src/csharp
CSHARP_OUT_PATH=$CSHARP_PATH
VERSION="$1"
echo $VERSION
protoc ../proto/*.proto -I=$IMPORT_PATH \
    --csharp_out=$CSHARP_OUT_PATH \
    --plugin=protoc-gen-grpc=/usr/local/Cellar/grpc/1.30.0/bin/grpc_csharp_plugin \
    --grpc_out=$CSHARP_OUT_PATH \
    #--grpc_opt=lite_client,no_server \
    
if [ "$?" -ne "0" ]; then
  echo "Failur  check proto files"
  exit 1
fi



#mono PServicePugin.exe dir:../proto file:*.proto saveto:$CSHARP_OUT_PATH version:$VERSION
#if [ "$?" -ne "0" ]; then
#  echo "Sorry, check service define"
#  exit 1
#fi

mono ./ExcelOut.exe dir:../econfigs namespace:EConfig exportJson:../src/json/ exportCs:$CSHARP_OUT_PATH/ExcelConfig.cs ex:*.xlsx debug:false
if [ "$?" -ne "0" ]; then
  echo "Sorry, check excel files "
  exit 1
fi

cd ../src/csharp
dotnet publish -o ../output/
if [ "$?" -ne "0" ]; then
  echo "Sorry, compile error"
  exit 1
fi
cd ../../../

cd ./GameCore/XNet
dotnet publish -o ../../PublicTools/src/output/
if [ "$?" -ne "0" ]; then
  echo "Sorry, compile error"
  exit 1
fi
cd ../JSON
dotnet publish -o ../../PublicTools/src/output/
if [ "$?" -ne "0" ]; then
  echo "Sorry, compile error"
  exit 1
fi


cd ../../PublicTools/ToolBin


cp -af ../src/output/*.dll  ../../client/Packages/Simulater.Core/Runtime/plugins/CoreDll/
if [ "$?" -ne "0" ]; then
  echo "Sorry, copy dll failure"
  exit 1
fi

cp -af ../src/json/  ../../client/Assets/Resources/Json/
#cp -af ../src/json/  ../../Server/Configs/

if [ "$?" -ne "0" ]; then
  echo "Sorry, copy json to client failure"
  exit 1
fi

cd ../

python3 uploadzk.py --host 129.211.9.75:2181 --root /configs --dir ./src/json
if [ "$?" -ne "0" ]; then
  echo "Sorry, upload to zk failure"
  exit 1
fi
