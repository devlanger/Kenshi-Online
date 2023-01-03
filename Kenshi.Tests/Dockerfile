FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Kenshi.Tests/Kenshi.Tests.csproj", "Kenshi.Tests/"]
RUN dotnet restore "Kenshi.Tests/Kenshi.Tests.csproj"
COPY . .
WORKDIR "/src/Kenshi.Tests"
RUN dotnet build "Kenshi.Tests.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Kenshi.Tests.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Kenshi.Tests.dll"]
