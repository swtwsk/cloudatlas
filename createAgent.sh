#!/bin/bash

if [[ "$#" -ne 7 ]]; then
    echo "Illegal number of parameters"
    echo "Proper usage: $0 nodeIndex recvHost recvPort rpcHost rpcPort fetchHost fetchPort"
    exit 1
fi

RECVHOST=$2
RECVPORT=$3
RPCHOST=$4
RPCPORT=$5
FETCHHOST=$6
FETCHPORT=$7

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

cd Fetcher; dotnet run -- --sHost ${RPCHOST} --sPort ${RPCPORT} --name ${NAME} -i sample.ini -c contacts.txt & cd ..
cd CloudAtlasAgent; dotnet run -- -k ../QuerySigner/rsa.pub -i ${INI_FILE} -n ${NAME} \
    -h ${RECVHOST} -p ${RECVPORT} --rpc ${RPCHOST} --rpcport ${RPCPORT} & cd ..
