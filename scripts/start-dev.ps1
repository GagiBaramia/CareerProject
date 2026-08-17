# CareerProject dev გარემოს ერთბაშად გაშვება:
# Docker infra (Postgres/RabbitMQ/Redis) + 5 backend service + Angular dev server,
# თითოეული ცალკე ტერმინალის ფანჯარაში (log-ები ჩანს, ფანჯრის დახურვა აჩერებს მხოლოდ იმ service-ს).

$ErrorActionPreference = 'Stop'

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$root = Split-Path -Parent $scriptDir

Write-Host "CareerProject dev გარემოს გაშვება..." -ForegroundColor Cyan

# 1. Docker infra
Write-Host "`n[1/3] Docker infra (docker compose up -d)..." -ForegroundColor Yellow
Push-Location $root
docker compose up -d
Pop-Location

# 2. .env-ის ჩატვირთვა ამ პროცესის environment-ში - ყველა შემდეგ გახსნილი
#    ფანჯარა ამ პროცესის შვილია და ავტომატურად დაიმემკვიდრებს ამ env var-ებს.
$envFile = Join-Path $root ".env"
if (-not (Test-Path $envFile)) {
    Write-Host "`n.env ვერ მოიძებნა ($envFile)." -ForegroundColor Red
    Write-Host "დააკოპირე .env.example -> .env და შეავსე პაროლები, შემდეგ სცადე ხელახლა." -ForegroundColor Red
    exit 1
}

Get-Content $envFile | ForEach-Object {
    if ($_ -match '^\s*([^#=\s][^=]*)=(.*)$') {
        [System.Environment]::SetEnvironmentVariable($matches[1].Trim(), $matches[2].Trim())
    }
}

function Start-ServiceWindow {
    param(
        [string]$Title,
        [string]$WorkingDir,
        [string]$Command
    )

    $fullCommand = "`$Host.UI.RawUI.WindowTitle = '$Title'; Set-Location '$WorkingDir'; $Command"
    Start-Process powershell -ArgumentList '-NoExit', '-Command', $fullCommand | Out-Null
}

Write-Host "`n[2/3] Backend service-ების გაშვება (თითო ცალკე ფანჯარაში)..." -ForegroundColor Yellow

$services = @(
    @{ Title = 'UserService (5269)';           Dir = 'backend\CareerProject.UserService';           Port = 5269 },
    @{ Title = 'JobService (5135)';             Dir = 'backend\CareerProject.JobService';             Port = 5135 },
    @{ Title = 'RecommendationService (5190)';  Dir = 'backend\CareerProject.RecommendationService';  Port = 5190 },
    @{ Title = 'NotificationService (5156)';    Dir = 'backend\CareerProject.NotificationService';    Port = 5156 },
    @{ Title = 'ApiGateway (5178)';             Dir = 'backend\CareerProject.ApiGateway';             Port = 5178 }
)

foreach ($svc in $services) {
    $workingDir = Join-Path $root $svc.Dir
    $command = "`$env:ASPNETCORE_URLS = 'http://localhost:$($svc.Port)'; dotnet run"
    Start-ServiceWindow -Title $svc.Title -WorkingDir $workingDir -Command $command
    Start-Sleep -Seconds 2
}

Write-Host "`n[3/3] Angular dev server-ის გაშვება..." -ForegroundColor Yellow
Start-ServiceWindow -Title 'Angular (4200)' -WorkingDir (Join-Path $root 'frontend\career-project-web') -Command 'npx ng serve'

Write-Host "`nყველაფერი გაეშვა 6 ცალკე ფანჯარაში (5 backend + Angular)." -ForegroundColor Green
Write-Host "ბრაუზერი ავტომატურად გაიხსნება ~25 წამში (Angular-ს სჭირდება დრო აშენებას)." -ForegroundColor Green
Write-Host "ერთი service-ის გასაჩერებლად უბრალოდ დახურე მისი ფანჯარა." -ForegroundColor Green
Write-Host "ყველას ერთად გასაჩერებლად: scripts\stop-dev.ps1 (ან stop-dev.bat)." -ForegroundColor Green

Start-Sleep -Seconds 25
Start-Process 'http://localhost:4200'
