﻿FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Kenshi.API/Kenshi.API.csproj", "Kenshi.API/"]
RUN dotnet restore "Kenshi.API/Kenshi.API.csproj"
COPY . .
WORKDIR "/src/Kenshi.API"
RUN dotnet build "Kenshi.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Kenshi.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Kenshi.API.dll"]
