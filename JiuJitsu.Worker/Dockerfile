FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY JiuJitsu.Worker/JiuJitsu.Worker.csproj         JiuJitsu.Worker/
COPY JiuJitsu.Application/JiuJitsu.Application.csproj JiuJitsu.Application/
COPY JiuJitsu.Infrastructure/JiuJitsu.Infrastructure.csproj JiuJitsu.Infrastructure/
COPY JiuJitsu.ServiceDefaults/JiuJitsu.ServiceDefaults.csproj JiuJitsu.ServiceDefaults/
COPY JiuJitsu.Domain/JiuJitsu.Domain.csproj         JiuJitsu.Domain/
COPY JiuJitsu.Contracts/JiuJitsu.Contracts.csproj   JiuJitsu.Contracts/

RUN dotnet restore JiuJitsu.Worker/JiuJitsu.Worker.csproj

COPY JiuJitsu.Worker/        JiuJitsu.Worker/
COPY JiuJitsu.Application/   JiuJitsu.Application/
COPY JiuJitsu.Infrastructure/ JiuJitsu.Infrastructure/
COPY JiuJitsu.ServiceDefaults/ JiuJitsu.ServiceDefaults/
COPY JiuJitsu.Domain/        JiuJitsu.Domain/
COPY JiuJitsu.Contracts/     JiuJitsu.Contracts/

RUN dotnet publish JiuJitsu.Worker/JiuJitsu.Worker.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "JiuJitsu.Worker.dll"]
