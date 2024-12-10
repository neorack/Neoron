# Clean previous results
Remove-Item -Path "./TestResults" -Recurse -ErrorAction SilentlyContinue

# Run tests with coverage
dotnet test `
    /p:CollectCoverage=true `
    /p:CoverletOutputFormat="cobertura,opencover" `
    /p:CoverletOutput="./TestResults/" `
    /p:Exclude="[*.Tests]*,[*]*.Migrations.*" `
    /p:Include="[Neoron.API]*" `
    /p:ExcludeByAttribute="GeneratedCodeAttribute,CompilerGeneratedAttribute" `
    /p:ExcludeByFile="**/Migrations/*.cs" `
    /p:SkipAutoProps=true `
    --logger "console;verbosity=detailed" `
    --logger "trx;LogFileName=TestResults.trx" `
    --settings "./test.runsettings" `
    --results-directory "./TestResults"

# Generate coverage report
dotnet reportgenerator `
    -reports:"./TestResults/coverage.cobertura.xml" `
    -targetdir:"./TestResults/CoverageReport" `
    -reporttypes:"Html;Cobertura;Badges;SonarQube" `
    -classfilters:"+*;-*.Migrations.*" `
    -title:"Neoron API Coverage Report" `
    -verbosity:"Info" `
    -tag:"$((Get-Date).ToString('yyyy-MM-dd_HH-mm-ss'))"

# Display results summary
Write-Host "`nTest Results:" -ForegroundColor Cyan
Get-Content "./TestResults/coverage.cobertura.xml" | Select-String -Pattern 'line-rate="([^"]*)"' | ForEach-Object {
    $coverage = [math]::Round(($_.Matches.Groups[1].Value -as [decimal]) * 100, 2)
    Write-Host "Code Coverage: $coverage%" -ForegroundColor $(if ($coverage -ge 80) { "Green" } else { "Yellow" })
}

# Check minimum coverage threshold
$minCoverage = 80
if ($coverage -lt $minCoverage) {
    Write-Host "Coverage below minimum threshold of $minCoverage%" -ForegroundColor Red
    exit 1
}

function Generate-PerformanceReport {
    param (
        [string]$ResultsPath
    )
    
    $results = Get-ChildItem -Path $ResultsPath -Filter "*.trx" | 
        ForEach-Object { 
            [xml](Get-Content $_.FullName)
        }
    
    $performanceTests = $results.TestRun.Results.UnitTestResult | 
        Where-Object { $_.TestCategory -eq "Performance" }
    
    $report = @{
        TotalTests = $performanceTests.Count
        PassedTests = ($performanceTests | Where-Object { $_.outcome -eq "Passed" }).Count
        FailedTests = ($performanceTests | Where-Object { $_.outcome -eq "Failed" }).Count
        AverageDuration = ($performanceTests.duration | Measure-Object -Average).Average
    }
    
    return $report
}

# Generate performance report
$perfReport = Generate-PerformanceReport -ResultsPath "./TestResults/Performance"
Write-Host "`nPerformance Test Results:" -ForegroundColor Cyan
Write-Host "Total Tests: $($perfReport.TotalTests)"
Write-Host "Passed: $($perfReport.PassedTests)"
Write-Host "Failed: $($perfReport.FailedTests)"
Write-Host "Average Duration: $($perfReport.AverageDuration)ms"
