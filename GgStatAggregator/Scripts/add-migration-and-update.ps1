param (
    [Parameter(Mandatory = $true)]
    [string]$MigrationName
)

# Define environments
$environments = @("Development", "Production")

# Create migration (default env, usually Development)
Write-Host "Creating migration: $MigrationName"
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet ef migrations add $MigrationName

# Apply migration to each environment
foreach ($envName in $environments) {
    Write-Host "`nApplying migration to $envName database..."
    $env:ASPNETCORE_ENVIRONMENT = $envName
    dotnet ef database update
}

Write-Host "`n✅ Migration '$MigrationName' created and applied to both Development and Production databases."
