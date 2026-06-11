FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Habitat.BackEnd.Progress.Application/Habitat.BackEnd.Progress.Application.csproj", "src/Habitat.BackEnd.Progress.Application/"]
COPY ["src/Habitat.BackEnd.Progress.Infrastructure/Habitat.BackEnd.Progress.Infrastructure.csproj", "src/Habitat.BackEnd.Progress.Infrastructure/"]
COPY ["src/Habitat.BackEnd.Progress.WebApi/Habitat.BackEnd.Progress.WebApi.csproj", "src/Habitat.BackEnd.Progress.WebApi/"]
RUN dotnet restore "src/Habitat.BackEnd.Progress.WebApi/Habitat.BackEnd.Progress.WebApi.csproj"
COPY . .
RUN dotnet publish "src/Habitat.BackEnd.Progress.WebApi/Habitat.BackEnd.Progress.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Habitat.BackEnd.Progress.WebApi.dll"]
