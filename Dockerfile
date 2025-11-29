FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /src

COPY ["CodiblyBackend/CodiblyBackend.csproj", "CodiblyBackend/"]
RUN dotnet restore "CodiblyBackend/CodiblyBackend.csproj"

COPY . .
RUN dotnet publish "CodiblyBackend/CodiblyBackend.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "CodiblyBackend.dll"]