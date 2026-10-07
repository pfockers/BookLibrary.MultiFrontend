FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["BookLibrary.Blazor.csproj", "./"]
RUN dotnet restore "BookLibrary.Blazor.csproj"
COPY . .
RUN dotnet publish "BookLibrary.Blazor.csproj" -c Release -o /app/publish

FROM nginx:alpine AS final
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
EXPOSE 80
