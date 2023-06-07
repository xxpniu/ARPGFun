
VERSION=$1
OUTPUT=$2

if [ -z "$VERSION" ]; then
    VERSION=latest
    echo "VERSION is empty use ${VERSION}"
fi
if [ -z "$OUTPUT" ]; then
    OUTPUT=`pwd`/publish 
    echo "OUTPUT is empty use ${OUTPUT}"
    
fi

docker build -t builder:${VERSION} .

echo "Begin Build:docker run --rm -v /serveroutput:${OUTPUT}  --name build${VERSION} builder:${VERSION} "
docker run --rm --volume ${OUTPUT}:/serveroutput:  --name build${VERSION} builder:${VERSION} 

docker rm build${VERSION}

docker rmi builder:${VERSION} 
#
echo "Export images"
sh buildimage.sh $VERSION ${OUTPUT}
