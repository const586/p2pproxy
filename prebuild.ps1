$plugins = "SavePlaylist","TorrentTelik","xbmc.pvr","PluginFavourites","UnrestrictedContents"
#$plugins = "SavePlaylist"
$solution = "e:\source\TTVProxy\"
cd C:\Windows\Microsoft.NET\Framework\v4.0.30319
chcp 855
foreach ($plug in $plugins) {
    ./msbuild $solution$plug\$plug.csproj /p:AssemblyName=$plug /t:Rebuild /p:Configuration=Release
    
}