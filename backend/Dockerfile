FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Restore dependencies first (layer caching)
COPY Trisecmed.slnx .
COPY src/Trisecmed.Domain/Trisecmed.Domain.csproj src/Trisecmed.Domain/
COPY src/Trisecmed.Application/Trisecmed.Application.csproj src/Trisecmed.Application/
COPY src/Trisecmed.Infrastructure/Trisecmed.Infrastructure.csproj src/Trisecmed.Infrastructure/
COPY src/Trisecmed.API/Trisecmed.API.csproj src/Trisecmed.API/
RUN dotnet restore src/Trisecmed.API/Trisecmed.API.csproj

# Build and publish
COPY . .
RUN dotnet publish src/Trisecmed.API/Trisecmed.API.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/out .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Trisecmed.API.dll"]
