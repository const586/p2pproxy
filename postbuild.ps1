$plugins = "SavePlaylist","TorrentTelik","xbmc.pvr","PluginFavourites","UnrestrictedContents"
$destination = "D:\p2pproxy\"

foreach ($plug in $plugins) {
#    C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild $plug\$plug.csproj /t:Build /p:Configuration=Release
}

if (-Not (Test-Path $destination)) {
    mkdir $destination
}
if (-Not (Test-Path $destination\plugins)) {
    mkdir $destination\plugins
}

C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe .\TTVProxy_win\P2pProxy_win.csproj /t:Build /p:Configuration=Release
C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe .\TTVProxy_console\P2pProxy_console.csproj /t:Build /p:Configuration=Release

#cp .\TTVProxy_win\bin\Release\* $destination -Exclude @("*.pdb", "*.config", "*.manifest", "*.vshost*")
#ls -Attributes Directory | foreach {
    #echo ("robocopy " + $_.Name + " " + $destination + $_.Name)
robocopy TTVProxy_win\bin\Release\ ("" + $destination + $_.Name) /s /xf *.pdb,*.config,*.manifest,*.vshost*
#}
cp .\TTVProxy_console\bin\Release\P2pProxy_console.exe $destination
foreach ($plug in $plugins) {
    if (Test-Path $plug\bin\Release\$plug.dll) {
        cp $plug\bin\Release\$plug.dll $destination\plugins\$plug.dll
    }
}
if (Test-Path $destination\p2pproxy.7z) {
    del $destination\p2pproxy.7z
}
& 'C:\Program Files\7-Zip\7z.exe' a -t7z $destination\p2pproxy.7z $destination*