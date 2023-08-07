$ProgressPreference = 'Continue'
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 2.0

New-Variable -Name curdir -Option Constant `
  -Value (Split-Path -Parent $MyInvocation.MyCommand.Definition)

Write-Host "[INFO] script directory: $curdir"

[Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor 'Tls12'

New-Variable -Name rabbitmq_version -Option Constant -Value '3.12.2'

$rabbitmq_dir = Join-Path -Path $curdir -ChildPath "rabbitmq_server-$rabbitmq_version"
$rabbitmq_sbin = Join-Path -Path $rabbitmq_dir -ChildPath 'sbin'
$rabbitmq_download_url = "https://github.com/rabbitmq/rabbitmq-server/releases/download/v$rabbitmq_version/rabbitmq-server-windows-$rabbitmq_version.zip"
$rabbitmq_zip_file = Join-Path -Path $curdir -ChildPath "rabbitmq-server-windows-$rabbitmq_version.zip"
$rabbitmq_plugins_cmd = Join-Path -Path $rabbitmq_sbin -ChildPath 'rabbitmq-plugins.bat'
$rabbitmq_server_cmd = Join-Path -Path $rabbitmq_sbin -ChildPath 'rabbitmq-server.bat'

$rabbitmq_base = Join-Path -Path $curdir -ChildPath 'rmq'
$rabbitmq_conf_in = Join-Path -Path $curdir -ChildPath 'rabbitmq.conf.in'
$rabbitmq_conf_out = Join-Path -Path $rabbitmq_base -ChildPath 'rabbitmq.conf'

if (!(Test-Path -Path $rabbitmq_dir))
{
    New-Item -Path $rabbitmq_base -ItemType Directory
    Invoke-WebRequest -Verbose -UseBasicParsing -Uri $rabbitmq_download_url -OutFile $rabbitmq_zip_file
    Expand-Archive -Path $rabbitmq_zip_file -DestinationPath $curdir
    & $rabbitmq_plugins_cmd enable rabbitmq_management
}

$pwd_slashes = $curdir -Replace '\\','/'
(Get-Content -Raw -Path $rabbitmq_conf_in) -Replace '@@PWD@@', $pwd_slashes | Set-Content -Path $rabbitmq_conf_out

try
{
    $env:RABBITMQ_ALLOW_INPUT = 'true'
    $env:RABBITMQ_BASE = $rabbitmq_base
    & $rabbitmq_server_cmd
}
finally
{
    Remove-Item -Path env:\RABBITMQ_ALLOW_INPUT
    Remove-Item -Path env:\RABBITMQ_BASE
}
