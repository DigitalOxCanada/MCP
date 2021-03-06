#Set-ExecutionPolicy Unrestricted 
Set-ExecutionPolicy Unrestricted -Scope CurrentUser
#Set-ExecutionPolicy RemoteSigned
$global:FuncID = [GUID]("af713f87-a605-46b8-8b03-10245a4cf54c")
$global:DevKeyID = [GUID]("e31e0df2-0982-4784-ad53-344cfe4234d8")
$global:JobID = [GUID]::NewGuid()


function DoWork()
{
	$outputString = ""
	$stringBuilder = New-Object System.Text.StringBuilder
    # CREATE KEY/VAL PAIR DATA
    #$kv = (New-Object PSObject |
    #		Add-Member -PassThru NoteProperty Filename "c:\test.bat" |
    #		Add-Member -PassThru NoteProperty URL "\\server\filename.bat"
    #		) | ConvertTo-JSON
	$blob = '';
	
	$users = Get-ADUser -Properties DisplayName, MemberOf, EmployeeID -Filter `
					{(Enabled -eq $true) -and (EmployeeID -notlike '*')} | `
									Where-Object { `
													!($_.MemberOf -like "*CN=Audit Contractors,*") -and `
													!($_.MemberOf -like "*CN=Students,*") -and `
													!($_.MemberOf -like "*CN=Board Members,*") -and ` 
													!($_.DistinguishedName -like "*ou=Service Accounts,*")}
	
    $cnt = 0

	
	ForEach($user in $users)
	{
		$null = $stringBuilder.Append("$($user.DisplayName), $($user.DistinguishedName)`r`n")
		$packagetype = 0
	}
	$cnt = $users.length
	$blob = $stringBuilder.ToString()
	
	Write-Host $packagetype
	
	if($cnt -eq 0) {
		$blob = "There are no users.";
		$packagetype = 0
	}

	$ts = get-date -format ("yyyy-MM-ddThh:mm:ss")
	$UtcTime = ((get-date).ToUniversalTime()).ToString("yyyy-MM-ddThh:mm:ss.fffffffZ")

	$keyvals = (New-Object PSObject |
			Add-Member -PassThru NoteProperty Server "yourdomainserver"
			)
	#"DT" : ISODate("2015-12-23T21:46:45.35Z"),
	$kv = (New-Object PSObject |
			Add-Member -PassThru NoteProperty FunctionID $FuncID | 
    		Add-Member -PassThru NoteProperty DevKeyID $DevKeyID |
			Add-Member -PassThru NoteProperty JobID $JobID |
			Add-Member -PassThru NoteProperty DT $UtcTime |
			Add-Member -PassThru NoteProperty Type $packagetype |
			Add-Member -PassThru NoteProperty NoticeCount $cnt |
			Add-Member -PassThru NoteProperty ErrorCount 0 |
			Add-Member -PassThru NoteProperty WarningCount 0 |
			Add-Member -PassThru NoteProperty InfoCount $cnt |
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

    #MCPUtils.CreateAndSendPackage $packagetype $cnt $outputString
}

DoWork

Write-Host "Done program"