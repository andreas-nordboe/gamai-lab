FROM mcr.microsoft.com/dotnet/sdk:10.0.201 AS build

ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["GamAILab.Frontend.Client/GamAILab.Frontend.Client.csproj", "GamAILab.Frontend.Client/"]
COPY ["GamAILab.Shared/GamAILab.Shared.csproj", "GamAILab.Shared/"]
RUN dotnet restore "GamAILab.Frontend.Client/GamAILab.Frontend.Client.csproj"
COPY . .
WORKDIR "/src/GamAILab.Frontend.Client"
RUN dotnet publish "./GamAILab.Frontend.Client.csproj" -c $BUILD_CONFIGURATION -o /app/publish
RUN find /app/publish -maxdepth 3 -type f | head -100

FROM nginx:alpine AS final
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY GamAILab.Frontend.Client/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80

ENTRYPOINT ["nginx", "-g", "daemon off;"]