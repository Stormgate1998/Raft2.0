#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["FrontEndRaft/FrontEndRaft.csproj", "FrontEndRaft/"]

RUN dotnet clean "./FrontEndRaft/./FrontEndRaft.csproj"
RUN dotnet restore "./FrontEndRaft/./FrontEndRaft.csproj"
COPY . .
WORKDIR "/src/FrontEndRaft"
RUN dotnet build "./FrontEndRaft.csproj" -c $BUILD_CONFIGURATION -o /app/build

ARG GATEWAY_URL_ARG
ARG IMAGE_GATEWAY_URL_ARG
ARG SIGNALR_HUB_URL_ARG
ENV API_URL=$GATEWAY_URL_ARG
RUN sed -i "s|\${API_URL}|$API_URL|g" wwwroot/appsettings.json

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FrontEndRaft.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM nginx:alpine
WORKDIR /user/share/nginx/html
COPY --from=publish /app/publish/wwwroot .
