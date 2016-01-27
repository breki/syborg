#Install-WindowsFeature Web-WebServer -IncludeAllSubFeature

Import-Module WebAdministration
$iisAppPoolName = "syborg"
$iisAppPoolDotNetVersion = "v4.0"
$iisAppName = "syborg-tests"
$directoryPath = "Syborg.WebTests"

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
