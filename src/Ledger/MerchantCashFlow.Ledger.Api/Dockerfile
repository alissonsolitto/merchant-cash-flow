FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json nuget.config Directory.Build.props Directory.Packages.props ./
COPY src/ src/

RUN dotnet publish src/Ledger/MerchantCashFlow.Ledger.Api/MerchantCashFlow.Ledger.Api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .

USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "MerchantCashFlow.Ledger.Api.dll"]
