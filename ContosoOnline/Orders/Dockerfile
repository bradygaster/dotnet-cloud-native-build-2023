FROM mcr.microsoft.com/dotnet/aspnet:8.0-preview AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0-preview AS build

# Add NativeAOT pre-reqs
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
    clang \
    zlib1g-dev

WORKDIR /src
COPY ["Orders/Orders.csproj", "Orders/"]
COPY ["Diagnostics/Diagnostics.csproj", "Diagnostics/"]
RUN dotnet restore "Orders/Orders.csproj"
COPY . .
WORKDIR "/src/Orders"
RUN dotnet build "Orders.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Orders.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER $APP_UID
ENTRYPOINT ["./Orders"]