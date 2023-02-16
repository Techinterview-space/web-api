param (    
    [Parameter(Mandatory=$true)][string]$migrationName
)

cd src
cd Domain
dotnet ef migrations add $migrationName
Read-Host "Press ENTER"