FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY MicroServices/LunchBag.RegistrationPortal.Api/LunchBag.RegistrationPortal.Api.csproj MicroServices/LunchBag.RegistrationPortal.Api/
RUN dotnet restore MicroServices/LunchBag.RegistrationPortal.Api/LunchBag.RegistrationPortal.Api.csproj
COPY . .
WORKDIR /src/MicroServices/LunchBag.RegistrationPortal.Api
RUN dotnet build LunchBag.RegistrationPortal.Api.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish LunchBag.RegistrationPortal.Api.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "LunchBag.RegistrationPortal.Api.dll"]
