FROM  mcr.microsoft.com/dotnet/sdk:3.1 as sdk

COPY ./Server/ /Builds/Server 
COPY ./PublicTools /Builds/PublicTools
COPY ./GameCore /Builds/GameCore

WORKDIR /Builds/Server


ENTRYPOINT [ "sh", "buildserver.sh" , "latest", "/serveroutput"]



