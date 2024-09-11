#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Use ASP.NET runtime image for the base layer
FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 5005
EXPOSE 5004

# Use a build-specific image for compilation
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Play.Inventory.Service.csproj", "."]
RUN dotnet restore "./Play.Inventory.Service.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./Play.Inventory.Service.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Play.Inventory.Service.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Use the base image for the final runtime
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install required packages for dotnet SDK
RUN apt-get update && apt-get install -y wget
RUN wget https://packages.microsoft.com/config/debian/11/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN apt-get update && apt-get install -y dotnet-sdk-5.0

# Set environment variables for HTTPS setup
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS="https://+:5005;http://+:5004"
ENV ASPNETCORE_Kestrel__Certificates__Default__Password="password123!"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx

# Create the self-signed certificate and prepare the start script
RUN echo '#!/bin/bash\n\
mkdir -p /https && \
dotnet dev-certs https -ep /https/aspnetapp.pfx -p password123! && \
dotnet Play.Inventory.Service.dll' > /app/start.sh && \
chmod +x /app/start.sh

# Use ENTRYPOINT to start the app using the custom start script
ENTRYPOINT ["/app/start.sh"]

