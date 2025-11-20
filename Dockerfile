# ============================
#   BUILD STAGE
# ============================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy full solution
COPY . .

# Restore dependencies
RUN dotnet restore CareerNexus/CareerNexus.csproj

# Publish in Release mode
RUN dotnet publish CareerNexus/CareerNexus.csproj -c Release -o /app/publish


# ============================
#   RUNTIME STAGE
# ============================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/publish .

# Railway injects PORT env variable → bind Kestrel to it
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}

# Run the Web API
ENTRYPOINT ["dotnet", "CareerNexus.dll"]
