FROM bitnami/dotnet-sdk:5 AS base

RUN apt-get update && apt-get install -y libfontconfig1

#COPY ./res /app/res #add res later .ttf etc.

FROM base AS build
WORKDIR /src
COPY . .
RUN dotnet restore && dotnet publish "MeiyounaiseSlash.csproj" -c Release -o ./publish

FROM base AS release
WORKDIR /app
COPY --from=build /src/publish .

ENTRYPOINT ["dotnet", "MeiyounaiseSlash.dll"]
