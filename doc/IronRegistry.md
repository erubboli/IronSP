IronRegistry
============

Powershell Basics
-----------------

Load IronSharePoint Assembly `[System.Reflection.Assembly]::LoadWithPartialName("IronSharePoint.Framework")`

Get local IronRegistry
```
$ir = [IronSharePoint.Administration.IronRegistry]::Local
```

Runtimes
--------

Properties
  * Id: Unique identifier of the runtime
  * DisplayName: Display name of the runtime. Should be unique
  * Description: Description of the runtime. Optional, can be anything.
  * Hives: List of all hives registered for this runtime
  * GemPaths: List of all gem paths for this runtime

List all runtimes 
```
$runtimes = $ir.Runtimes
```

Get runtime 
  * by display name
    `$rt = $ir.Runtimes["Display Name"]`
  * by Guid
    `$rt = $ir.Runtimes[(New-Object System.Guid ED082290-2072-406A-9888-AEE46C3B8692)]`
  * by index
    `$rt = $ir.Runtimes[0]`

Add a new runtime
```
$rt = $ir.Runtimes.Add()
```

Add a hive to the runtime
```
$rt.AddHive($ir.Hives["My Hive"])
$rt.AddHive((New-Object System.Guid ED082290-2072-406A-9888-AEE46C3B8692))
```

Remove a hive from the runtime
```
$rt.RemoveHive($ir.Hives["My Hive"])
$rt.RemoveHive((New-Object System.Guid ED082290-2072-406A-9888-AEE46C3B8692))
```
 
Add a gem path to the runtime
```
$rt.AddGemPath("c:\foo\bar")
```

Remove a gem path from the runtime
```
$rt.RemoveGemPath("c:\foo\bar")
```

Example:
```
$rt = $ir.Runtimes.Add();
$rt.Environment = [IronSharePoint.IronEnvironment]::Development
$rt.DisplayName = "My Runtime";
$rt.Description = "Runtime for http://my-sharepoint and http://my-sharepoint/sites/IronHive"
$myRT.AddHive($ir.Hives["My Hive"]);
```

Hives
-----

Properties:
  * Id: Unique identifier of the runtime
  * DisplayName: Display name of the hive. Should be unique
  * Description: Description of the hive. Optional, can be anything.
  * Priority: Lower number -> higher priority for files
  * HiveType: .NET Type of the Hive
  * HiveArguments: Constructor arguments for the HiveType

List all hives
```
$hives = $ir.Hives
```

Get hive 
  * by display name
    `$hive = $ir.Hives["Display Name"]`
  * by Guid
    `$hive = $ir.Hives[(New-Object System.Guid ED082290-2072-406A-9888-AEE46C3B8692)]`
  * by index
    `$hive = $ir.Hives[0]`

Add a new hive
```
$rt = $ir.Hive.Add()
```

Associations
------------

List all associations
```
$associations = $ir.Associations
```

 * *Key*: Target Id (Site, SiteSubscription, ...)
 * *Value*: Runtime Id

Associate a runtime with a target
```
$ir.Associate((Get-SPSite http://my-sharepoint), $rt)
```

Overwrite Association
```
$ir.Associate((Get-SPSite http://my-sharepoint), $rt, $true)
```

Dissociate a runtime from a target
```
$ir.Dissociate((Get-SPSite http://my-sharepoint), $rt)
$ir.Dissociate((New-Object System.Guid ED082290-2072-406A-9888-AEE46C3B8692), $rt)
```

 
