# CareerProject-ის ყველა გაშვებული backend service-ისა და Angular dev server-ის გაჩერება.
# Docker infra (Postgres/RabbitMQ/Redis) არ ჩერდება - ის ცალკე გრძელდება, საჭიროებისამებრ
# გააჩერე ხელით: docker compose down

Write-Host "CareerProject-ის backend/frontend პროცესების გაჩერება..." -ForegroundColor Cyan

$processNames = @(
    'CareerProject.UserService',
    'CareerProject.JobService',
    'CareerProject.RecommendationService',
    'CareerProject.NotificationService',
    'CareerProject.ApiGateway'
)

foreach ($name in $processNames) {
    $procs = Get-Process -Name $name -ErrorAction SilentlyContinue
    if ($procs) {
        $procs | Stop-Process -Force
        Write-Host "  გაჩერდა: $name" -ForegroundColor Green
    }
}

# Angular dev server node.exe პროცესია - სახელით ვერ გამოვარჩევთ, command line-ით ვამოწმებთ
# (career-project-web-ის path-ზე, რომ სხვა, დაუკავშირებელი node პროცესები არ დავხუროთ).
$angularProcs = Get-CimInstance Win32_Process -Filter "Name='node.exe'" -ErrorAction SilentlyContinue |
    Where-Object { $_.CommandLine -like '*career-project-web*' }

foreach ($proc in $angularProcs) {
    Stop-Process -Id $proc.ProcessId -Force -ErrorAction SilentlyContinue
    Write-Host "  გაჩერდა: Angular dev server (PID $($proc.ProcessId))" -ForegroundColor Green
}

Write-Host "`nდასრულდა. Docker infra კვლავ მუშაობს (docker compose down რომ გინდა მისი გაჩერებაც)." -ForegroundColor Cyan
