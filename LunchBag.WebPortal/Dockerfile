FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 50902
EXPOSE 44333

FROM microsoft/dotnet:2.1-sdk AS build

#Add NodeJS
RUN apt-get update && \
    apt-get install -y wget && \
    apt-get install -y gnupg2 && \
    wget -qO- https://deb.nodesource.com/setup_8.x | bash - && \
    apt-get install -y build-essential nodejs && \
	  rm -rf /var/lib/apt/lists/*

WORKDIR /src
COPY LunchBag.WebPortal/LunchBag.WebPortal.csproj LunchBag.WebPortal/
RUN dotnet restore LunchBag.WebPortal/LunchBag.WebPortal.csproj
COPY . .
WORKDIR /src/LunchBag.WebPortal
RUN dotnet build LunchBag.WebPortal.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish LunchBag.WebPortal.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "LunchBag.WebPortal.dll"]