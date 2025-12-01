FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 9001

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/DirectoryService.Application/DirectoryService.Application.csproj", "DirectoryService.Application/"]
COPY ["src/DirectoryService.Contracts/DirectoryService.Contracts.csproj", "DirectoryService.Contracts/"]
COPY ["src/DirectoryService.Domain/DirectoryService.Domain.csproj", "DirectoryService.Domain/"]
COPY ["src/DirectoryService.Infrastructure/DirectoryService.Infrastructure.csproj", "DirectoryService.Infrastructure/"]
COPY ["src/DirectoryService.Presentation/DirectoryService.Presentation.csproj", "DirectoryService.Presentation/"]
COPY ["src/SharedKernel/SharedKernel.csproj", "SharedKernel/"]

COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]

RUN dotnet restore "DirectoryService.Presentation/DirectoryService.Presentation.csproj"
COPY . .
WORKDIR "/src/src/DirectoryService.Presentation"
RUN dotnet build "./DirectoryService.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DirectoryService.Presentation.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DirectoryService.Presentation.dll"]