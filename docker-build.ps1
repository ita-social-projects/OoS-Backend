$Tag=git rev-parse --short HEAD

if ($null -eq (Get-Command "pack.exe" -ErrorAction SilentlyContinue)) 
{ 
    Throw "Error: pack is not installed.`nInstallation instructions: https://buildpacks.io/docs/tools/pack/"
}

if (-Not (docker images -q oos-nginx:$Tag)) {
    $Current=(Get-Item .).FullName
    cd .\Nginx
    pack build oos-nginx:$Tag `
        --buildpack gcr.io/paketo-buildpacks/nginx `
        --builder paketobuildpacks/builder:base
    cd $Current
}

pack build oos-auth:$Tag `
    --buildpack gcr.io/paketo-buildpacks/dotnet-core `
    --builder paketobuildpacks/builder:base `
    --env BP_DOTNET_PROJECT_PATH=./OutOfSchool/OutOfSchool.IdentityServer/

pack build oos-api:$Tag `
    --buildpack gcr.io/paketo-buildpacks/dotnet-core `
    --builder paketobuildpacks/builder:base `
    --env BP_DOTNET_PROJECT_PATH=./OutOfSchool/OutOfSchool.WebApi/
