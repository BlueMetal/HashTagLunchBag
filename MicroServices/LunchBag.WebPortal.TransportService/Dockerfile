FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 52470
EXPOSE 44321

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY MicroServices/LunchBag.WebPortal.TransportService/LunchBag.WebPortal.TransportService.csproj MicroServices/LunchBag.WebPortal.TransportService/
RUN dotnet restore MicroServices/LunchBag.WebPortal.TransportService/LunchBag.WebPortal.TransportService.csproj
COPY . .
WORKDIR /src/MicroServices/LunchBag.WebPortal.TransportService
RUN dotnet build LunchBag.WebPortal.TransportService.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish LunchBag.WebPortal.TransportService.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "LunchBag.WebPortal.TransportService.dll"]
