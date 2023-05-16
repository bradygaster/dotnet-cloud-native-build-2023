FROM mcr.microsoft.com/dotnet/aspnet:8.0-preview AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0-preview AS build

# Add NativeAOT pre-reqs
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
    clang \
    zlib1g-dev

WORKDIR /src
COPY ["OrderProcessor/OrderProcessor.csproj", "OrderProcessor/"]
COPY ["Diagnostics/Diagnostics.csproj", "Diagnostics/"]
RUN dotnet restore "OrderProcessor/OrderProcessor.csproj"
COPY . .
WORKDIR "/src/OrderProcessor"
RUN dotnet build "OrderProcessor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OrderProcessor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["./OrderProcessor"]