FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 9001

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/FileService.Core/FileService.Core.csproj", "FileService.Core/"]
COPY ["src/FileService.Contracts/FileService.Contracts.csproj", "FileService.Contracts/"]
COPY ["src/FileService.Domain/FileService.Domain.csproj", "FileService.Domain/"]
COPY ["src/FileService.Infrastructure/FileService.Infrastructure.csproj", "FileService.Infrastructure/"]
COPY ["src/FileService.Web/FileService.Web.csproj", "FileService.Web/"]

COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]

RUN dotnet restore "FileService.Presentation/FileService.Presentation.csproj"
COPY . .
WORKDIR "/src/src/FileService.Presentation"
RUN dotnet build "./FileService.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./FileService.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileService.Presentation.dll"]