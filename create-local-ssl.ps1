param(
    [Parameter(Mandatory=$True, ValueFromPipeline=$false)]
    [System.String]
    $Domain
) 

if ($null -eq (Get-Command "openssl.exe" -ErrorAction SilentlyContinue)) 
{ 
    Throw "Error: openssl is not installed.`n"
}

$Https = ".\https"

if (-Not (Test-Path -Path $Https)) {
    mkdir $Https
    $Env:Passphrase=-join ((0x30..0x39) + ( 0x41..0x5A) + ( 0x61..0x7A) | Get-Random -Count 128  | ForEach-Object {[char]$_})
    $subj="/C=UA/ST=Kyiv/L=Kyiv/O=SS/OU=ITA/CN=$Domain/emailAddress=admin@$Domain/"
    # TODO: Check if SAN is set
    openssl req `
        -x509 `
        -newkey rsa:2048 `
        -keyout $Https\${Domain}.key `
        -out $Https\${Domain}.crt `
        -days 365 `
        -subj $subj `
        -addext "subjectAltName = DNS:${Domain},DNS:*.${Domain},IP:192.168.100.100" `
        -passout env:Passphrase

    openssl dhparam `
        -out $Https\dhparam.pem 2048 2>$null
    
    openssl rsa `
        -in $Https\${Domain}.key `
        -out $Https\${Domain}.key `
        -passin env:Passphrase
    
    $LocalCert = Get-ChildItem -Path Cert:\LocalMachine\Root | Where-Object { $_.Subject -match "CN=$Domain" }
    if ($LocalCert) {
        certutil -delstore -f "ROOT" ${LocalCert}.Thumbprint
    }
    certutil -addstore -f "ROOT" $Https\${Domain}.crt
}