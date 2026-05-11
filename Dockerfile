FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Copie des fichiers projets (.csproj)
# Rappel : le contexte est "..", donc on doit spécifier les sous-dossiers
COPY ModuleHelpdesk/ModuleHelpdesk.csproj ModuleHelpdesk/
COPY Contrat-d-evenement/ITANIS.SharedEvents.csproj Contrat-d-evenement/

# 2. Restauration des dépendances
# On restaure le projet principal qui va automatiquement chercher ses références
RUN dotnet restore ModuleHelpdesk/ModuleHelpdesk.csproj

# 3. Copie du code source complet
COPY ModuleHelpdesk/ ModuleHelpdesk/
COPY Contrat-d-evenement/ Contrat-d-evenement/

# 4. Publication
WORKDIR /src/ModuleHelpdesk
RUN dotnet publish -c Release -o /app/out

# --- Étape finale ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Configuration réseau
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080 

ENTRYPOINT ["dotnet", "ModuleHelpdesk.dll"]