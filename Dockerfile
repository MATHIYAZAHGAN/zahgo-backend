# Build Stage - Using .NET 8 for better MongoDB driver compatibility
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Install ca-certificates for SSL/TLS
RUN apt-get update && apt-get install -y ca-certificates && update-ca-certificates

# Copy everything
COPY . .

# Restore and build from the API project
WORKDIR /app/src/ZAH.API
RUN dotnet restore
RUN dotnet publish -c Release -o /app/out

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install ca-certificates for SSL/TLS
RUN apt-get update && apt-get install -y ca-certificates && update-ca-certificates && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=build /app/out .

# Expose port
EXPOSE 8080

# Set environment
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
ENV DOTNET_RUNNING_IN_CONTAINER=true

ENTRYPOINT ["dotnet", "ZAH.API.dll"]
