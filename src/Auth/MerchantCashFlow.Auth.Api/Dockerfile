FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json nuget.config Directory.Build.props Directory.Packages.props ./
COPY src/ src/

RUN dotnet publish src/Auth/MerchantCashFlow.Auth.Api/MerchantCashFlow.Auth.Api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .

# Volume para a chave do data protection
RUN mkdir -p /app/keys && chown $APP_UID:$APP_UID /app/keys

USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "MerchantCashFlow.Auth.Api.dll"]
