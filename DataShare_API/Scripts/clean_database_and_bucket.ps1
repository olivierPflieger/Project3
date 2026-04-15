<#
.SYNOPSIS
    Cleans the application database and empties the configured AWS S3 bucket.
    WARNING: Destructive action.
#>

$ConfirmPreference = 'High'

# --- Configuration ---
$S3BucketName = "oc-datashare-bucket"
$DatabaseName = "datashare"
$SqlServerInstance = "localhost"
$DbUser = "admin"
$DockerContainerName = "postgres_db"

Write-Warning "You are about to DELETE ALL FILES in AWS S3 bucket '$S3BucketName' and CLEAN THE DATABASE '$DatabaseName'."
$confirmation = Read-Host "Type 'YES' to proceed"

if ($confirmation -cne 'YES') {
    Write-Host "Operation cancelled." -ForegroundColor Yellow
    exit
}

# ---------------------------------------------------------
# 1. CLEAN AWS S3 BUCKET (Using AWS CLI)
# ---------------------------------------------------------
Write-Host "Emptying AWS S3 bucket: $S3BucketName..." -ForegroundColor Cyan
try {
    # The --recursive flag deletes all objects inside the bucket
    aws s3 rm s3://$S3BucketName --recursive
    Write-Host "Successfully emptied S3 bucket." -ForegroundColor Green
}
catch {
    Write-Error "Failed to clean AWS S3 bucket: $_"
}

# ---------------------------------------------------------
# 2. CLEAN DATABASE (Choose Option A or Option B)
# ---------------------------------------------------------
Write-Host "Cleaning database..." -ForegroundColor Cyan


try {
        
    $truncateQuery = '
        DELETE FROM "FileMetaDatas";
        DELETE FROM "Users";
    '

    $truncateQuery | docker exec -i $DockerContainerName psql -U $DbUser -d $DatabaseName
        
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Tables 'users' and 'filemetadatas' cleared successfully via Docker." -ForegroundColor Green
    } else {
        Write-Error "Database cleanup failed. Make sure your Docker container '$DockerContainerName' is running."
    }        
}
catch {
    Write-Error "Database cleanup failed : $_"
}

Write-Host "Cleanup script completed." -ForegroundColor Green