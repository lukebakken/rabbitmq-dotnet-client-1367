$ProgressPreference = 'Continue'
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 2.0

New-Variable -Name curdir -Option Constant `
  -Value (Split-Path -Parent $MyInvocation.MyCommand.Definition)

New-Variable -Name ca_cert_file -Option Constant `
  -Value (Join-Path -Path $curdir -ChildPath 'certs' | Join-Path -ChildPath 'ca_certificate.crt')

Import-Certificate -Verbose -FilePath $ca_cert_file -CertStoreLocation cert:\CurrentUser\Root

New-Variable -Name projdir -Option Constant `
  -Value (Join-Path -Path $curdir -ChildPath 'RabbitMQ.Explore')

New-Variable -Name appsettings_json -Option Constant `
  -Value (Join-Path -Path $projdir -ChildPath 'appsettings.json')

New-Variable -Name curdir2 -Option Constant `
  -Value ($curdir -Replace '\\','\\')
(Get-Content -Raw -Path $appsettings_json) -Replace '..\\\\..\\\\..\\\\..', $curdir2 | Set-Content -Path $appsettings_json
