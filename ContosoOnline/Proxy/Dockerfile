FROM mcr.microsoft.com/dotnet/nightly/aspnet:8.0-preview AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/nightly/sdk:8.0-preview AS build
WORKDIR /src
COPY ["NuGet.config", "."]
COPY ["ContosoOnline/Proxy/Proxy.csproj", "Proxy/"]
COPY ["ContosoOnline/Diagnostics/Diagnostics.csproj", "Diagnostics/"]
RUN dotnet restore "Proxy/Proxy.csproj"
COPY ./ContosoOnline .
WORKDIR "/src/Proxy"
RUN dotnet build "Proxy.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Proxy.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Proxy.dll"]