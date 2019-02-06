$NS = "Freecon.Core.Networking.Models.Proto"
$OUTPUT = "..\SRServer\Core.Networking.Models.Proto\"

cd .\Proto

foreach ($file in get-childitem -path .\*.proto)
{
  .\tools\protogen.exe -i:$file.FullName -o:$OUTPUT\$($file.BaseName).cs -ns:$NS -p:fixCase -p:detectMissing
}

cd ..