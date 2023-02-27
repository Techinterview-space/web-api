# https://stackoverflow.com/a/64844268
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# https://github.com/rdvojmoc/DinkToPdf/issues/53#issuecomment-535026014
# https://github.com/rdvojmoc/DinkToPdf/issues/138#issuecomment-914972777
RUN apt-get update
RUN apt-get install wget libgdiplus -y
RUN wget -P /app https://github.com/rdvojmoc/DinkToPdf/raw/master/v0.12.4/64%20bit/libwkhtmltox.dll
RUN wget -P /app https://github.com/rdvojmoc/DinkToPdf/raw/master/v0.12.4/64%20bit/libwkhtmltox.dylib
RUN wget -P /app https://github.com/rdvojmoc/DinkToPdf/raw/master/v0.12.4/64%20bit/libwkhtmltox.so

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /src
COPY ["Web.Api/Web.Api.csproj", "Web.Api/"]

RUN dotnet restore "Web.Api/Web.Api.csproj"
COPY . .
WORKDIR "/src/Web.Api"
RUN dotnet build "Web.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Web.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .
COPY aspnetapp.pfx .
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/app/aspnetapp.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=petrel
ENV ASPNETCORE_URLS http://0.0.0.0:5000;https://0.0.0.0:5001
ENV ASPNETCORE_HTTPS_PORT=443
EXPOSE 5000
EXPOSE 5001

ENTRYPOINT ["dotnet", "Web.Api.dll"]
