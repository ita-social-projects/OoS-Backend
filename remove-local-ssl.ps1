param(
    [Parameter(Mandatory=$True, ValueFromPipeline=$false)]
    [System.String]
    $Domain
) 

$Https = ".\https"
$LocalCert = Get-ChildItem -Path Cert:\LocalMachine\Root | Where-Object { $_.Subject -match "CN=$Domain" }
if ($LocalCert) {
    certutil -delstore -f "ROOT" ${LocalCert}.Thumbprint
}
if (Test-Path -Path $Https) {
    Remove-Item -Path $Https -Recurse
}