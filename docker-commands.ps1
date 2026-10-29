# Docker Compose Helper Script for Windows PowerShell
# Вспомогательные команды для управления контейнерами

Write-Host "📦 Comment System - Docker Compose Helper" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Доступные команды:" -ForegroundColor Yellow
Write-Host ""

$commands = @(
	@{
		Num = "1"
		Desc = "Запуск всех контейнеров"
		Cmd = "docker-compose up -d"
	},
	@{
		Num = "2"
		Desc = "Остановка всех контейнеров"
		Cmd = "docker-compose down"
	},
	@{
		Num = "3"
		Desc = "Просмотр логов всех сервисов"
		Cmd = "docker-compose logs -f"
	},
	@{
		Num = "4"
		Desc = "Просмотр логов React frontend"
		Cmd = "docker-compose logs -f comment_client"
	},
	@{
		Num = "5"
		Desc = "Просмотр логов API backend"
		Cmd = "docker-compose logs -f comment_api"
	},
	@{
		Num = "6"
		Desc = "Статус всех контейнеров"
		Cmd = "docker-compose ps"
	},
	@{
		Num = "7"
		Desc = "Перестроить образы"
		Cmd = "docker-compose build"
	},
	@{
		Num = "8"
		Desc = "Перестроить и запустить"
		Cmd = "docker-compose up -d --build"
	},
	@{
		Num = "9"
		Desc = "PowerShell в React контейнере"
		Cmd = "docker-compose exec comment_client sh"
	},
	@{
		Num = "10"
		Desc = "PowerShell в API контейнере"
		Cmd = "docker-compose exec comment_api bash"
	},
	@{
		Num = "11"
		Desc = "Удалить контейнеры и тома"
		Cmd = "docker-compose down -v"
	}
)

foreach ($cmd in $commands) {
	Write-Host "$($cmd.Num). $($cmd.Desc):" -ForegroundColor Green
	Write-Host "   $($cmd.Cmd)" -ForegroundColor White
	Write-Host ""
}

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "🌐 После запуска доступно:" -ForegroundColor Yellow
Write-Host "   Frontend: http://localhost:3000" -ForegroundColor Cyan
Write-Host "   Backend:  http://localhost:5000" -ForegroundColor Cyan
Write-Host "   Swagger:  http://localhost:5000/swagger" -ForegroundColor Cyan
Write-Host "   RabbitMQ: http://localhost:15672 (guest/guest)" -ForegroundColor Cyan
