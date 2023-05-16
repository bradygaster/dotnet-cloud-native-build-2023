FROM mcr.microsoft.com/dotnet/aspnet:8.0-preview AS base
WORKDIR /app
EXPOSE 80
EXPOSE 8080
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0-preview AS build

# Add NativeAOT pre-reqs
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
    clang \
    zlib1g-dev

WORKDIR /src
COPY ["Products/Products.csproj", "Products/"]
COPY ["Diagnostics/Diagnostics.csproj", "Diagnostics/"]
RUN dotnet restore "Products/Products.csproj"
COPY . .
WORKDIR "/src/Products"
RUN dotnet build "Products.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Products.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["./Products"]