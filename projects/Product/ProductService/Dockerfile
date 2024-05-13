#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["projects/Product/ProductService/ProductService.csproj", "projects/Product/ProductService/"]
COPY ["projects/Product/ProductApplication/ProductApplication.csproj", "projects/Product/ProductApplication/"]
COPY ["projects/Product/ProductInfrastructure/ProductInfrastructure.csproj", "projects/Product/ProductInfrastructure/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["packages/VaultService/VaultService.csproj", "packages/VaultService/"]
RUN dotnet restore "./projects/Product/ProductService/ProductService.csproj"
COPY . .
WORKDIR "/src/projects/Product/ProductService"
RUN dotnet build "./ProductService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ProductService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProductService.dll"]