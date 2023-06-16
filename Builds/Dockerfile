# Use a base image suitable for your game server
FROM --platform=linux/x86_64 ubuntu:20.04

COPY . .

# Set the entry point for the container
CMD ["./gameserver/gameserver.x86_64", "-logfile"]