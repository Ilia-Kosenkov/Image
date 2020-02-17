nuget pack Image.nuspec
ls *nupkg | sort -Property LastWriteTime -Descending | select -First 1 | foreach {nuget push $_.Name -source GPR}