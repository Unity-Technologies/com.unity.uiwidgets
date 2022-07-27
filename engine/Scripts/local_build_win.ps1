param(
    [Parameter()]
    [String]$mode= "release"
)
$current_dir = Get-Location
$localLibsPath = Join-Path -Path $current_dir -ChildPath "../libs/engine_dependencies"

python3 lib_build.py -m $mode -p windows -l True -r $localLibsPath