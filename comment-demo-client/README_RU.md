# 💬 Система комментариев - React Frontend

Простой фронтенд приложение на React для демонстрации системы комментариев в реальном времени с использованием SignalR.

## 🚀 Возможности

- ✅ Подключение к SignalR хабу в реальном времени
- ✅ Добавление новых комментариев
- ✅ Редактирование комментариев
- ✅ Удаление комментариев
- ✅ Live обновление - все изменения отображаются мгновенно
- ✅ Статус подключения к серверу
- ✅ Обработка ошибок
- ✅ Красивый интерфейс с поддержкой мобильных устройств

## 📋 Требования

- Node.js (v14 или выше)
- npm или yarn
- Backend сервер (ASP.NET Core с SignalR)

## 🔧 Установка

```bash
# Перейти в директорию проекта
cd comment-demo-client

# Установить зависимости
npm install
```

## 🎯 Запуск в режиме разработки

```bash
npm start
```

Приложение откроется в браузере на `http://localhost:3000`

## 🐳 Запуск в Docker

### Как отдельный контейнер

```bash
# Собрать образ
docker build -t comment-demo-client .

# Запустить контейнер
docker run -p 3000:3000 -e REACT_APP_API_URL=http://localhost:5000 comment-demo-client
```

### С помощью docker-compose

Запустить все контейнеры (backend + frontend + database):

```bash
# Из корневой директории проекта
cd ..
docker-compose up -d

# Приложение будет доступно на http://localhost:3000
```

Детальные инструкции по Docker см. в `../../DOCKER.md`

## 📦 Сборка для production

```bash
npm run build
```

## 🔌 Конфигурация SignalR

По умолчанию фронтенд пытается подключиться к:
```
http://localhost:5000/commenthub
```

Для изменения адреса хаба, отредактируйте в файле `src/App.js`:

```javascript
const [hubUrl] = useState('http://your-server:port/commenthub');
```

## 📁 Структура проекта

```
comment-demo-client/
├── src/
│   ├── components/          # React компоненты
│   │   ├── CommentForm.js   # Форма для добавления комментариев
│   │   ├── CommentList.js   # Список комментариев
│   │   └── ConnectionStatus.js  # Статус подключения
│   ├── services/            # Сервисы
│   │   └── commentService.js # SignalR сервис
│   ├── styles/              # CSS стили
│   │   ├── CommentForm.css
│   │   ├── CommentList.css
│   │   └── ConnectionStatus.css
│   ├── App.js               # Главный компонент
│   ├── App.css              # Стили приложения
│   ├── index.js             # Точка входа
│   └── index.css            # Глобальные стили
├── package.json
└── README.md
```

## 🔗 API методы

### CommentService

#### `connect(hubUrl)`
Подключиться к SignalR хабу
```javascript
await CommentService.connect('http://localhost:5000/commenthub');
```

#### `sendComment(content, userId)`
Отправить новый комментарий
```javascript
await CommentService.sendComment('Мой комментарий', 'user123');
```

#### `getComments()`
Получить все комментарии
```javascript
const comments = await CommentService.getComments();
```

#### `invoke(methodName, ...args)`
Вызвать метод на сервере
```javascript
await CommentService.invoke('UpdateComment', commentId, newContent);
await CommentService.invoke('DeleteComment', commentId);
```

#### Слушатели событий
```javascript
CommentService.onCommentReceived((comment) => {
  // Новый комментарий получен
});

CommentService.onCommentUpdated((comment) => {
  // Комментарий обновлен
});

CommentService.onCommentDeleted((commentId) => {
  // Комментарий удален
});
```

## 🎨 Стили

Приложение использует современные CSS стили с:
- Градиентные фоны
- Анимации и переходы
- Адаптивный дизайн
- Цветовая схема Bootstrap

## 🐛 Решение проблем

### Не подключается к серверу
1. Проверьте, запущен ли backend сервер
2. Проверьте адрес хаба в `App.js`
3. Убедитесь, что CORS настроен на backend

### CORS ошибки
Добавьте CORS политику в backend (`Program.cs` или `Startup.cs`):

```csharp
builder.Services.AddCors(options =>
{
	options.AddPolicy("ReactApp", builder =>
	{
		builder
			.WithOrigins("http://localhost:3000")
			.AllowAnyMethod()
			.AllowAnyHeader()
			.AllowCredentials();
	});
});
```

## 📝 Лицензия

MIT

## 👨‍💻 Автор

Создано для демонстрации системы комментариев на SignalR
