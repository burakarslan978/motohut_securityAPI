FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://*:8080

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Motohut API.csproj", "./"]
RUN dotnet restore "Motohut API.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Motohut API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Motohut API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Motohut API.dll"]
