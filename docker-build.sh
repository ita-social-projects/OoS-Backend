#!/usr/bin/env bash

if [ "$IN_VAGRANT" ]; then
    cd /vagrant
fi  

TAG=$(git rev-parse --short HEAD)
PACK=$(command -v pack)

if ! [ -x "$PACK" ]; then
    echo 'Error: pack is not installed.' >&2
    echo 'Installation instructions: https://buildpacks.io/docs/tools/pack/' >&2
    exit 1
fi

if [[ "$(docker images -q oos-nginx:${TAG} 2> /dev/null)" == "" ]]; then
    CURRENT=$(pwd)
    cd Nginx
    $PACK build oos-nginx:${TAG} \
        --buildpack gcr.io/paketo-buildpacks/nginx \
        --builder paketobuildpacks/builder:base
    cd ${CURRENT}
fi

$PACK build oos-auth:${TAG} \
    --buildpack gcr.io/paketo-buildpacks/dotnet-core \
    --builder paketobuildpacks/builder:base \
    --env BP_DOTNET_PROJECT_PATH=./OutOfSchool/OutOfSchool.IdentityServer/

$PACK build oos-api:${TAG} \
    --buildpack gcr.io/paketo-buildpacks/dotnet-core \
    --builder paketobuildpacks/builder:base \
    --env BP_DOTNET_PROJECT_PATH=./OutOfSchool/OutOfSchool.WebApi/
