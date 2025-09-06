# Étape 1 : build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copier les fichiers .csproj et restaurer les dépendances
COPY *.csproj ./
RUN dotnet restore

# Copier tout le projet et publier en Release
COPY . ./
RUN dotnet publish -c Release -o out

# Étape 2 : runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Port exposé par l'application
EXPOSE 5000

# Commande de démarrage
ENTRYPOINT ["dotnet", "DjanabaApi1.dll"]