FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY JiuJitsu.Api/JiuJitsu.Api.csproj               JiuJitsu.Api/
COPY JiuJitsu.Application/JiuJitsu.Application.csproj JiuJitsu.Application/
COPY JiuJitsu.Infrastructure/JiuJitsu.Infrastructure.csproj JiuJitsu.Infrastructure/
COPY JiuJitsu.ServiceDefaults/JiuJitsu.ServiceDefaults.csproj JiuJitsu.ServiceDefaults/
COPY JiuJitsu.Domain/JiuJitsu.Domain.csproj         JiuJitsu.Domain/
COPY JiuJitsu.Contracts/JiuJitsu.Contracts.csproj   JiuJitsu.Contracts/

RUN dotnet restore JiuJitsu.Api/JiuJitsu.Api.csproj

COPY JiuJitsu.Api/           JiuJitsu.Api/
COPY JiuJitsu.Application/   JiuJitsu.Application/
COPY JiuJitsu.Infrastructure/ JiuJitsu.Infrastructure/
COPY JiuJitsu.ServiceDefaults/ JiuJitsu.ServiceDefaults/
COPY JiuJitsu.Domain/        JiuJitsu.Domain/
COPY JiuJitsu.Contracts/     JiuJitsu.Contracts/

RUN dotnet publish JiuJitsu.Api/JiuJitsu.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "JiuJitsu.Api.dll"]
