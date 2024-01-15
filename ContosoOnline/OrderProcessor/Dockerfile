FROM mcr.microsoft.com/dotnet/aspnet:8.0.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0.101 AS build
WORKDIR /src
COPY ["NuGet.config", "."]
COPY ["ContosoOnline/OrderProcessor/OrderProcessor.csproj", "OrderProcessor/"]
COPY ["ContosoOnline/Diagnostics/Diagnostics.csproj", "Diagnostics/"]
RUN dotnet restore "OrderProcessor/OrderProcessor.csproj"
COPY ./ContosoOnline .
WORKDIR "/src/OrderProcessor"
RUN dotnet build "OrderProcessor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OrderProcessor.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderProcessor.dll"]