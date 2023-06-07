 
IMPORT_PATH=./proto/
CSHARP_PATH=./src/csharp
CSHARP_OUT_PATH=$CSHARP_PATH
VERSION="$1"
echo $VERSION
protoc ./proto/*.proto -I=$IMPORT_PATH \
    --csharp_out=$CSHARP_OUT_PATH \
    --plugin=protoc-gen-grpc=`which grpc_csharp_plugin` \
    --grpc_out=$CSHARP_OUT_PATH \
    #--grpc_opt=lite_client,no_server \
    
if [ "$?" -ne "0" ]; then
  echo "Failur  check proto files"
  exit 1
fi

python3 ./pys/process_excel.py -f ./econfigs/ -o ./src/csharp/ExcelConfig.cs -d ./src/json/  -n EConfig 
 
if [ "$?" -ne "0" ]; then
  echo "Sorry, check excel files "
  exit 1
fi

cd ./src/csharp
dotnet publish -o ../output/
if [ "$?" -ne "0" ]; then
  echo "Sorry, compile error"
  exit 1
fi

cd ../../

cp -af ./src/json /var/output/json
cp -af ./src/output /var/output/dll
 

#python3 uploadzk.py --host 129.211.9.75:2181 --root /configs --dir ./src/json
if [ "$?" -ne "0" ]; then
  echo "Sorry, Copy failure!"
  exit 1
fi

echo "Copy to out dll"