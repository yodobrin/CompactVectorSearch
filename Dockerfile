# Use the official ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

ENV ASPNETCORE_URLS=http://+:80


# Use the SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the CSPROJ file and restore any NuGet packages
COPY ["CompactVectorSearch.csproj", "./"]
RUN dotnet restore "CompactVectorSearch.csproj"

# Copy the rest of your source code
COPY . .

# Build the project
RUN dotnet build "CompactVectorSearch.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "CompactVectorSearch.csproj" -c Release -o /app/publish

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY application.env ./

ENTRYPOINT ["dotnet", "CompactVectorSearch.dll"]
