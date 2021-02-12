FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgdiplus libc6-dev \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*
ENV ASPNETCORE_ENVIRONMENT=Release
ENV ASPNETCORE_URLS http://*:5000;


FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS builder
ARG Configuration=Release
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgdiplus libc6-dev \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*


WORKDIR /OoS-Backend
COPY ./OutOfSchool/OutOfSchool.WebApi/*.csproj ./OutOfSchool.WebApi/
COPY ./OutOfSchool/OutOfSchool.DataAccess/*.csproj ./OutOfSchool.DataAccess/

RUN dotnet restore ./OutOfSchool.WebApi/OutOfSchool.WebApi.csproj

COPY ./OutOfSchool/OutOfSchool.WebApi/ ./OutOfSchool.WebApi/
COPY ./OutOfSchool/OutOfSchool.DataAccess/ ./OutOfSchool.DataAccess/

RUN dotnet build ./OutOfSchool.WebApi/OutOfSchool.WebApi.csproj -c $Configuration -o /app

FROM builder AS publish
ARG Configuration=Release
RUN dotnet publish ./OutOfSchool.WebApi/OutOfSchool.WebApi.csproj -c $Configuration -o /app
FROM base AS final
COPY --from=publish /app .
EXPOSE 5000 5001
ENTRYPOINT ["dotnet", "OutOfSchool.WebApi.dll"]
