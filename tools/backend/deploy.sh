#!/bin/bash
cd ../..

docker-compose -f docker-compose.yml -f docker-compose.prod.yml build
docker-compose push

cd tools/backend

/bin/bash restart_remote.sh
