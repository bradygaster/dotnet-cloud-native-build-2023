FROM mcr.microsoft.com/dotnet/sdk:8.0-preview-alpine AS build

RUN apk add \
	# Add NativeAOT pre-reqs
	clang build-base zlib-dev \
	# workaround so GRPC protoc works - https://github.com/grpc/grpc/blob/master/src/csharp/BUILD-INTEGRATION.md#using-grpctools-with-unsupported-architectures
	grpc-plugins

ENV PROTOBUF_PROTOC=/usr/bin/protoc
ENV GRPC_PROTOC_PLUGIN=/usr/bin/grpc_csharp_plugin

WORKDIR /src
COPY ["Products/Products.csproj", "Products/"]
COPY ["Diagnostics/Diagnostics.csproj", "Diagnostics/"]
RUN dotnet restore "Products/Products.csproj"
COPY . .
WORKDIR /src/Products
RUN dotnet publish "Products.csproj" -o /app/publish

# remove the symbols so they aren't part of the published app
RUN rm -rf /app/publish/*.dbg

FROM alpine AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
ENV DOTNET_RUNNING_IN_CONTAINER=true
EXPOSE 80
EXPOSE 8080
EXPOSE 443
COPY --from=build /app/publish .
ENTRYPOINT ["./Products"]
