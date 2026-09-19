FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS build
WORKDIR /src
COPY KombfuscaWebManager/KombfuscaWebManager.csproj KombfuscaWebManager/
RUN dotnet restore KombfuscaWebManager/KombfuscaWebManager.csproj --runtime linux-x64
COPY . .
RUN dotnet publish KombfuscaWebManager/KombfuscaWebManager.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    --runtime linux-x64 \
    --self-contained true

FROM mcr.microsoft.com/playwright/dotnet:v1.62.0-noble AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_EnableDiagnostics=0
COPY --from=build /app/publish .
EXPOSE 8080
RUN mkdir -p /home/pwuser/.aspnet/DataProtection-Keys \
    && chown -R pwuser:pwuser /home/pwuser/.aspnet
USER pwuser
ENTRYPOINT ["./KombfuscaWebManager"]
