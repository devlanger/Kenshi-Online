# Kenshi Backend Guide:
### 1. How to build server:
1. Open [**<REPO_FOLDER_PATH/KenshiClient>**] project
2. Go to Building->Build Game Server for Ubuntu
3. Wait for the build to finish building

### 2. How to copy server to remote:
1. Execute shell script **[<REPO_FOLDER_PATH>/tools/gameserver/deploy.sh]**
    - It will build & push docker image

### 3. How to restart Backend on remote:
1. Execute shell script **[<REPO_FOLDER_PATH>/tools/backend/restart_remote.sh]**