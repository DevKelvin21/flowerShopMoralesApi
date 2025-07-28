FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src


COPY *.sln .
COPY flowerShopMoralesApi/*.csproj ./flowerShopMoralesApi/
RUN dotnet restore


COPY . .
WORKDIR /src/flowerShopMoralesApi
RUN dotnet publish -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .


EXPOSE 8080
ENTRYPOINT ["dotnet", "flowerShopMoralesApi.dll"]