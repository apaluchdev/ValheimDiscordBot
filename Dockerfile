#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER root
RUN apt-get update && apt-get install -y iputils-ping && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app

# Environment variables for configuration
ENV ValheimServer__Host=""
ENV ValheimServer__QueryPort="2457"
ENV Bot__StatusUpdateIntervalSeconds="60"
ENV Bot__CommandPrefix="/"

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ValheimDiscordBot.csproj", "."]
RUN dotnet restore "./ValheimDiscordBot.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./ValheimDiscordBot.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ValheimDiscordBot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ValheimDiscordBot.dll"]