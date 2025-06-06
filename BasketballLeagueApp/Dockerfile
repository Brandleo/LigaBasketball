# Imagen base .NET para ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Imagen SDK para compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar proyecto y restaurar dependencias
COPY BasketballLeagueApp/BasketballLeagueApp.csproj BasketballLeagueApp/
RUN dotnet restore BasketballLeagueApp/BasketballLeagueApp.csproj

# Copiar todo el código
COPY BasketballLeagueApp/. ./BasketballLeagueApp

# Publicar
WORKDIR /src/BasketballLeagueApp
RUN dotnet publish -c Release -o /app/publish

# Imagen final
FROM base AS final
WORKDIR /app

# Copiar aplicación publicada
COPY --from=build /app/publish .

# Añadir permisos para ejecutar libwkhtmltox (para DinkToPdf)
RUN apt-get update && apt-get install -y libgdiplus libc6-dev libx11-dev libxext6 libxrender1 libfontconfig1 libfreetype6 && \
    apt-get clean && rm -rf /var/lib/apt/lists/*

# Copiar wkhtmltox si lo tienes local (opcional, o usar desde código)
# COPY libwkhtmltox.so /app/

ENTRYPOINT ["dotnet", "BasketballLeagueApp.dll"]
