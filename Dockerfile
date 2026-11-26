FROM mcr.microsoft.com/dotnet/sdk:11.0 AS build
WORKDIR /src
# The project files are copied first, so that the layer with the restored packages is cached.
COPY src/FloorPlanner.Core/FloorPlanner.Core.csproj src/FloorPlanner.Core/
COPY src/FloorPlanner.Api/FloorPlanner.Api.csproj src/FloorPlanner.Api/
RUN dotnet restore src/FloorPlanner.Api/FloorPlanner.Api.csproj
COPY src/ src/
RUN dotnet publish src/FloorPlanner.Api/FloorPlanner.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:11.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
EXPOSE 8080
# The application does not run as root.
USER $APP_UID
ENTRYPOINT ["dotnet", "FloorPlanner.Api.dll"]
