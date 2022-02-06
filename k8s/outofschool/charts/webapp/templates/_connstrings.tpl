{{/*
Create a connection string with password and username
*/}}
{{- define "webapp.mysqlconnection" -}}
{{- $username := (index . "username") | default "" }}
{{- $password := (index . "password") | default "" }}
{{- $release := (index . "release") | default "outofschool"}}
{{- $connection := printf "server=%s-mysql;user=%s;password=%s;database=outofschool;guidformat=binary16" $release $username $password }}
ConnectionStrings__DefaultConnection: {{ $connection | b64enc }}
{{- end }}