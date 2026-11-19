# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore Host/PropertyPlatform.Host.csproj
# Git SHA from CI; .NET appends it to the informational version (shown by /version).
ARG SOURCE_REVISION=""
RUN dotnet publish Host/PropertyPlatform.Host.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false /p:SourceRevisionId=${SOURCE_REVISION}

# ---- runtime (chiseled: no shell, non-root by default) ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "PropertyPlatform.Host.dll"]
