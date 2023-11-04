#!/bin/bash

if [[ $1 =~ ^v([\\d])* ]]; then
    for d in ./dist/* ; do
        if [ -d "$d" ]; then
            PLATEFORM=${d//.\/dist\/}
            echo "$PLATEFORM"
            tar -czvf "./dist/$PLATEFORM.tar.gz" "./dist/$PLATEFORM"
        fi
    done
    echo release create "$1" ./dist/*.tar.gz
else
    echo "Please specify a version arg0 vX.X.X"
fi