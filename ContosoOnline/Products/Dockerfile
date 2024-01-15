FROM mcr.microsoft.com/dotnet/aspnet:8.0.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 8080
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0.101 AS build
WORKDIR /src
COPY ["NuGet.config", "."]
COPY ["ContosoOnline/Products/Products.csproj", "Products/"]
COPY ["ContosoOnline/Diagnostics/Diagnostics.csproj", "Diagnostics/"]
RUN dotnet restore "Products/Products.csproj"
COPY ./ContosoOnline .
WORKDIR "/src/Products"
RUN dotnet build "Products.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Products.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Products.dll"]