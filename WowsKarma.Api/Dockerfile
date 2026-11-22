#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:11.0 AS base
WORKDIR /app
EXPOSE 8080
USER $APP_UID

FROM mcr.microsoft.com/dotnet/sdk:11.0 AS publish
WORKDIR /src
COPY global.json ./
COPY WowsKarma.Api/ WowsKarma.Api/
COPY WowsKarma.Api.Minimap.Client/ WowsKarma.Api.Minimap.Client/
COPY WowsKarma.Common/ WowsKarma.Common/
COPY WowsKarma.ServiceDefaults/ WowsKarma.ServiceDefaults/
RUN dotnet publish WowsKarma.Api/WowsKarma.Api.csproj --configuration Release --no-self-contained --output /app/publish

FROM base AS final
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WowsKarma.Api.dll"]
