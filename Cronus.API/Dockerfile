#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Cronus.API/Cronus.API.csproj", "Cronus.API/"]
RUN dotnet restore "Cronus.API/Cronus.API.csproj"
COPY . .
WORKDIR "/src/Cronus.API"
RUN dotnet build "Cronus.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Cronus.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cronus.API.dll"]