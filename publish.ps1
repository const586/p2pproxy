echo $args[0]
echo $args[1]
echo $args[2]
echo $args[3]

$plugins = "SavePlaylist","TorrentTelik","xbmc.pvr","PluginFavourites","UnrestrictedContents"

$destination = $args[3]
$solution = $args[0]
$project = $args[1]
$target = $args[2]

if (-Not (Test-Path $destination)) {
    mkdir $destination
}
if (-Not (Test-Path $destination\plugins)) {
    mkdir $destination\plugins
}
cd $target
cp * $destination -Exclude @("*.pdb", "*.config", "*.manifest", "*.vshost*")
ls -Attributes Directory | foreach {
    #echo ("robocopy " + $_.Name + " " + $destination + $_.Name)
    robocopy $_.Name ("" + $destination + $_.Name)
}

foreach ($plug in $plugins) {
    if (Test-Path $solution$plug\bin\Release\$plug.dll) {
        cp $solution$plug\bin\Release\$plug.dll $destination\plugins\$plug.dll
    }
}
if (Test-Path $destination\p2pproxy.7z) {
    del $destination\p2pproxy.7z
}
& 'C:\Program Files\7-Zip\7z.exe' a -t7z $destination\p2pproxy.7z $destination*