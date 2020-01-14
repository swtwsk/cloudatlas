#!/bin/bash

if [[ "$#" -ne 3 ]]; then
    echo "Illegal number of parameters"
    echo "Proper usage: $0 name host port"
    exit 1
fi

NAME=$1
HOST=$2
PORT=$3

echo "contacts : set of 1 contact={$NAME $HOST $PORT}" > contacts.txt
