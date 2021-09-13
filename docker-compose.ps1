$Env:TAG=git rev-parse --short HEAD

if ($null -eq (Get-Command "docker-compose.exe" -ErrorAction SilentlyContinue)) 
{ 
    Throw "Error: Docker compose is not installed.`n"
}

docker-compose `
    -f docker-compose.yml `
    -f docker-compose.local.yml `
    up -d
