FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["NotificationApi/NotificationApi.csproj", "NotificationApi/"]
COPY ["Workflow.IO.Shared/Workflow.IO.Shared.csproj", "Workflow.IO.Shared/"]
RUN dotnet restore "NotificationApi/NotificationApi.csproj"
COPY . .
RUN dotnet publish "NotificationApi/NotificationApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NotificationApi.dll"]
