FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
RUN addgroup --gid 2000 appgrp \
    && adduser --uid 1000 --gid 2000 appusr
WORKDIR /app
RUN chown appusr:appgrp /app
USER appusr:appgrp

# .Net 8 uses now 8080 instead of 80
EXPOSE 8080
ENV COMPlus_EnableDiagnostics=0

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release

COPY . . 
RUN dotnet restore AdminServices.sln
RUN dotnet build AdminServices.sln -c ${BUILD_CONFIGURATION} -o /app

FROM build AS publish
RUN dotnet publish "./src/AdminServices.WebApi/AdminServices.WebApi.csproj" -c ${BUILD_CONFIGURATION} -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "AdminServices.WebApi.dll"]
