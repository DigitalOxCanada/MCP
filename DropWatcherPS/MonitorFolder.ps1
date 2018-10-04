#put this file in the folder where the json files are being dropped
$mongopath = 'c:\mongodb\bin'
$folder = $PSScriptRoot
$filter = '*.json'

$fsw = New-Object IO.FileSystemWatcher $folder, $filter -Property @{
 IncludeSubdirectories = $false
 NotifyFilter = [IO.NotifyFilters]'FileName, LastWrite'
}
$onCreated = Register-ObjectEvent $fsw Created -SourceIdentifier FileCreated -Action {
 $path = $Event.SourceEventArgs.FullPath
 $name = $Event.SourceEventArgs.Name
 $changeType = $Event.SourceEventArgs.ChangeType
 $timeStamp = $Event.TimeGenerated
 Write-Host "The file '$name' was $changeType at $timeStamp"
 
 $cmd = & $mongopath + '\mongoimport.exe' --host mcpserver:27017 --db mcp --collection packages --file $path
 # add --jsonArray if the file is going to have an array of items in it.
 
 Write-Host "Executing $cmd"
 
 $cmd

 $cmdb = & $mongopath + '\mongo' mcpserver:27017/mcp $PSScriptRoot + "\fixTimestamp.js"

 Write-Host "Fixing Timestamps command: $cmdb"
 
 $destination = $PSScriptRoot += '\processed'

 Move-Item $path -Destination $destination -Force -Verbose # Force will overwrite files with same name

 
# $cmdb
}

#Unregister-Event -SourceIdentifier FileCreated
