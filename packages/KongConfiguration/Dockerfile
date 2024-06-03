# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["packages/KongConfiguration/KongSetup.csproj", "packages/KongConfiguration/"]
RUN dotnet restore "./packages/KongConfiguration/KongSetup.csproj"
COPY . .
WORKDIR "/src/packages/KongConfiguration"
RUN dotnet build "./KongSetup.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./KongSetup.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Copy the kong-config.json and appsettings.json files to the final image
COPY ["packages/KongConfiguration/kong-config.json", "packages/KongConfiguration/appsettings.json", "./"]
ENTRYPOINT ["dotnet", "KongSetup.dll"]
