# BUILD
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY HippoExchange.Api.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /out /p:UseAppHost=false

# RUNTIME (non-root)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
ENV ASPNETCORE_URLS=http://+:8080
WORKDIR /app
USER app
COPY --chown=app:app --from=build /out ./

# Health endpoint wiring (match compose healthcheck)
# Add this minimal endpoint in Program.cs: app.MapGet("/health", () => Results.Ok("ok")));
EXPOSE 8080
ENTRYPOINT ["dotnet", "HippoExchange.Api.dll"]
