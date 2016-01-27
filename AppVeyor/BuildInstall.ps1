#Install-WindowsFeature Web-WebServer -IncludeAllSubFeature

Import-Module WebAdministration
$iisAppPoolName = "syborg"
$iisAppPoolDotNetVersion = "v4.0"
$iisAppName = "syborg-tests"
$currentDir = Get-Location
$appPath = Join-Path $currentDir "\Syborg.WebTests"

#navigate to the app pools root
cd IIS:\AppPools\

#check if the app pool exists
if (!(Test-Path $iisAppPoolName -pathType container))
{
    #create the app pool
    $appPool = New-Item $iisAppPoolName
    $appPool | Set-ItemProperty -Name "managedRuntimeVersion" -Value $iisAppPoolDotNetVersion

	"Created application pool '{0}'" -f $iisAppPoolName
}

New-WebApplication -Name $iisAppName -ApplicationPool $iisAppPoolName -Force -PhysicalPath $appPath -Site "Default Web Site"