# Imagen base .NET para ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Imagen SDK para compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar el archivo .csproj
COPY BasketballLeagueApp.csproj ./
RUN dotnet restore BasketballLeagueApp.csproj

# Copiar el resto del proyecto
COPY . ./

# Publicar
RUN dotnet publish -c Release -o /app/publish

# Imagen final
FROM base AS final
WORKDIR /app

# Copiar aplicación publicada
COPY --from=build /app/publish .

# Instalar dependencias para DinkToPdf (wkhtmltopdf)
RUN apt-get update && apt-get install -y libgdiplus libc6-dev libx11-dev libxext6 libxrender1 libfontconfig1 libfreetype6 && \
    apt-get clean && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["dotnet", "BasketballLeagueApp.dll"]
