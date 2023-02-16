param (    
    [Parameter(Mandatory=$true)][string]$migrationName
)
dotnet ef migrations add $migrationName
Read-Host "Press ENTER"