FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src
COPY ChatService.Api/ChatService.Api.csproj ChatService.Api/
RUN dotnet restore ChatService.Api/ChatService.Api.csproj
COPY . .
RUN dotnet publish ChatService.Api/ChatService.Api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "ChatService.Api.dll"]