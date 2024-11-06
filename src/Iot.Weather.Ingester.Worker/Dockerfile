FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Iot.Weather.Ingester.Worker/Iot.Weather.Ingester.Worker.csproj", "src/Iot.Weather.Ingester.Worker/"]
COPY ["src/Iot.Weather.Ingester.IoC/Iot.Weather.Ingester.IoC.csproj", "src/Iot.Weather.Ingester.IoC/"]
COPY ["src/Iot.Weather.Ingester.InfluxDb/Iot.Weather.Ingester.InfluxDb.csproj", "src/Iot.Weather.Ingester.InfluxDb/"]
COPY ["src/Iot.Weather.Ingester.Mqtt/Iot.Weather.Ingester.Mqtt.csproj", "src/Iot.Weather.Ingester.Mqtt/"]
RUN dotnet restore "src/Iot.Weather.Ingester.Worker/Iot.Weather.Ingester.Worker.csproj"
COPY . .
WORKDIR "/src/src/Iot.Weather.Ingester.Worker"
RUN dotnet build "Iot.Weather.Ingester.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Iot.Weather.Ingester.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Iot.Weather.Ingester.Worker.dll"]
