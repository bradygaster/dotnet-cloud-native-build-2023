FROM mcr.microsoft.com/dotnet/sdk:8.0-preview-alpine AS build

RUN apk add \
	# Add NativeAOT pre-reqs
	clang build-base zlib-dev \
	# workaround so GRPC protoc works - https://github.com/grpc/grpc/blob/master/src/csharp/BUILD-INTEGRATION.md#using-grpctools-with-unsupported-architectures
	grpc-plugins

ENV PROTOBUF_PROTOC=/usr/bin/protoc
ENV GRPC_PROTOC_PLUGIN=/usr/bin/grpc_csharp_plugin

WORKDIR /src
COPY ["OrderProcessor/OrderProcessor.csproj", "OrderProcessor/"]
COPY ["OrderProcessor/NuGet.config", "OrderProcessor/"]
COPY ["Diagnostics/Diagnostics.csproj", "Diagnostics/"]
RUN dotnet restore "OrderProcessor/OrderProcessor.csproj"
COPY . .
WORKDIR /src/OrderProcessor
RUN dotnet publish "OrderProcessor.csproj" -o /app/publish

# remove the symbols so they aren't part of the published app
RUN rm -rf /app/publish/*.dbg

FROM alpine AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["./OrderProcessor"]
