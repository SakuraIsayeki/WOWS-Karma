#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["WowsKarma.Web/WowsKarma.Web.csproj", "WowsKarma.Web/"]
RUN dotnet restore "WowsKarma.Web/WowsKarma.Web.csproj"
COPY . .
WORKDIR "/src/WowsKarma.Web"
RUN dotnet build "WowsKarma.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WowsKarma.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
ENTRYPOINT ["dotnet", "WowsKarma.Web.dll"]