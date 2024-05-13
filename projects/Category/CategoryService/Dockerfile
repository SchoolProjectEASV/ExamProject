#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["projects/Category/CategoryService/CategoryService.csproj", "projects/Category/CategoryService/"]
COPY ["packages/LoggingService/LoggingService.csproj", "packages/LoggingService/"]
COPY ["packages/TracingService/TracingService.csproj", "packages/TracingService/"]
COPY ["projects/Category/CategoryApplication/CategoryApplication.csproj", "projects/Category/CategoryApplication/"]
COPY ["projects/Category/CategoryInfrastructure/CategoryInfrastructure.csproj", "projects/Category/CategoryInfrastructure/"]
COPY ["Domain/Domain.csproj", "Domain/"]
RUN dotnet restore "./projects/Category/CategoryService/CategoryService.csproj"
COPY . .
WORKDIR "/src/projects/Category/CategoryService"
RUN dotnet build "./CategoryService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CategoryService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CategoryService.dll"]