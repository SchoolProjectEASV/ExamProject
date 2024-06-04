#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["projects/User/UserService/UserService.csproj", "projects/User/UserService/"]
COPY ["packages/LoggingService/LoggingService.csproj", "packages/LoggingService/"]
COPY ["packages/TracingService/TracingService.csproj", "packages/TracingService/"]
COPY ["projects/User/UserApplication/UserApplication.csproj", "projects/User/UserApplication/"]
COPY ["projects/User/UserInfrastructure/UserInfrastructure.csproj", "projects/User/UserInfrastructure/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["packages/VaultService/VaultService.csproj", "packages/VaultService/"]
RUN dotnet restore "./projects/User/UserService/UserService.csproj"
COPY . .
WORKDIR "/src/projects/User/UserService"
RUN dotnet build "./UserService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./UserService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserService.dll"]