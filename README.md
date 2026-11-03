## Что реализовано?
- Трёхшаровая архитектура.
- Использование патерна "Repository", в комбинации с "UnitOfWork" для удобной читаемости api.
- Создание базы по принципу CodeFirst.
- Сервис уведомлений через RabbitMQ
- Кеширование отображения комментариев с помощью Redis
- Быстрое обновление комментариев у всех подписчиков с помощью SignalR
- Полное логирование через Serilog.
- backend подготовлен к использовани CAPCHA

## Что можно добавить?
- Патерн CicuitBraker мог бы защитить от бесконечного спама в логах, и организовать переподключения для Redis и Rabbit.
- Добить регистрацию токена Capcha.
- Добавить авторизацию и аутентификацию с помощью jwt.
- Добавить дополнительный способ регистрации через Auth0
- Купить хостинг ща 7$

## 📋 Требования

- Docker (версия 20.10+)
- Docker Compose (версия 1.29+)

## 🚀 Быстрый старт

```bash
# 1. Перейти в корневую директорию проекта
cd *ваш путь*\ProjectComments

# 2. Запустить все контейнеры
docker-compose up -d

# 3. Проверить статус
docker-compose ps

# 4. Просмотреть логи, если нужно
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
┌─────────────────────────────────────────────────────────────┐
│                    DOCKER COMPOSE NETWORK                   │
│                       (app-network)                          │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌──────────────────┐         ┌──────────────────┐           │
│  │  comment_client  │         │  comment_api     │           │
│  │  (React 3000)    │────────▶│ (ASP.NET 5000)   │           │
│  │                  │         │                  │           │
│  │ - CommentForm    │         │ - SignalR Hub    │           │
│  │ - CommentList    │         │ - API Endpoints  │           │
│  │ - Status         │         │ - Controllers    │           │
│  └──────────────────┘         └────────┬─────────┘           │
│                                        │                     │
│        ┌──────────────────────────────┼──────────────────┐   │
│        │                              │                  │   │
│   ┌────▼────┐  ┌─────────┐  ┌────────▼────┐  ┌────────┐  │   │
│   │sqlserver │  │  redis  │  │  rabbitmq   │  │ logs   │  │   │
│   │ (1433)   │  │ (6379)  │  │   (5672)    │  │        │  │   │
│   │          │  │         │  │  (15672)    │  │        │  │   │
│   └──────────┘  └─────────┘  └─────────────┘  └────────┘  │   │
│                                                               │
└─────────────────────────────────────────────────────────────┘
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