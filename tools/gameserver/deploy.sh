#! /bin/bash
scp -rp ../../Builds/GameServer/Build debian@146.59.16.113:/home/debian/kenshigs
ssh debian@146.59.16.113 '/home/debian/kenshigs/Build/deploy.sh'
