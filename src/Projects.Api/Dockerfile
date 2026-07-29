FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Projects.Api/Projects.Api.csproj", "Projects.Api/"]
RUN dotnet restore "Projects.Api/Projects.Api.csproj"

COPY ["src/Projects.Api/", "Projects.Api/"]
WORKDIR "/src/Projects.Api"
RUN dotnet publish "Projects.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "Projects.Api.dll"]