FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS base
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build
WORKDIR /src

COPY ["Codebelt.Website.csproj", "Codebelt.Website/"]
RUN dotnet restore "Codebelt.Website/Codebelt.Website.csproj"

COPY [".", "Codebelt.Website/"]

WORKDIR "/src/Codebelt.Website"
RUN dotnet build "Codebelt.Website.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Codebelt.Website.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Codebelt.Website.dll"]