{{/*
Expand the name of the chart.
*/}}
{{- define "beachbreak.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
We truncate at 63 chars because some Kubernetes name fields are limited to this (by the DNS naming spec).
If release name contains chart name it will be used as a full name.
*/}}
{{- define "beachbreak.fullname" -}}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := default .Chart.Name .Values.nameOverride }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}
{{- end }}

{{/*
Create chart name and version as used by the chart label.
*/}}
{{- define "beachbreak.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Common labels
*/}}
{{- define "beachbreak.labels" -}}
helm.sh/chart: {{ include "beachbreak.chart" . }}
{{ include "beachbreak.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Selector labels
*/}}
{{- define "beachbreak.selectorLabels" -}}
app.kubernetes.io/name: {{ include "beachbreak.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{/*
Create the name of the service account to use
*/}}
{{- define "beachbreak.serviceAccountName" -}}
{{- if .Values.serviceAccount.create }}
{{- default (include "beachbreak.fullname" .) .Values.serviceAccount.name }}
{{- else }}
{{- default "default" .Values.serviceAccount.name }}
{{- end }}
{{- end }}

{{/*
Common environment variables for all services
*/}}
{{- define "beachbreak.commonEnv" -}}
- name: ASPNETCORE_ENVIRONMENT
  value: "Production"
- name: ASPNETCORE_URLS
  value: "http://+:8080"
- name: AzureAd__Instance
  valueFrom:
    configMapKeyRef:
      name: {{ include "beachbreak.fullname" . }}-config
      key: azure-ad-instance
- name: AzureAd__Domain
  valueFrom:
    configMapKeyRef:
      name: {{ include "beachbreak.fullname" . }}-config
      key: azure-ad-domain
- name: AzureAd__TenantId
  valueFrom:
    secretKeyRef:
      name: {{ include "beachbreak.fullname" . }}-secrets
      key: azure-ad-tenant-id
- name: AzureAd__ClientId
  valueFrom:
    secretKeyRef:
      name: {{ include "beachbreak.fullname" . }}-secrets
      key: azure-ad-client-id
- name: AzureAd__ClientSecret
  valueFrom:
    secretKeyRef:
      name: {{ include "beachbreak.fullname" . }}-secrets
      key: azure-ad-client-secret
- name: AzureAd__Audience
  valueFrom:
    secretKeyRef:
      name: {{ include "beachbreak.fullname" . }}-secrets
      key: azure-ad-audience
- name: AzureAd__Scope
  valueFrom:
    secretKeyRef:
      name: {{ include "beachbreak.fullname" . }}-secrets
      key: azure-ad-scope
- name: ConnectionStrings__beachbreakdb
  valueFrom:
    secretKeyRef:
      name: {{ include "beachbreak.fullname" . }}-secrets
      key: database-connection-string
{{- end }}

{{/*
Service discovery environment variables (replaces Aspire)
*/}}
{{- define "beachbreak.serviceDiscoveryEnv" -}}
- name: services__CommandApi__https__0
  value: "http://{{ .Values.global.serviceDiscovery.commandApiService }}:{{ .Values.global.serviceDiscovery.commandApiPort }}"
- name: services__QueryApi__https__0
  value: "http://{{ .Values.global.serviceDiscovery.queryApiService }}:{{ .Values.global.serviceDiscovery.queryApiPort }}"
{{- end }}

{{/*
Database connection string
*/}}
{{- define "beachbreak.databaseConnectionString" -}}
Host={{ .Values.global.database.host }};Port={{ .Values.global.database.port }};Database={{ .Values.global.database.name }};Username={{ .Values.global.database.username }};Password={{ .Values.global.database.password }};Include Error Detail=true;
{{- end }}

{{/*
Image repository with registry
*/}}
{{- define "beachbreak.imageRepository" -}}
{{- if .Values.global.imageRegistry }}
{{- printf "%s/%s" .Values.global.imageRegistry .Values.global.imageRepository }}
{{- else }}
{{- .Values.global.imageRepository }}
{{- end }}
{{- end }}