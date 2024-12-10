dotnet test /p:CollectCoverage=true `
            /p:CoverletOutputFormat=cobertura `
            /p:CoverletOutput=./TestResults/ `
            /p:Exclude="[*.Tests]*" `
            --settings test.runsettings

dotnet reportgenerator `
       -reports:"./TestResults/coverage.cobertura.xml" `
       -targetdir:"./TestResults/CoverageReport" `
       -reporttypes:Html
