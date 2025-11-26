FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS build
WORKDIR /src

COPY ['CodiblyBackend/CodiblyBackend.csproj', 'CodiblyBackend/']
RUN dotnet restore 'CodiblyBackend/CodiblyBackend.csproj'

COPY . .
WORKDIR "/src/CodiblyBackend"
RUN dotnet publish "CodiblyBackend.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 8080

ENTRYPOINT ["dotnet", "CodiblyBackend.dll"]