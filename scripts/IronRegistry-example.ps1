Add-PSSnapin "Microsoft.SharePoint.PowerShell" -ErrorAction SilentlyContinue
[System.Reflection.Assembly]::LoadWithPartialName("IronSharePoint.Framework")

$ir = [IronSharePoint.Administration.IronRegistry]::Local
$ir.FarmEnvironment = [IronSharePoint.IronEnvironment]::Production # Default IronEnvironment

$rootSite = Get-SPSite 'http://my-sharepoint'
$hiveSite = Get-SPSite 'http://my-sharepoint/sites/IronHive'

$hive = $ir.Hives.Add();
$hive.HiveArguments = $hiveSite.ID;
$hive.HiveType = [IronSharePoint.Hives.SPDocumentHive];
$hive.DisplayName = "My Hive";
$hive.Description = "Main Hive for http://my-sharepoint"
$hive.Priority = 1; # Files in hives with higher priority have precendence in case of duplicates

# OPTIONAL shared hive
# $sharedHiveSite = Get-SPSite 'http://my-sharepoint/sites/SharedHive'
# $sharedHive = $ir.Hives.Add();
# $sharedHive.HiveArguments = $sharedHiveSite.ID;
# $sharedHive.HiveType = [IronSharePoint.Hives.SPDocumentHive];
# $sharedHive.DisplayName = "My Shared Hive";
# $sharedHive.Description = "Shared hive for localization files etc."
# $sharedHive.Priority = 20; # Lower priority s.t. the files can be overridden

$myRT = $ir.Runtimes.Add();
$myRT.Environment = [IronSharePoint.IronEnvironment]::Development # OPTIONAL: override default IronEnvironment for this runtime
$myRT.DisplayName = "My Runtime";
$myRT.Description = "Runtime for http://my-sharepoint and http://my-sharepoint/sites/IronHive"
$myRT.AddHive($hive);
# $myRT.AddHive($sharedHive);

$ir.Associate($rootSite, $myRT); # Associate $rootSite to use IronControls on the Site
$ir.Associate($hiveSite, $myRT); # Associate $hiveSite to use an IronConsole on the Site

$ir.Hives # Show all Hives
$ir.Runtimes # Show all Runtimes

#$ir.Update($true) # Persists changes. Passing $true activates the hive feature on hive sites