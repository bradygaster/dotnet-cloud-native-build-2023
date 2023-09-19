FROM mcr.microsoft.com/dotnet/aspnet:8.0.0-rc.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0.100-rc.1 AS build
WORKDIR /src
COPY ["NuGet.config", "."]
COPY ["ContosoOnline/Orders/Orders.csproj", "Orders/"]
COPY ["ContosoOnline/Diagnostics/Diagnostics.csproj", "Diagnostics/"]
RUN dotnet restore "Orders/Orders.csproj"
COPY ./ContosoOnline .
WORKDIR "/src/Orders"
RUN dotnet build "Orders.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Orders.csproj" -c Release -o /app/publish /p:UseAppHost=true

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "Orders.dll"]