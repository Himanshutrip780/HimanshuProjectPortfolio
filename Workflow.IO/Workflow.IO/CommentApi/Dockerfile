FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CommentApi/CommentApi.csproj", "CommentApi/"]
COPY ["Workflow.IO.Shared/Workflow.IO.Shared.csproj", "Workflow.IO.Shared/"]
RUN dotnet restore "CommentApi/CommentApi.csproj"
COPY . .
RUN dotnet publish "CommentApi/CommentApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CommentApi.dll"]
