@echo off

dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
if errorlevel 1 (
    echo Test run failed, exiting.
    exit /b %errorlevel%
)

::dotnet tool install --global dotnet-reportgenerator-globaltool
reportgenerator -reports:coverage.cobertura.xml -targetdir:coverage-report
if errorlevel 1 (
    echo Report generation failed, exiting.
    exit /b %errorlevel%
)

start coverage-report\index.html
