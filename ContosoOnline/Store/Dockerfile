#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0-preview AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0-preview AS build
WORKDIR /src
COPY ["Store/Store.csproj", "Store/"]
COPY ["Diagnostics/Diagnostics.csproj", "Diagnostics/"]
RUN dotnet restore "Store/Store.csproj"
COPY . .
WORKDIR "/src/Store"
RUN dotnet build "Store.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Store.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Store.dll"]