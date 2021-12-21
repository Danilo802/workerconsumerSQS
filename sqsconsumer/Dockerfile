#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["sqsconsumer/sqsconsumer.csproj", "sqsconsumer/"]
RUN dotnet restore "sqsconsumer/sqsconsumer.csproj"
COPY . .
WORKDIR "/src/sqsconsumer"
RUN dotnet build "sqsconsumer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "sqsconsumer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "sqsconsumer.dll"]