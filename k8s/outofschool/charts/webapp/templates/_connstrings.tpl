{{/*
Create a connection string with password and username
*/}}
{{- define "webapp.mysqlconnection" -}}
{{- $namespace := (index . "namespace") | default "" }}
{{- $username := (index . "username") | default "" }}
{{- $secretname := (index . "secretname") | default "" }}
{{- $secretkey := (index . "secretkey") | default "" }}
{{- $secret := (lookup "v1" "Secret" $namespace $secretname) | default dict }}
{{- $password := (get $secret $secretkey) | default "" }}
{{- $connection := printf "server=mysql;user=%s;password=%s;database=outofschool;guidformat=binary16" $username $password }}
ConnectionStrings__DefaultConnection: {{ $connection }}
{{- end }}