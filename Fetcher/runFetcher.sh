#!/bin/bash

if [[ "$#" -lt 3 ]]; then
    echo "Illegal number of parameters"
    echo "Proper usage: $0 nodeIndex host rpcPort [release]"
    exit 1
fi

if [[ "$#" -eq 4 ]]; then
    # Create Release dlls
    dotnet publish -c Release
fi

RPCHOST=$2
RPCPORT=$3

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

dotnet bin/Release/netcoreapp3.0/Fetcher.dll --sHost ${RPCHOST} --sPort ${RPCPORT} \
    --name ${NAME} -i sample.ini -c contacts.txt
