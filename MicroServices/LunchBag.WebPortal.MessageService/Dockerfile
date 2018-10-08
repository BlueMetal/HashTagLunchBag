FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 56676
EXPOSE 44310

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY MicroServices/LunchBag.WebPortal.MessageService/LunchBag.WebPortal.MessageService.csproj MicroServices/LunchBag.WebPortal.MessageService/
RUN dotnet restore MicroServices/LunchBag.WebPortal.MessageService/LunchBag.WebPortal.MessageService.csproj
COPY . .
WORKDIR /src/MicroServices/LunchBag.WebPortal.MessageService
RUN dotnet build LunchBag.WebPortal.MessageService.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish LunchBag.WebPortal.MessageService.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "LunchBag.WebPortal.MessageService.dll"]
