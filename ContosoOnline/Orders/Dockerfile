FROM mcr.microsoft.com/dotnet/aspnet:8.0-preview AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0-preview AS build
WORKDIR /src
COPY ["Orders/Orders.csproj", "Orders/"]
RUN dotnet restore "Orders/Orders.csproj"
COPY . .
WORKDIR "/src/Orders"
RUN dotnet build "Orders.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Orders.csproj" -c Release -o /app/publish /p:UseAppHost=true

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Orders.dll"]