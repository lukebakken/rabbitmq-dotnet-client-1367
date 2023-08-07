$DebugPreference = "Continue"
$ErrorActionPreference = 'Stop'
$ProgressPreference = 'Continue'
# Set-PSDebug -Strict -Trace 1
Set-PSDebug -Off
Set-StrictMode -Version 'Latest' -ErrorAction 'Stop' -Verbose

New-Variable -Name curdir  -Option Constant -Value $PSScriptRoot
Write-Host "[INFO] curdir: $curdir"

[Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor 'Tls12'

New-Variable -Name rmq_version -Option Constant -Value '3.11.10'

# https://github.com/rabbitmq/rabbitmq-server/releases/download/v3.11.10/rabbitmq-server-windows-3.11.10.zip
New-Variable -Name rmq_zip_download_url -Option Constant -Value "https://github.com/rabbitmq/rabbitmq-server/releases/download/v$rmq_version/rabbitmq-server-windows-$rmq_version.zip"
New-Variable -Name rmq_zip_file -Option Constant -Value (Join-Path -Path $curdir -ChildPath "rabbitmq-server-windows-$rmq_version.zip")

if (-Not (Test-Path -Path $rmq_zip_file))
{
    Invoke-WebRequest -UseBasicParsing -Uri $rmq_zip_download_url -OutFile $rmq_zip_file
}
else
{
    Write-Host "[INFO] found '$rmq_zip_file'"
}

New-Variable -Name rmq_dir -Option Constant -Value (Join-Path -Path $curdir -ChildPath "rabbitmq_server-$rmq_version")

if (-Not (Test-Path -Path $rmq_dir -Type Container))
{
    Expand-Archive -Path $rmq_zip
}
else
{
    Write-Host "[INFO] found '$rmq_dir'"
}

New-Variable -Name rmq_base_dir -Option Constant -Value (Join-Path -Path $curdir -ChildPath 'rabbitmq')
New-Variable -Name rmq_log_dir -Option Constant -Value (Join-Path -Path $rmq_base_dir -ChildPath 'log')
New-Variable -Name rmq_config_file -Option Constant -Value (Join-Path -Path $curdir -ChildPath 'rabbitmq.conf')
New-Variable -Name rmq_enabled_plugins_file -Option Constant -Value (Join-Path -Path $rmq_base_dir -ChildPath 'enabled_plugins')

try
{
    $env:RABBITMQ_ALLOW_INPUT = 'true'
    $env:RABBITMQ_BASE = $rmq_base_dir
    $env:RABBITMQ_CONFIG_FILE = $rmq_config_file

    Remove-Item -Force $rmq_log_dir/*

    if (-Not (Test-Path -Path $rmq_enabled_plugins_file))
    {
        & "$rmq_dir/sbin/rabbitmq-plugins.bat" enable rabbitmq_management rabbitmq_top
    }
    else
    {
        Write-Host "[INFO] found '$rmq_enabled_plugins_file'"
    }

    & "$rmq_dir/sbin/rabbitmq-server.bat"
}
finally
{
    Remove-Item -Path env:\RABBITMQ_ALLOW_INPUT
    Remove-Item -Path env:\RABBITMQ_BASE
    Remove-Item -Path env:\RABBITMQ_CONFIG_FILE
}
