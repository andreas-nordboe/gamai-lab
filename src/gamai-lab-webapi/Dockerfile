FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["gamai-lab-webapi/gamai-lab-webapi.csproj", "gamai-lab-webapi/"]
RUN dotnet restore "gamai-lab-webapi/gamai-lab-webapi.csproj"
COPY . .
WORKDIR "/src/gamai-lab-webapi"
RUN dotnet build "./gamai-lab-webapi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./gamai-lab-webapi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "gamai-lab-webapi.dll"]
