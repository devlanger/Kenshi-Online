#! /bin/bash
/usr/local/bin/docker build -t piotrlanger/shindogs '/Users/piotrlanger/Repositories/Kenshi/Builds/'
/usr/local/bin/docker push piotrlanger/shindogs:latest
#ssh debian@146.59.16.113 'docker pull piotrlanger/shindogs:latest'
