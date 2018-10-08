FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 64200
EXPOSE 44354

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY MicroServices/LunchBag.WebPortal.BusService/LunchBag.WebPortal.BusService.csproj MicroServices/LunchBag.WebPortal.BusService/
RUN dotnet restore MicroServices/LunchBag.WebPortal.BusService/LunchBag.WebPortal.BusService.csproj
COPY . .
WORKDIR /src/MicroServices/LunchBag.WebPortal.BusService
RUN dotnet build LunchBag.WebPortal.BusService.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish LunchBag.WebPortal.BusService.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "LunchBag.WebPortal.BusService.dll"]
