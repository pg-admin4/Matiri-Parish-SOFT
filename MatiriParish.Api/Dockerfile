FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# copy csproj and restore
COPY *.sln .
COPY MatiriParish.Api/*.csproj MatiriParish.Api/
RUN dotnet restore

COPY . .

RUN dotnet publish MatiriParish.Api -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "MatiriParish.Api.dll"]
