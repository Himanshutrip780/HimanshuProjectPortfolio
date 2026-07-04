FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["ATS.API/ATS.API.csproj", "ATS.API/"]
COPY ["ATS.Application/ATS.Application.csproj", "ATS.Application/"]
COPY ["ATS.Domain/ATS.Domain.csproj", "ATS.Domain/"]
COPY ["ATS.Infrastructure/ATS.Infrastructure.csproj", "ATS.Infrastructure/"]
COPY ["ATS.Persistence/ATS.Persistence.csproj", "ATS.Persistence/"]
COPY ["ATS.Shared/ATS.Shared.csproj", "ATS.Shared/"]
RUN dotnet restore "ATS.API/ATS.API.csproj"
COPY . .
WORKDIR "/src/ATS.API"
RUN dotnet build "ATS.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ATS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ATS.API.dll"]
