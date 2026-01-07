{{- define "fcg-pagamentos-api.name" -}}
fcg-usuarios-api
{{- end -}}

{{- define "fcg-pagamentos-api.fullname" -}}
{{ .Release.Name }}-{{ include "fcg-pagamentos-api.name" . }}
{{- end -}}
