# Stage 1: Build Frontend (Angular)
FROM node:20 AS frontend-build
WORKDIR /frontend
COPY ["Workflow.IO.Web/package.json", "Workflow.IO.Web/package-lock.json*", "./"]
RUN npm ci
COPY ["Workflow.IO.Web/", "."]
RUN npm run build -- --configuration production

# Stage 2: Base runtime for ASP.NET
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Stage 3: Build backend
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["GatewayApi/GatewayApi.csproj", "GatewayApi/"]
COPY ["Workflow.IO.Shared/Workflow.IO.Shared.csproj", "Workflow.IO.Shared/"]
RUN dotnet restore "GatewayApi/GatewayApi.csproj"
COPY . .
RUN dotnet publish "GatewayApi/GatewayApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Final output
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
# Copy frontend build output into wwwroot
COPY --from=frontend-build /frontend/dist/zen-track.web/browser ./wwwroot
ENTRYPOINT ["dotnet", "GatewayApi.dll"]