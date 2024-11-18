FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /api
COPY *.sln .
COPY SimpleBankApi.Api ./SimpleBankApi.Api
COPY SimpleBankApi.Application ./SimpleBankApi.Application
COPY SimpleBankApi.Domain ./SimpleBankApi.Domain
COPY SimpleBankApi.Factory ./SimpleBankApi.Factory
COPY SimpleBankApi.Infra ./SimpleBankApi.Infra
COPY SimpleBankApi.Repository ./SimpleBankApi.Repository
RUN dotnet sln remove "SimpleBankApi.Tests\SimpleBankApi.Tests.csproj"
RUN dotnet restore
RUN dotnet publish -c Release -o build

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /api
COPY --from=build /api/build .
EXPOSE 8080
ENTRYPOINT ["dotnet", "SimpleBankApi.Api.dll"]