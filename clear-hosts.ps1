$CurrentHosts = Get-Content -Path $env:windir\System32\drivers\etc\hosts
$NewHosts = $CurrentHosts | Where-Object {$_ -notmatch 'oos.local'}
Set-Content -Path $env:windir\System32\drivers\etc\hosts -Value $NewHosts -Force