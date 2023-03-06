# рабочая команда создания имейджа: docker build -t jfrog.mydomain.ru/dockerdgtl/beeline.mobid.idgw:latest --build-arg Nuget_User --build-arg Nuget_Password .

FROM jfrog.mydomain.ru/dockervirtual/beeline.mobid.aspnet:6.0 AS base
EXPOSE 80

FROM jfrog.mydomain.ru/dockervirtual/dotnet/sdk:6.0 AS build
WORKDIR /src

ARG Nuget_User
ARG Nuget_Password

COPY ["src/.nuget/", "src/.nuget/"]
COPY ["src/AppSample.Api/*.csproj", "src/AppSample.Api/"]
COPY ["src/AppSample.Db/*.csproj", "src/AppSample.Db/"]
COPY ["src/AppSample.Domain/*.csproj", "src/AppSample.Domain/"]
COPY ["src/AppSample.Infrastructure/*.csproj", "src/AppSample.Infrastructure/"]
COPY ["submodules/", "submodules/"]
RUN dotnet restore --configfile "src/.nuget/nuget.config" "src/AppSample.Api/AppSample.Api.csproj"

COPY ["src/", "src/"]
ARG FRONT_VERSION="*"
RUN dotnet add "src/AppSample.Api/AppSample.Api.csproj" package "AppSample.front-dev" -v "$FRONT_VERSION" -n
RUN dotnet publish "src/AppSample.Api/AppSample.Api.csproj" --configfile "src/.nuget/nuget.config" -c Release -o /app/publish 

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AppSample.Api.dll"]
