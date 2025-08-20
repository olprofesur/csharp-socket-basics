# Usa l'immagine ufficiale .NET SDK per compilare l'app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copia i file del progetto e ripristina i pacchetti
COPY *.csproj .
RUN dotnet restore

# Copia tutto il resto e compila
COPY . .
RUN dotnet publish -c Release -o out

# Usa l'immagine runtime per eseguire l'app
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "socketserverjsondocker.dll"]