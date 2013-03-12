function Global:Get-IronHiveRegistry
{
    Add-PSSnapin "Microsoft.SharePoint.PowerShell" -ErrorAction SilentlyContinue
    [void][System.Reflection.Assembly]::LoadWithPartialName("IronSharePoint.Framework")
    return [IronSharePoint.Framework.Administration.IronHiveRegistry]::Local
}

'Use Get-IronHiveRegistry to load the IronHiveRegistry'| out-host
'Example:'| out-host
'$target = Get-SPWebApplication "http://intranet"'| out-host
'$hiveSite = Get-SPSite "http://intranet/sites/site"'| out-host
'$ihr = Get-IronHiveRegistry'| out-host
'$ihr.AddTrustedHive($hiveSite.Id)'| out-host
'$ihr.TrustedHives'| out-host
'$ihr.AddHiveMapping($hiveSite, $target)'| out-host
'$ihr.HiveMappings' | out-host
'$ihr.Update($true) ' | out-host
