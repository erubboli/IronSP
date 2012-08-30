
function Global:Get-IronHiveRegistry
{
    Add-PSSnapin "Microsoft.SharePoint.PowerShell" -ErrorAction SilentlyContinue
    [void][System.Reflection.Assembly]::LoadWithPartialName("IronSharePoint")
    return [IronSharePoint.Administration.IronHiveRegistry]::Local
}



'Use Get-IronHiveRegistry to load the IronHiveRegistry' | out-host

$x =  Get-IronHiveRegistry
$site = Get-SPSite http://intranet/sites/IronSharePoint
$x.AddHiveMapping($site, $site)