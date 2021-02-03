FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS base
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgdiplus libc6-dev \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*
ENV ASPNETCORE_ENVIRONMENT=Release
ENV ASPNETCORE_URLS http://*:5443;


FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS builder
ARG Configuration=Release
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgdiplus libc6-dev \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /OoS-Backend
COPY ./OutOfSchool/*.sln ./
COPY ./OutOfSchool/OutOfSchool.Tests/*.csproj ./OutOfSchool.Tests/
COPY ./OutOfSchool/OutOfSchool.WebApi/*.csproj ./OutOfSchool.WebApi/
COPY ./OutOfSchool/IdentityServer/*.csproj ./IdentityServer/

RUN dotnet restore

COPY ./OutOfSchool/ ./

WORKDIR /OoS-Backend
RUN dotnet build -c $Configuration -o /app

FROM builder AS publish
ARG Configuration=Release
RUN dotnet publish IdentityServer -c $Configuration -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
EXPOSE 5443
ENTRYPOINT ["dotnet", "OutOfSchool.IdentityServer.dll"]