# Stage 1: Build Angular frontend
FROM node:20-alpine AS frontend-build
WORKDIR /app
COPY Workflow.IO.Web/package*.json ./
RUN npm ci
COPY Workflow.IO.Web/ .
RUN npm run build -- --configuration=production

# Stage 2: Build C# backend
FROM mcr.microsoft.com/dotnet/sdk:8.0 as builder

WORKDIR /src

# Copy solution files
COPY ["Workflow.IO.sln", "Workflow.IO.slnx", "Directory.Build.props", "./"]

# Copy all project files
COPY ["GatewayApi/", "GatewayApi/"]
COPY ["UserApi/", "UserApi/"]
COPY ["ProjectApi/", "ProjectApi/"]
COPY ["TaskApi/", "TaskApi/"]
COPY ["CommentApi/", "CommentApi/"]
COPY ["NotificationApi/", "NotificationApi/"]
COPY ["ActivityApi/", "ActivityApi/"]
COPY ["FileApi/", "FileApi/"]
COPY ["AnalyticsApi/", "AnalyticsApi/"]
COPY ["RealtimeApi/", "RealtimeApi/"]
COPY ["JwtAuthenticationManager/", "JwtAuthenticationManager/"]
COPY ["Workflow.IO.Shared/", "Workflow.IO.Shared/"]

# Restore
RUN dotnet restore "Workflow.IO.sln"

# Build
RUN dotnet build "Workflow.IO.sln" -c Release -o /app/build

# Publish Gateway API
RUN dotnet publish "GatewayApi/GatewayApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

# Copy published app
COPY --from=builder /app/publish .

# Copy Angular built static files to GatewayApi's wwwroot
COPY --from=frontend-build /app/dist/zen-track.web/browser ./wwwroot

# Set environment
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:${PORT:-8080}/health || exit 1

# Expose port
EXPOSE 8080

# Run
ENTRYPOINT ["dotnet", "GatewayApi.dll"]
