FROM bitnami/dotnet:5 AS base

RUN apt-get update \
    && apt-get install -y --no-install-recommends libfontconfig1 \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

#COPY ./res /app/res #add res later .ttf etc.

FROM bitnami/dotnet-sdk:5 AS build
WORKDIR /src
COPY . .
RUN dotnet restore && dotnet publish "MeiyounaiseSlash.csproj" -c Release -o ./publish

FROM base AS release
WORKDIR /app
COPY --from=build /src/publish .

ENTRYPOINT ["dotnet", "MeiyounaiseSlash.dll"]
