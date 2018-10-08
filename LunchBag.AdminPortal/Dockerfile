FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 59433

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY LunchBag.AdminPortal/LunchBag.AdminPortal.csproj LunchBag.AdminPortal/
RUN dotnet restore LunchBag.AdminPortal/LunchBag.AdminPortal.csproj
COPY . .
WORKDIR /src/LunchBag.AdminPortal
RUN dotnet build LunchBag.AdminPortal.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish LunchBag.AdminPortal.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "LunchBag.AdminPortal.dll"]
