$ProgressPreference = 'Continue'
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 2.0

New-Variable -Name curdir -Option Constant `
  -Value (Split-Path -Parent $MyInvocation.MyCommand.Definition)

& openssl x509 -in ./certs/ca_certificate.pem -out ./certs/ca_certificate.crt

& openssl pkcs12 -inkey ./certs/client_localhost_key.pem -in ./certs/client_localhost_certificate.pem -certfile ./certs/ca_certificate.pem -export -out ./certs/client_localhost_certificate.pfx

New-Variable -Name ca_cert_file -Option Constant `
  -Value (Join-Path -Path $curdir -ChildPath 'certs' | Join-Path -ChildPath 'ca_certificate.crt')

Import-Certificate -Verbose -FilePath $ca_cert_file -CertStoreLocation cert:\CurrentUser\Root
