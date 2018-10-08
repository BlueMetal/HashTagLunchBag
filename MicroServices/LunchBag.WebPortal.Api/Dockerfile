FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 55022
EXPOSE 44353

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY MicroServices/LunchBag.WebPortal.Api/LunchBag.WebPortal.Api.csproj MicroServices/LunchBag.WebPortal.Api/
RUN dotnet restore MicroServices/LunchBag.WebPortal.Api/LunchBag.WebPortal.Api.csproj
COPY . .
WORKDIR /src/MicroServices/LunchBag.WebPortal.Api
RUN dotnet build LunchBag.WebPortal.Api.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish LunchBag.WebPortal.Api.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "LunchBag.WebPortal.Api.dll"]
