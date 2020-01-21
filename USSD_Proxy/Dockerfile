FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["USSD_Proxy/USSD_Proxy.csproj", "USSD_Proxy/"]
RUN dotnet restore "USSD_Proxy/USSD_Proxy.csproj"
COPY . .
WORKDIR "/src/USSD_Proxy"
RUN dotnet build "USSD_Proxy.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "USSD_Proxy.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "USSD_Proxy.dll"]