EVN_FILE=$1
if [ -z "$IPLOCAL" ]; then
    echo "host is empty use mac local"
    EVN_FILE="./config/env.mac"
fi


echo "Host:${EVN_FILE}"

docker-compose  --env-file ${EVN_FILE} up -d 