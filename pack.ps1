#####NuGet.CommandLine is too old and does not support Xamarin.iOS libs
#####Please install the package "NuGet.CommandLine" from https://chocolatey.org/ before running this script
#####After chocolatey is installed, type: choco install NuGet.CommandLine
#####Before running this script, download nuget.exe from @echo https://nuget.codeplex.com/releases/view/133091
#####and put nuget.exe in the path.

#####set /p nugetServer=Enter base nuget server url (with /): 
$nugetServer="http://nugets.org/"
$msbuild = 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe'
$version="1.0.0"

#####################
#Build release config
cd $PSScriptRoot
cd ..
nuget restore Vapolia.KeyValueLite.sln
$msbuildparams = '/t:Clean;Build', '/p:Configuration=Release', '/p:Platform=Any CPU', 'Vapolia.KeyValueLite.sln'
& $msbuild $msbuildparams
cd nuget

del *.nupkg

nuget pack "Vapolia.KeyValueLite.nuspec" -Version $version
nuget push "Vapolia.KeyValueLite.$version.nupkg" -Source $nugetServer

#####set assembly info to version
#####https://gist.github.com/derekgates/4678882
