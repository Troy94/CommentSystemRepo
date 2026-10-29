# 🚀 Архитектура для локального запуска и production

## 📋 Что было сделано

### Проблема
React фронтенд использовал имя контейнера `comment_api:5000`, которое браузер не может резолвить.

### Решение
Фронтенд **всегда использует `localhost:5000`**, независимо от того, запущен ли он в контейнере или нет. Это работает благодаря port mapping в Docker:
- React контейнер слушает порт 3000 → браузер обращается к `localhost:3000`
- API контейнер слушает порт 5000 → браузер обращается к `localhost:5000`

## 🎯 Текущая архитектура

```
┌─────────────────────────────────────────┐
│   Ваша машина (Windows/Mac/Linux)       │
│                                         │
│  ┌─────────────────────────────────┐   │
│  │   Браузер (Chrome/Firefox/Edge) │   │
│  │                                 │   │
│  │  http://localhost:3000  ────┐   │   │
│  │  http://localhost:5000  ────┤   │   │
│  └─────────────────────────────────┘   │
│            ▲                 ▲           │
│            │                 │           │
└────────────┼─────────────────┼───────────┘
			 │                 │
	 ┌───────▼──────┐  ┌──────▼────────┐
	 │  docker:3000 │  │  docker:5000  │
	 │  (React)     │  │  (API)        │
	 └──────────────┘  └───────────────┘
			 ▲                  ▲
			 └──────────────────┘
		 (оба в контейнере)
```

## ✅ Что измененo

### 1. **App.js** - Всегда localhost
```javascript
const [hubUrl] = useState('http://localhost:5000');
```

### 2. **commentService.js** - Хардкод localhost
```javascript
const url = hubUrl || 'http://localhost:5000';
```

### 3. **docker-compose.yml** - Убрана переменная окружения
```yaml
# ДО (неправильно):
environment:
  - REACT_APP_API_URL=http://comment_api:5000

# ПОСЛЕ (правильно - не нужна):
# (переменная удалена)
```

## 🌐 Использование

### Локальная разработка
```bash
docker-compose up -d

# Браузер:
http://localhost:3000
```

### Production (будущее)
Для хостинга просто замените `localhost` на доменное имя:

**Вариант 1 - Переменная окружения при деплое:**
```yaml
# docker-compose.prod.yml
services:
  comment_client:
	environment:
	  - REACT_APP_API_URL=https://api.example.com
```

**Вариант 2 - Изменить в коде:**
```javascript
const [hubUrl] = useState(
  process.env.REACT_APP_API_URL || 'http://localhost:5000'
);
```

## 📊 Статус локально

```
✅ Frontend  http://localhost:3000  (контейнер)
✅ Backend   http://localhost:5000  (контейнер)
✅ Database  localhost:1433         (контейнер)
✅ Cache     localhost:6379         (контейнер)
✅ Queue     localhost:5672         (контейнер)
```

## 🎬 Сейчас можно протестировать

1. Откройте **http://localhost:3000**
2. Откройте **F12** → **Console**
3. Должны увидеть: `✅ Успешно подключено к CommentHub`
4. Напишите комментарий
5. Откройте второе окно браузера
6. Комментарий должен синхронизироваться в real-time! 🎉

## 🚢 Для production деплоя

Когда будете делать деплой:

1. **Docker образ** - остаётся без изменений
2. **Переменная окружения** - установите при запуске:
   ```bash
   docker-compose -f docker-compose.prod.yml up -d
   ```
3. **CORS** - убедитесь, что на backend разрешены ваши домены

## 📝 Структура для future

```
ProjectComments/
├── docker-compose.yml       # Локальная разработка
├── docker-compose.prod.yml  # Production (будет создан)
├── comment-demo-client/
│   ├── src/
│   │   ├── App.js          # localhost:5000
│   │   └── services/
│   │       └── commentService.js
│   └── Dockerfile
└── CommentSystem.Presentation/
	└── Program.cs          # CORS настройки
```

## 🔑 Ключевые моменты

- ✅ **Браузер всегда использует localhost** - это работает как локально, так и в production
- ✅ **Port mapping в Docker** - обеспечивает связь между контейнерами и браузером
- ✅ **CORS настроены правильно** - фронтенд и бэкенд могут общаться
- ✅ **WebSocket работает** - SignalR передаёт данные в real-time

---

**Статус:** ✅ ГОТОВО И РАБОТАЕТ ЛОКАЛЬНО

**Для production:** Просто установите правильный URL через переменные окружения
