FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props MyPost.sln ./
COPY src/MyPost.Domain/MyPost.Domain.csproj src/MyPost.Domain/
COPY src/MyPost.Application/MyPost.Application.csproj src/MyPost.Application/
COPY src/MyPost.Infrastructure/MyPost.Infrastructure.csproj src/MyPost.Infrastructure/
COPY apps/MyPost.Api/MyPost.Api.csproj apps/MyPost.Api/
RUN dotnet restore apps/MyPost.Api/MyPost.Api.csproj
COPY src src
COPY apps/MyPost.Api apps/MyPost.Api
RUN dotnet publish apps/MyPost.Api/MyPost.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
RUN adduser --disabled-password --gecos "" --uid 10001 mypost
COPY --from=build --chown=mypost:mypost /app .
USER mypost
EXPOSE 8080
ENTRYPOINT ["dotnet", "MyPost.Api.dll"]
