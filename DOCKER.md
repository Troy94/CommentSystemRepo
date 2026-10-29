# 🐳 Docker & Docker Compose

Полное руководство по запуску всей системы комментариев в контейнерах.

## 📋 Требования

- Docker (версия 20.10+)
- Docker Compose (версия 1.29+)

## 🚀 Быстрый старт

```bash
# 1. Перейти в корневую директорию проекта
cd D:\STUDY\ProjectComments

# 2. Запустить все контейнеры
docker-compose up -d

# 3. Проверить статус
docker-compose ps

# 4. Просмотреть логи
docker-compose logs -f
```

После запуска приложение доступно по адресам:
- **Frontend**: http://localhost:3000
- **Backend API**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)
- **Redis**: localhost:6379

## 🏗️ Архитектура

```
┌─────────────────┐
│  comment_client │ (React 3000)
└────────┬────────┘
		 │
┌────────▼────────┐
│  comment_api    │ (ASP.NET Core 5000)
└────────┬────────┘
		 │
	┌────┴───┬─────────┬─────────┐
	│         │         │         │
┌───▼──┐ ┌───▼──┐ ┌───▼──┐ ┌───▼──┐
│ SQL  │ │Redis │ │RabMQ │ │ Logs │
│2022  │ │6379  │ │5672  │ │      │
└──────┘ └──────┘ └──────┘ └──────┘
```

## 📦 Сервисы

### 1. **sqlserver** - MSSQL Server 2022
```
Host: localhost:1433
Container: sqlserver
Пользователь: sa
Пароль: YourPassword123!
База: CommentSystemDb
```

### 2. **redis** - Redis Cache
```
Host: localhost:6379
Container: redis
Используется для кэширования
```

### 3. **rabbitmq** - Message Broker
```
HTTP: http://localhost:15672
AMQP: localhost:5672
Пользователь: guest
Пароль: guest
```

### 4. **comment_api** - Backend API
```
URL: http://localhost:5000
Container: comment_api
Зависит от: sqlserver, redis, rabbitmq
```

### 5. **comment_client** - React Frontend
```
URL: http://localhost:3000
Container: comment_client
Зависит от: comment_api
Соединяется к API через: http://comment_api:5000
```

## 🔧 Управление контейнерами

### Запуск
```bash
# Запустить все контейнеры
docker-compose up -d

# Запустить в режиме для разработки (видеть логи)
docker-compose up

# Запустить конкретный сервис
docker-compose up -d comment_client
```

### Остановка
```bash
# Остановить все контейнеры
docker-compose stop

# Остановить конкретный контейнер
docker-compose stop comment_client

# Удалить контейнеры
docker-compose down
```

### Логи
```bash
# Просмотреть логи всех сервисов
docker-compose logs -f

# Логи конкретного сервиса
docker-compose logs -f comment_client
docker-compose logs -f comment_api

# Последние 100 строк
docker-compose logs --tail=100 comment_api
```

### Перестроение
```bash
# Перестроить образы
docker-compose build

# Перестроить и запустить
docker-compose up -d --build

# Перестроить конкретный сервис
docker-compose build comment_client
docker-compose up -d comment_client
```

## 🖥️ Доступ к контейнерам

### Выполнить команду в контейнере
```bash
# Интерактивный bash в React контейнере
docker-compose exec comment_client sh

# Интерактивный bash в API контейнере
docker-compose exec comment_api bash

# Выполнить одну команду
docker-compose exec comment_api ls -la
```

### Подключиться к SQL Server
```bash
# Из хост-машины
sqlcmd -S localhost,1433 -U sa -P YourPassword123!

# Из контейнера
docker-compose exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourPassword123!
```

## 🌐 Сетевое взаимодействие

Все контейнеры находятся в одной сети (`app-network`), что позволяет им общаться по именам:

```
comment_client -> comment_api:5000 ✅
comment_api -> sqlserver:1433 ✅
comment_api -> redis:6379 ✅
comment_api -> rabbitmq:5672 ✅
```

**Важно**: При обращении из браузера используйте `localhost`, а не имя контейнера!

## 📝 Переменные окружения

### Frontend (comment_client)
```env
REACT_APP_API_URL=http://comment_api:5000
PORT=3000
```

### Backend (comment_api)
```env
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:5000
ConnectionStrings__DefaultConnection=Server=sqlserver,1433;Database=CommentSystemDb;User Id=sa;...
Redis__Host=redis:6379
MessageBroker__Host=amqp://rabbitmq
```

Для переопределения переменных создайте `.env` файл в корне проекта.

## 🏥 Проверка здоровья контейнеров

Каждый сервис имеет healthcheck. Проверить статус:

```bash
docker-compose ps

# Статус будут показаны как "healthy" или "unhealthy"
```

## 💾 Данные и том

```yaml
volumes:
  db_data:  # Сохраняется данные SQL Server между перезапусками
```

Очистить данные:
```bash
docker-compose down -v  # -v удалит все тома
```

## 🔐 Безопасность

### Production-ready конфигурация

Для production среды создайте `docker-compose.prod.yml`:

```bash
# Запуск production конфигурации
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

Измените:
- 🔐 Пароли (SQL, RabbitMQ)
- 🌍 API_URL на production домен
- 📍 Адреса хостов
- 🔒 Добавьте HTTPS/TLS

## 📊 Мониторинг

### Просмотр использования ресурсов
```bash
docker-compose stats
```

### Просмотр сетевой активности
```bash
docker network inspect <network_name>
```

## 🚨 Решение проблем

### Контейнер постоянно перезагружается
```bash
# Проверьте логи
docker-compose logs <service_name>

# Проверьте healthcheck статус
docker inspect <container_name> | grep -A 10 "Health"
```

### Не может подключиться к SQL Server
```bash
# Проверьте, запущен ли sqlserver
docker-compose ps sqlserver

# Проверьте логи SQL Server
docker-compose logs sqlserver

# Убедитесь, что здоров
docker-compose ps | grep sqlserver  # Должно быть "healthy"
```

### React не подключается к API
1. Проверьте переменную окружения `REACT_APP_API_URL`
2. Убедитесь, что API запущен: `docker-compose logs comment_api`
3. Проверьте CORS настройки в backend
4. Если локально: используйте `http://localhost:5000`
5. Если в контейнере: используйте `http://comment_api:5000`

### Ошибка "port already in use"
```bash
# Найдите процесс, использующий порт (Windows PowerShell)
Get-NetTCPConnection -LocalPort 3000 | Select-Object -Property State, OwningProcess

# Найти процесс (Linux/Mac)
lsof -i :3000

# Или используйте другой порт
docker-compose -f docker-compose.yml -e PORT=3001 up -d comment_client
```

## 📚 Дополнительно

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Reference](https://docs.docker.com/compose/compose-file/)
- [Best Practices](https://docs.docker.com/develop/dev-best-practices/)
