if ((Get-Command "openssl.exe" -ErrorAction SilentlyContinue) -eq $null) 
{ 
    Throw "Error: openssl is not installed.`n"
}
# TODO: Write PS script