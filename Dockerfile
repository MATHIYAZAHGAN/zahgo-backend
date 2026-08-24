# Build Stage - Using .NET 9 as .NET 10 images don't exist yet
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["src/ZAH.API/ZAH.API.csproj", "ZAH.API/"]
COPY ["src/ZAH.Application/ZAH.Application.csproj", "ZAH.Application/"]
COPY ["src/ZAH.Domain/ZAH.Domain.csproj", "ZAH.Domain/"]
COPY ["src/ZAH.Infrastructure/ZAH.Infrastructure.csproj", "ZAH.Infrastructure/"]
COPY ["src/ZAH.Shared/ZAH.Shared.csproj", "ZAH.Shared/"]

# Restore dependencies
RUN dotnet restore "ZAH.API/ZAH.API.csproj"

# Copy all source code
COPY src/ .

# Build and publish
WORKDIR "/src/ZAH.API"
RUN dotnet publish "ZAH.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy published app
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080

# Set environment
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "ZAH.API.dll"]
