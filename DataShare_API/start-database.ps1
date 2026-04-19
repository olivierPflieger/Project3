# Host
$secret = dotnet user-secrets list | Select-String "POSTGRES_HOST"
$env:POSTGRES_HOST = ($secret -split "=")[1].Trim()
# Port
$secret = dotnet user-secrets list | Select-String "POSTGRES_PORT"
$env:POSTGRES_PORT = ($secret -split "=")[1].Trim()
# Database
$secret = dotnet user-secrets list | Select-String "POSTGRES_DB"
$env:POSTGRES_DB = ($secret -split "=")[1].Trim()
# User
$secret = dotnet user-secrets list | Select-String "POSTGRES_USER"
$env:POSTGRES_USER = ($secret -split "=")[1].Trim()
# Password
$secret = dotnet user-secrets list | Select-String "POSTGRES_PASSWORD"
$env:POSTGRES_PASSWORD = ($secret -split "=")[1].Trim()

docker compose up -d
dotnet ef database update