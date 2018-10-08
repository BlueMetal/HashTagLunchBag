FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 51732
EXPOSE 44363

FROM microsoft/dotnet:2.1-sdk AS build

#Add NodeJS
RUN apt-get update && \
    apt-get install -y wget && \
    apt-get install -y gnupg2 && \
    wget -qO- https://deb.nodesource.com/setup_8.x | bash - && \
    apt-get install -y build-essential nodejs && \
	  rm -rf /var/lib/apt/lists/*

WORKDIR /src
COPY LunchBag.RegistrationPortal/LunchBag.RegistrationPortal.csproj LunchBag.RegistrationPortal/
RUN dotnet restore LunchBag.RegistrationPortal/LunchBag.RegistrationPortal.csproj
COPY . .
WORKDIR /src/LunchBag.RegistrationPortal
RUN dotnet build LunchBag.RegistrationPortal.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish LunchBag.RegistrationPortal.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "LunchBag.RegistrationPortal.dll"]