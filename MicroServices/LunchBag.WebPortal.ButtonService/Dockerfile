FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 55023
EXPOSE 44305

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY MicroServices/LunchBag.WebPortal.ButtonService/LunchBag.WebPortal.ButtonService.csproj MicroServices/LunchBag.WebPortal.ButtonService/
RUN dotnet restore MicroServices/LunchBag.WebPortal.ButtonService/LunchBag.WebPortal.ButtonService.csproj
COPY . .
WORKDIR /src/MicroServices/LunchBag.WebPortal.ButtonService
RUN dotnet build LunchBag.WebPortal.ButtonService.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish LunchBag.WebPortal.ButtonService.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "LunchBag.WebPortal.ButtonService.dll"]
