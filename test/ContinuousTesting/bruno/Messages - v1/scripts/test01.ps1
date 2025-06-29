# Example of comment-based help for a script or function:
<#
.SYNOPSIS
  run bruno tests and extract ids from results
.DESCRIPTION
  use local environment to run bruno tests and extract ids from the results
.PARAMETER param1
  Description of param1.
.EXAMPLE
  PS> .\yourscript.ps1 -param1 value
#>
bru run --env local --output ./temp/results.json

# extract all the ids from the results
$content = (Get-Content -Raw -Path ./temp/results.json | ConvertFrom-Json)
$results = $content.results | Where-Object { $_.test.filename -like '*GetMessageThread.bru' }
$results 
$data = $results.response.data
$data
$data | ForEach-Object { [PSCustomObject]@{ id = $_.id; title = $_.title; } } | Export-Csv -Path './temp/ids.csv' -NoTypeInformation