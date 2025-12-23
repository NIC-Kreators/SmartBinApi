FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
ARG PORT=8080
ARG BUILD_ID=None
ENV BUILD_ID=$BUILD_ID
ENV ASPNETCORE_HTTP_PORTS=$PORT
EXPOSE $PORT

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/SmartBin.Api/SmartBin.Api.csproj", "SmartBin.Api/"]
COPY ["src/SmartBin.Infrastructure/SmartBin.Infrastructure.csproj", "SmartBin.Infrastructure/"]
COPY ["src/SmartBin.Application/SmartBin.Application.csproj", "SmartBin.Application/"]
COPY ["src/SmartBin.Domain/SmartBin.Domain.csproj", "SmartBin.Domain/"]
RUN dotnet restore "SmartBin.Api/SmartBin.Api.csproj"
COPY src/ .
WORKDIR "/src/SmartBin.Api"
RUN dotnet build "SmartBin.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SmartBin.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY global-bundle.pem .
ARG ENVIRONMENT=Production
ENV ASPNETCORE_ENVIRONMENT=$ENVIRONMENT

ENTRYPOINT ["dotnet", "SmartBin.Api.dll"]
