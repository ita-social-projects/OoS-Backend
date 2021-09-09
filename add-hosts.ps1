param(
    [Parameter(Mandatory=$True, ValueFromPipeline=$false)]
    [System.String]
    $Ip
)

$SEL = Select-String -Path $env:windir\System32\drivers\etc\hosts -Pattern "oos.local"
if ($null -eq $SEL)
{
    Add-Content -Path $env:windir\System32\drivers\etc\hosts -Value "`n$Ip`toos.local" -Force
}