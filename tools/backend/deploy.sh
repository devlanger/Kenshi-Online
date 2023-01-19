#!/bin/bash
cd ../..

docker-compose build
docker-compose push

cd tools/backend

/bin/bash restart_remote.sh
