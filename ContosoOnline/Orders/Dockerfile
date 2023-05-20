FROM mcr.microsoft.com/dotnet/nightly/sdk:8.0-preview-alpine AS build

RUN apk add \
	# Add NativeAOT pre-reqs
	clang build-base zlib-dev

WORKDIR /src
COPY ["NuGet.config", "."]
COPY ["ContosoOnline/Orders/Orders.csproj", "Orders/"]
COPY ["ContosoOnline/Diagnostics/Diagnostics.csproj", "Diagnostics/"]
RUN dotnet restore "Orders/Orders.csproj"
COPY ./ContosoOnline .
WORKDIR "/src/Orders"
RUN dotnet publish "Orders.csproj" -c Release -o /app/publish

# remove the symbols so they aren't part of the published app
RUN rm -rf /app/publish/*.dbg

FROM alpine AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
ENV DOTNET_RUNNING_IN_CONTAINER=true
COPY --from=build /app/publish .
ENTRYPOINT ["./Orders"]