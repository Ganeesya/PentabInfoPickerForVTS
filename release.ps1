Remove-Item "VTSPentabController" -Recurse
Remove-Item  "VTSPentabController.zip"
cp -force -r "VTSPentabPlugin\bin\Release\netcoreapp3.1" "VTSPentabController"
Compress-Archive -Path "VTSPentabController" -DestinationPath "VTSPentabController.zip"
$host.UI.RawUI.ReadKey()