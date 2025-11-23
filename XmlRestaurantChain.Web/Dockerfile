# Build and run ASP.NET Core MVC app with PostgreSQL connection string injected via env.
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["XmlRestaurantChain.Web/XmlRestaurantChain.Web.csproj", "XmlRestaurantChain.Web/"]
RUN dotnet restore "XmlRestaurantChain.Web/XmlRestaurantChain.Web.csproj"

COPY . .
WORKDIR "/src/XmlRestaurantChain.Web"
RUN dotnet publish "XmlRestaurantChain.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "XmlRestaurantChain.Web.dll"]
