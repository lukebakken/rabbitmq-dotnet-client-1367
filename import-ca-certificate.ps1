$ProgressPreference = 'Continue'
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 2.0

New-Variable -Name curdir -Option Constant `
  -Value (Split-Path -Parent $MyInvocation.MyCommand.Definition)

New-Variable -Name ca_cert_file -Option Constant `
  -Value (Join-Path -Path $curdir -ChildPath 'certs' | Join-Path -ChildPath 'ca_certificate.crt')

Import-Certificate -Verbose -FilePath $ca_cert_file -CertStoreLocation cert:\CurrentUser\Root
