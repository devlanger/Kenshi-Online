FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Kenshi.Backend.GameServer/Kenshi.Backend.GameServer.csproj", "Kenshi.Backend.GameServer/"]
RUN dotnet restore "Kenshi.Backend.GameServer/Kenshi.Backend.GameServer.csproj"
COPY . .
WORKDIR "/src/Kenshi.Backend.GameServer"
RUN dotnet build "Kenshi.Backend.GameServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Kenshi.Backend.GameServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Kenshi.Backend.GameServer.dll"]
