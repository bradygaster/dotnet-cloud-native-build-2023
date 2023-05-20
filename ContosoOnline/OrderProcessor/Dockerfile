FROM mcr.microsoft.com/dotnet/nightly/sdk:8.0-preview-alpine AS build

RUN apk add \
	# Add NativeAOT pre-reqs
	clang build-base zlib-dev \
	# workaround so GRPC protoc works - https://github.com/grpc/grpc/blob/master/src/csharp/BUILD-INTEGRATION.md#using-grpctools-with-unsupported-architectures
	grpc-plugins

ENV PROTOBUF_PROTOC=/usr/bin/protoc
ENV GRPC_PROTOC_PLUGIN=/usr/bin/grpc_csharp_plugin

WORKDIR /src
COPY ["NuGet.config", "."]
COPY ["ContosoOnline/OrderProcessor/OrderProcessor.csproj", "OrderProcessor/"]
COPY ["ContosoOnline/Diagnostics/Diagnostics.csproj", "Diagnostics/"]
RUN dotnet restore "OrderProcessor/OrderProcessor.csproj"
COPY ./ContosoOnline .
WORKDIR "/src/OrderProcessor"
RUN dotnet publish "OrderProcessor.csproj" -c Release -o /app/publish

# remove the symbols so they aren't part of the published app
RUN rm -rf /app/publish/*.dbg

FROM alpine AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["./OrderProcessor"]
