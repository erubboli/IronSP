$packages = ls -Filter packages.config -recurse
foreach($package in $packages) { ./tools/NuGet.exe i $package.FullName -o packages }
