# рабочая команда создания имейджа: docker build -t jfrog.mydomain.ru/dockerdgtl/beeline.mobid.idgw_adm:latest --build-arg Nuget_User --build-arg Nuget_Password .

FROM jfrog.mydomain.ru/dockervirtual/beeline.mobid.aspnet:6.0 AS base
EXPOSE 3000

FROM jfrog.mydomain.ru/dockervirtual/dotnet/sdk:6.0 AS build
WORKDIR /src

ARG Nuget_User
ARG Nuget_Password

COPY ["src/.nuget/", "src/.nuget/"]
COPY ["src/AppSample.Admin/*.csproj", "src/AppSample.Admin/"]
COPY ["src/AppSample.Domain/*.csproj", "src/AppSample.Domain/"]
COPY ["src/AppSample.Infrastructure/*.csproj", "src/AppSample.Infrastructure/"]
COPY ["submodules/", "submodules/"]
RUN dotnet restore --configfile "src/.nuget/nuget.config" "src/AppSample.Admin/AppSample.Admin.csproj"

COPY ["src/", "src/"]
RUN dotnet publish "src/AppSample.Admin/AppSample.Admin.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AppSample.Admin.dll"]
