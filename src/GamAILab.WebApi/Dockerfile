FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["GamAILab.WebApi/GamAILab.WebApi.csproj", "GamAILab.WebApi/"]
COPY ["GamAILab.Shared/GamAILab.Shared.csproj", "GamAILab.Shared/"]
RUN dotnet restore "GamAILab.WebApi/GamAILab.WebApi.csproj"
COPY . .
WORKDIR "/src/GamAILab.WebApi"
RUN dotnet build "./GamAILab.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./GamAILab.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
    

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GamAILab.WebApi.dll"]

USER root

RUN apt-get update && apt-get install -y --no-install-recommends docker.io && docker --version && rm -rf /var/lib/apt/lists/*