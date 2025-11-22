FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TranscriptGenerator.Server/TranscriptGenerator.Server.csproj", "TranscriptGenerator.Server/"]
RUN dotnet restore "TranscriptGenerator.Server/TranscriptGenerator.Server.csproj"
COPY . .
WORKDIR "/src/TranscriptGenerator.Server"
RUN dotnet build "TranscriptGenerator.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TranscriptGenerator.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TranscriptGenerator.Server.dll"]
