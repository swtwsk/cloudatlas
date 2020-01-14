#!/bin/bash

if [[ "$#" -lt 4 ]]; then
    echo "Illegal number of parameters"
    echo "Proper usage: $0 nodeIndex host recvPort rpcPort [release]"
    exit 1
fi

if [[ "$#" -eq 5 ]]; then
    # Create Release dlls
    dotnet publish -c Release
fi

RECVHOST=$2
RECVPORT=$3
RPCPORT=$4

NAME=""
INI_FILE="random.ini"

case "$1" in
    "1") NAME="/uw/wazniak" ;;
    "2") NAME="/uw/smurf"; INI_FILE="roundrobin.ini" ;;
    "3") NAME="/uw/students" ;;
    "4") NAME="/pw/galera"; INI_FILE="roundrobin.ini" ;;
    "5") NAME="/pw/eres" ;;
    *) NAME="/wrong/number" ;;
esac

dotnet bin/Release/netcoreapp3.0/CloudAtlasAgent.dll -k ../QuerySigner/rsa.pub \
    -i ${INI_FILE} -n ${NAME} -h ${RECVHOST} -p ${RECVPORT} \
    --rpc ${RECVHOST} --rpcport ${RPCPORT} && fg
