FROM mcr.microsoft.com/dotnet/sdk:8.0-preview-alpine AS build

RUN apk add \
	# Add NativeAOT pre-reqs
	clang build-base zlib-dev

WORKDIR /src
COPY ["Orders/Orders.csproj", "Orders/"]
COPY ["Diagnostics/Diagnostics.csproj", "Diagnostics/"]
RUN dotnet restore "Orders/Orders.csproj"
COPY . .
WORKDIR /src/Orders
RUN dotnet publish "Orders.csproj" -o /app/publish

# remove the symbols so they aren't part of the published app
RUN rm -rf /app/publish/*.dbg

FROM alpine AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
ENV DOTNET_RUNNING_IN_CONTAINER=true
COPY --from=build /app/publish .
ENTRYPOINT ["./Orders"]