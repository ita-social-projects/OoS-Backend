$SEL = Select-String -Path $env:windir\System32\drivers\etc\hosts -Pattern "oos.local"
if ($null -eq $SEL)
{
    Add-Content -Path $env:windir\System32\drivers\etc\hosts -Value "`n192.168.100.100`toos.local" -Force
}