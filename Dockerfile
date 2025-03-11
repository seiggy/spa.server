#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
# Create a non-root user to host with. Security best practice
USER app
WORKDIR /app
EXPOSE 5184
ENV ASPNETCORE_URLS=http://+:5184

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/spa.server.csproj", "spa.server/"]
COPY ["spa.server.ServiceDefaults/spa.server.ServiceDefaults.csproj", "spa.server.ServiceDefaults/"]
RUN dotnet restore "./spa.server/spa.server.csproj"
COPY spa.server.ServiceDefaults/ spa.server.ServiceDefaults/
COPY src/ spa.server/
WORKDIR "/src/spa.server"
RUN dotnet build "./spa.server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./spa.server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "spa.server.dll"]