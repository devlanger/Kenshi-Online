FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Kenshi.TestGameServer/Kenshi.TestGameServer.csproj", "Kenshi.TestGameServer/"]
RUN dotnet restore "Kenshi.TestGameServer/Kenshi.TestGameServer.csproj"
COPY . .
WORKDIR "/src/Kenshi.TestGameServer"
RUN dotnet build "Kenshi.TestGameServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Kenshi.TestGameServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Kenshi.TestGameServer.dll"]
