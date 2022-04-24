FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ChessBot/ChessBot.csproj", "ChessBot/"]
RUN dotnet restore "ChessBot/ChessBot.csproj"
COPY . .
WORKDIR "/src/ChessBot"
RUN dotnet build "ChessBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChessBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChessBot.dll"]
