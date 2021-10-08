﻿FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["MeiyounaiseSlash.csproj", "./"]
RUN dotnet restore "MeiyounaiseSlash.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "MeiyounaiseSlash.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MeiyounaiseSlash.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MeiyounaiseSlash.dll"]
