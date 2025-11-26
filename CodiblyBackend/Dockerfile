FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /src

COPY ['CodiblyBackend.csproj', './']
RUN dotnet restore './CodiblyBackend.csproj'

COPY . .
RUN dotnet publish 'CodiblyBackend.csproj' -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CodiblyBackend.dll"]