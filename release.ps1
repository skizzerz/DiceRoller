& "C:\Program Files\dotnet\dotnet.exe" clean DiceRoller\Dice.csproj --configuration Release --nologo
& "C:\Program Files\dotnet\dotnet.exe" build DiceRoller\Dice.csproj --configuration Release --nologo
& "C:\Program Files (x86)\Windows Kits\10\bin\10.0.18362.0\x86\signtool.exe" sign /tr http://timestamp.digicert.com /td sha256 /fd sha256 /a DiceRoller\bin\Release\net452\Dice.dll
& "C:\Program Files (x86)\Windows Kits\10\bin\10.0.18362.0\x86\signtool.exe" sign /tr http://timestamp.digicert.com /td sha256 /fd sha256 /a DiceRoller\bin\Release\netstandard2.0\Dice.dll
& "C:\Program Files\dotnet\dotnet.exe" pack DiceRoller\Dice.csproj --configuration Release --no-build --output . --include-symbols -p:SymbolPackageFormat=snupkg --nologo
$vers = Get-ChildItem DiceRoller.*.nupkg | ForEach-Object { New-Object System.Version($_.Name.Substring(11, $_.Name.Length - 17)) } | Sort-Object -Descending
$pkg = "DiceRoller.$($vers[0].Major).$($vers[0].Minor).$($vers[0].Build).nupkg"
# Publish to NuGet; api key must be saved via nuget setApiKey beforehand
D:\Programs\nuget.exe sign $pkg -Timestamper http://timestamp.digicert.com -CertificateFingerprint a87d3b543d551ccd330c23b31b653835a86569bc
D:\Programs\nuget.exe push $pkg -Source https://www.nuget.org
# Publish to GitHub (TODO: use github API for this)
Write-Output "Don't forget to cut a new release on GitHub!"