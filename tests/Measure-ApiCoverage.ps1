param(
    [string]$ReportPath = (Join-Path $PSScriptRoot 'TestResults/api-coverage.cobertura.xml')
)

$ErrorActionPreference = 'Stop'

function Format-Coverage([int]$Covered, [int]$Total) {
    if ($Total -eq 0) { return 'n/a' }
    return ('{0}/{1} ({2:P1})' -f $Covered, $Total, ($Covered / $Total))
}

[xml]$report = Get-Content -Raw -LiteralPath $ReportPath
$coverage = $report.coverage
$classes = @($coverage.SelectNodes('packages/package/classes/class'))
$controllerFiles = $classes |
    Where-Object { $_.filename -match '(^|[\\/])Controllers[\\/]' } |
    Group-Object filename |
    Sort-Object Name

# Test fixtures load API database types in the runner. Controller hits prove the HTTP process was measured.
if (-not ($controllerFiles.Group | Where-Object { $_.SelectSingleNode('lines/line[@hits > 0]') })) {
    throw 'No executed API controller lines were recorded. Check coverage collection for the Aspire API process.'
}

'### API code coverage'
''
$lines = Format-Coverage $coverage.'lines-covered' $coverage.'lines-valid'
$branches = Format-Coverage $coverage.'branches-covered' $coverage.'branches-valid'
"WowsKarma.Api excluding migrations has $lines line coverage and $branches branch coverage."
''
'| Controller | Covered lines |'
'| --- | ---: |'

foreach ($file in $controllerFiles) {
    $lineGroups = @($file.Group.SelectNodes('lines/line') | Group-Object number)
    $covered = @($lineGroups | Where-Object { $_.Group | Where-Object { [long]$_.hits -gt 0 } }).Count
    $name = ($file.Name -split '[\\/]')[-1]
    "| $name | $(Format-Coverage $covered $lineGroups.Count) |"
}
