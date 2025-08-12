# Автоматическое резервное копирование с версионированием
$backupRoot = "Z:\Development\Backups\MinimalApi"
$dateStamp = Get-Date -Format "yyyyMMdd"

# Находим последнюю версию для текущей даты
$latestVersion = Get-ChildItem -Path $backupRoot -Directory |
    Where-Object { $_.Name -match "${dateStamp}_v(\d+)" } |
    ForEach-Object {
        [int]($_.Name -replace "${dateStamp}_v", '')
    } |
    Measure-Object -Maximum |
    Select-Object -ExpandProperty Maximum

# Определяем новую версию
$nextVersion = if ($latestVersion) { $latestVersion + 1 } else { 1 }
$backupDir = Join-Path -Path $backupRoot -ChildPath "${dateStamp}_v$nextVersion"

# Создаем папку для резервной копии
New-Item -ItemType Directory -Path $backupDir -Force

# Копируем проект
Copy-Item -Path ".\MagicVilla_CouponAPI" -Destination $backupDir -Recurse -Force

Write-Host "Резервная копия $nextVersion создана в: $backupDir"