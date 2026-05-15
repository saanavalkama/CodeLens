FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["CodeLens.API/CodeLens.API.csproj", "CodeLens.API/"]
COPY ["CodeLens.Application/CodeLens.Application.csproj", "CodeLens.Application/"]
COPY ["CodeLens.Domain/CodeLens.Domain.csproj", "CodeLens.Domain/"]
COPY ["CodeLens.Infrastructure/CodeLens.Infrastructure.csproj", "CodeLens.Infrastructure/"]
RUN dotnet restore "CodeLens.API/CodeLens.API.csproj"
COPY . .
RUN dotnet build "CodeLens.API/CodeLens.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CodeLens.API/CodeLens.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CodeLens.API.dll"]