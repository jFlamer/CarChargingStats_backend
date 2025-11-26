FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /src

COPY ['CodiblyBackend/CodiblyBackend.csproj', 'CodiblyBackend/']
RUN dotnet restore 'CodiblyBackend/CodiblyBackend.csproj'

COPY . .
WORKDIR "/src/CodiblyBackend"
RUN dotnet publish "CodiblyBackend.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 80

ENTRYPOINT ["dotnet", "CodiblyBackend.dll"]