# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Kopiuj csproj wszystkich projektów i przywróć pakiety
COPY ["Trisecmed.API/Trisecmed.API.csproj", "Trisecmed.API/"]
COPY ["Trisecmed.Infrastructure/Trisecmed.Infrastructure.csproj", "Trisecmed.Infrastructure/"]
COPY ["Trisecmed.Application/Trisecmed.Application.csproj", "Trisecmed.Application/"]
COPY ["Trisecmed.Domain/Trisecmed.Domain.csproj", "Trisecmed.Domain/"]

RUN dotnet restore "Trisecmed.API/Trisecmed.API.csproj"

# Kopiuj cały kod
COPY . .

# Publikacja API
WORKDIR /src/Trisecmed.API
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Trisecmed.API.dll"]