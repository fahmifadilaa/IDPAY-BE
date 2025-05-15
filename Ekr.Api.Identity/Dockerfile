#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Ekr.Api.Identity/Ekr.Api.Identity.csproj", "Ekr.Api.Identity/"]
COPY ["Ekr.Auth.Contracts/Ekr.Auth.Contracts.csproj", "Ekr.Auth.Contracts/"]
COPY ["Ekr.Core/Ekr.Core.csproj", "Ekr.Core/"]
COPY ["Ekr.Dependency/Ekr.Dependency.csproj", "Ekr.Dependency/"]
COPY ["Ekr.Services/Ekr.Services.csproj", "Ekr.Services/"]
COPY ["Ekr.Dapper.Connection/Ekr.Dapper.Connection.csproj", "Ekr.Dapper.Connection/"]
COPY ["Ekr.Dapper.Connection.Contracts/Ekr.Dapper.Connection.Contracts.csproj", "Ekr.Dapper.Connection.Contracts/"]
COPY ["Ekr.Business.Contracts/Ekr.Business.Contracts.csproj", "Ekr.Business.Contracts/"]
COPY ["Ekr.Business/Ekr.Business.csproj", "Ekr.Business/"]
COPY ["Ekr.Repository.Contracts/Ekr.Repository.Contracts.csproj", "Ekr.Repository.Contracts/"]
COPY ["Ekr.Auth/Ekr.Auth.csproj", "Ekr.Auth/"]
COPY ["Ekr.Gateway/Ekr.Gateway.csproj", "Ekr.Gateway/"]
COPY ["Ekr.Repository/Ekr.Repository.csproj", "Ekr.Repository/"]
RUN dotnet restore "Ekr.Api.Identity/Ekr.Api.Identity.csproj"
COPY . .
WORKDIR "/src/Ekr.Api.Identity"
RUN dotnet build "Ekr.Api.Identity.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Ekr.Api.Identity.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ekr.Api.Identity.dll"]