FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/BookLibrary.Api/BookLibrary.Api.csproj", "src/BookLibrary.Api/"]
RUN dotnet restore "src/BookLibrary.Api/BookLibrary.Api.csproj"
COPY . .
WORKDIR "/src/src/BookLibrary.Api"
RUN dotnet publish "BookLibrary.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BookLibrary.Api.dll"]
