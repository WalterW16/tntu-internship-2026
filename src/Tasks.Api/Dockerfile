FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Tasks.Api/Tasks.Api.csproj", "Tasks.Api/"]
RUN dotnet restore "Tasks.Api/Tasks.Api.csproj"

COPY ["src/Tasks.Api/", "Tasks.Api/"]
WORKDIR "/src/Tasks.Api"
RUN dotnet publish "Tasks.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "Tasks.Api.dll"]