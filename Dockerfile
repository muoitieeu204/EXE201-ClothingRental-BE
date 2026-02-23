FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY EXE201.sln .
COPY EXE201/EXE201.API.csproj EXE201/
COPY EXE201.service/EXE201.Service.csproj EXE201.service/
COPY EXE201.Repository/EXE201.Repository.csproj EXE201.Repository/

RUN dotnet restore EXE201/EXE201.API.csproj

COPY . .
RUN dotnet publish EXE201/EXE201.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 10000
ENTRYPOINT ["sh","-c","dotnet EXE201.API.dll --urls http://0.0.0.0:${PORT:-10000}"]
