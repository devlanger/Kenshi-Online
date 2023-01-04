FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Kenshi.TestClient/Kenshi.TestClient.csproj", "Kenshi.TestClient/"]
RUN dotnet restore "Kenshi.TestClient/Kenshi.TestClient.csproj"
COPY . .
WORKDIR "/src/Kenshi.TestClient"
RUN dotnet build "Kenshi.TestClient.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Kenshi.TestClient.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Kenshi.TestClient.dll"]
