#Set-ExecutionPolicy Unrestricted
Set-ExecutionPolicy Unrestricted -Scope CurrentUser
#Set-ExecutionPolicy RemoteSigned
$global:FuncID = [GUID]("2b807c90-e356-4d6d-8ba7-78ca112f5e5d")
$global:DevKeyID = [GUID]("e31e0df2-0982-4784-ad53-344cfe4234d8")
$global:JobID = [GUID]::NewGuid()


function DoWork()
{
    # CREATE KEY/VAL PAIR DATA
    #$kv = (New-Object PSObject |
    #		Add-Member -PassThru NoteProperty Filename "c:\test.bat" |
    #		Add-Member -PassThru NoteProperty URL "\\server\filename.bat"
    #		) | ConvertTo-JSON

    $users = Get-ADUser -Properties cn -Filter  {(Enabled -eq $false)} | ? { ($_.distinguishedname -notlike '*Disabled Users*') }
    $blob = ''
    $cnt = 0
	$errcnt = 0


    Foreach($user in $users)
    {
        $blob = $blob + "$($user.samAccountName): $($user.DistinguishedName)`r`n"
        $errcnt = $errcnt + 1
		$packagetype = 2
    }

	if($errcnt -eq 0) {
		$blob = "There are no disabled users.";
		$packagetype = 0
	}

	$ts = get-date -format ("yyyy-MM-ddThh:mm:ss")
	$UtcTime = ((get-date).ToUniversalTime()).ToString("yyyy-MM-ddThh:mm:ss.fffffffZ")
	

	$keyvals = (New-Object PSObject |
			Add-Member -PassThru NoteProperty Server "yourdomainserver"
			)
	
	$kv = (New-Object PSObject |
			Add-Member -PassThru NoteProperty FunctionID $FuncID | 
    		Add-Member -PassThru NoteProperty DevKeyID $DevKeyID |
			Add-Member -PassThru NoteProperty JobID $JobID |
			Add-Member -PassThru NoteProperty DT $UtcTime |
			Add-Member -PassThru NoteProperty Type $packagetype |
			Add-Member -PassThru NoteProperty NoticeCount $cnt |
			Add-Member -PassThru NoteProperty ErrorCount $errcnt |
			Add-Member -PassThru NoteProperty WarningCount 0 |
			Add-Member -PassThru NoteProperty InfoCount 0 |
			Add-Member -PassThru NoteProperty Blob $blob |
			Add-Member -PassThru NoteProperty KeyVals $keyvals
    		) | ConvertTo-JSON 

	#Write-Host $kv
	
	$dt = get-date -format ("yyyyMMddhhmmssfff")
	$outfilename = "\\mcpserver\MCPDropBox\" + $FuncID + "_" + $dt + ".json"
	#$outfilename = "D:\temp\" + $FuncID + "_" + $dt + ".json"
	$stream = new-object System.IO.StreamWriter($outfilename)
	$stream.WriteLine($kv)
	$stream.close()
	
    #MCPUtils.CreateAndSendPackage $packagetype $cnt $blob
}

DoWork

Write-Host "Done program"