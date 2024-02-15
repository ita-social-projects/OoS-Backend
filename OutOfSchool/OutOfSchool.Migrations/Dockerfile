ARG DOTNET_VERSION=6.0

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION}-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS builder
# Allow builds for multiple context (app, openiddict, etc.)
ARG DB_CONTEXT=OutOfSchoolDbContext

WORKDIR /src

#### Install EF Core CLI ###
COPY .config/dotnet-tools.json .config/
RUN dotnet tool restore

### Dependencies to restore ###
COPY *.props .
COPY *.sln .
COPY ["OutOfSchool.Migrations/OutOfSchool.Migrations.csproj", "OutOfSchool.Migrations/"]
COPY ["OutOfSchool.Common/OutOfSchool.Common.csproj", "OutOfSchool.Common/"]
COPY ["OutOfSchool.DataAccess/OutOfSchool.DataAccess.csproj", "OutOfSchool.DataAccess/"]
COPY ["OutOfSchool.AuthCommon/OutOfSchool.AuthCommon.csproj", "OutOfSchool.AuthCommon/"]
COPY ["OutOfSchool.EmailSender/OutOfSchool.EmailSender.csproj", "OutOfSchool.EmailSender/"]
COPY ["OutOfSchool.RazorTemplatesData/OutOfSchool.RazorTemplatesData.csproj", "OutOfSchool.RazorTemplatesData/"]
COPY ["OutOfSchool.GRPC/OutOfSchool.GRPC.csproj", "OutOfSchool.GRPC/"]
RUN dotnet restore "OutOfSchool.Migrations/OutOfSchool.Migrations.csproj" --use-current-runtime

#### Create EF Migration Bundle ###
COPY . .
WORKDIR "/src/OutOfSchool.Migrations"
# Need runtime flag as we're building on debain/ubuntu, but running on alpine
RUN dotnet ef migrations bundle -v \
    --configuration Bundle \
    --target-runtime linux-musl-x64 \
    --output ./efbundle \
    --context ${DB_CONTEXT}

FROM base AS final
ENV DB_HOST="localhost"
ENV DB_PORT="3306"
ENV DB_USER="user"
ENV DB_PASS="password"
ENV DB_NAME="outofschool"
# Default is latest migration
# set this variable if you want to revert to specific migration
ENV DB_MIGRATION_NAME=""

COPY --from=builder /src/OutOfSchool.Migrations/efbundle .
COPY --from=builder /src/OutOfSchool.Migrations/appsettings.json .
CMD exec ./efbundle --connection "server=${DB_HOST};port=${DB_PORT};user=${DB_USER};password=${DB_PASS};database=${DB_NAME};guidformat=binary16" ${DB_MIGRATION_NAME}