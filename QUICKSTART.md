# 🚀 Быстрый старт - Система комментариев

## ⚡ Самый быстрый способ (Docker Compose)

```bash
# 1. Откройте PowerShell или терминал в папке проекта
cd D:\STUDY\ProjectComments

# 2. Запустите все контейнеры
docker-compose up -d

# 3. Откройте браузер
http://localhost:3000
```

**Готово!** 🎉 Система запущена и работает.

## 📋 Что запустилось

| Сервис | Адрес | Статус |
|--------|-------|--------|
| 💬 **Frontend** | http://localhost:3000 | React приложение |
| 🔌 **Backend API** | http://localhost:5000 | ASP.NET Core |
| 📚 **Swagger Docs** | http://localhost:5000/swagger | API документация |
| 🐰 **RabbitMQ** | http://localhost:15672 | Message Broker (guest/guest) |
| 📊 **Redis** | localhost:6379 | Cache |
| 🗄️ **SQL Server** | localhost:1433 | Database |

## 🎮 Управление контейнерами

### Просмотр логов
```bash
# Все логи в реальном времени
docker-compose logs -f

# Только логи frontend
docker-compose logs -f comment_client

# Только логи backend
docker-compose logs -f comment_api
```

### Остановка
```bash
# Остановить все
docker-compose stop

# Остановить один контейнер
docker-compose stop comment_client
```

### Перезагрузка
```bash
# Перезагрузить все
docker-compose restart

# Перезагрузить один сервис
docker-compose restart comment_client
```

### Удаление
```bash
# Удалить контейнеры (данные сохранятся)
docker-compose down

# Удалить контейнеры и все данные
docker-compose down -v
```

## 👨‍💻 Для разработчиков

### Запуск локально (без Docker)

#### Frontend
```bash
cd comment-demo-client
npm install
npm start
# Откроется http://localhost:3000
```

#### Backend
- Откройте `CommentSystem.sln` в Visual Studio
- Установите зависимости (NuGet)
- Нажмите F5 для запуска
- Будет доступен на http://localhost:5000

### Запуск в Docker с горячей перезагрузкой

1. Раскомментируйте строки в `docker-compose.override.yml`:
```yaml
services:
  comment_client:
	build:
	  context: ./comment-demo-client
	  dockerfile: Dockerfile.dev
	volumes:
	  - ./comment-demo-client:/app
	  - /app/node_modules
	command: npm start
```

2. Запустите:
```bash
docker-compose up -d --build comment_client
docker-compose logs -f comment_client
```

Теперь изменения в коде будут автоматически отражаться (при сохранении файлов).

## 🐛 Решение проблем

### React не подключается к API
```bash
# Проверьте, запущен ли backend
docker-compose ps comment_api

# Проверьте логи backend
docker-compose logs comment_api

# Убедитесь, что backend здоров
docker-compose ps  # смотрите STATUS
```

### "port already in use" error
```bash
# Найдите процесс на порту (в PowerShell):
Get-NetTCPConnection -LocalPort 3000

# Используйте другой порт:
# Отредактируйте docker-compose.yml
# - "3001:3000"  вместо  - "3000:3000"
```

### SQL Server не запускается
```bash
# Проверьте логи
docker-compose logs sqlserver

# Убедитесь, что достаточно памяти (нужно минимум 3.25GB)
# Перезапустите Docker Desktop
```

### Хотите полностью очистить систему
```bash
# Удалить все контейнеры и тома
docker-compose down -v

# Удалить неиспользуемые образы
docker image prune -a

# Полная переустановка
docker-compose build --no-cache
docker-compose up -d
```

## 📚 Подробная документация

- **Docker & Compose**: см. [DOCKER.md](./DOCKER.md)
- **Frontend**: см. [comment-demo-client/README_RU.md](./comment-demo-client/README_RU.md)
- **Вспомогательные команды**: [docker-commands.ps1](./docker-commands.ps1)

## 🔗 Полезные ссылки

- [Docker Docs](https://docs.docker.com/)
- [Docker Compose Reference](https://docs.docker.com/compose/compose-file/)
- [React Documentation](https://react.dev)
- [ASP.NET Core SignalR](https://learn.microsoft.com/en-us/aspnet/core/signalr)

---

**Есть вопросы?** Проверьте логи:
```bash
docker-compose logs -f
```
