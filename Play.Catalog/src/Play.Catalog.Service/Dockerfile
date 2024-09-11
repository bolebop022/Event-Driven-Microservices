#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 5001
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Play.Catalog.Service.csproj", "."]
RUN dotnet restore "./Play.Catalog.Service.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./Play.Catalog.Service.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Play.Catalog.Service.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN apt-get update && apt-get install -y wget
RUN wget https://packages.microsoft.com/config/debian/11/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN apt-get update && apt-get install -y dotnet-sdk-5.0

ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS="https://+:5001;http://+:5000"
ENV ASPNETCORE_Kestrel__Certificates__Default__Password="password123!"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx

RUN echo '#!/bin/bash\n\
mkdir -p /https && \
dotnet dev-certs https -ep /https/aspnetapp.pfx -p password123! && \
dotnet Play.Catalog.Service.dll' > /app/start.sh && \
chmod +x /app/start.sh

ENTRYPOINT ["/app/start.sh"]
