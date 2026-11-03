import * as signalR from '@microsoft/signalr';

const API_URL =
  process.env.REACT_APP_API_URL ||
  'https://symmetrical-goggles-qvq75g55qjg29jw-5000.app.github.dev';

export const HUB_URL = `${API_URL}/commenthub`;
const COMMENTS_URL = `${API_URL}/api/comments`;

// Достаёт понятный текст ошибки из ответа бекенда
// (ValidationProblem -> json с errors, BadRequest("...") -> строка).
async function readError(response, fallback) {
  try {
    const text = await response.text();
    if (!text) return fallback;
    try {
      const json = JSON.parse(text);
      if (typeof json === 'string') return json;
      if (json.errors) {
        const messages = Object.values(json.errors).flat();
        if (messages.length) return messages.join(' ');
      }
      if (json.title) return json.title;
    } catch {
      /* не JSON — вернём как есть */
    }
    return text;
  } catch {
    return fallback;
  }
}

async function request(url, options, fallbackError) {
  const response = await fetch(url, options);
  if (!response.ok) {
    throw new Error(await readError(response, fallbackError));
  }
  return response;
}

const jsonOptions = (method, body) => ({
  method,
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(body),
});

class CommentService {
  // Создаёт (но не запускает) подключение к хабу.
  createConnection({ onCreated, onUpdated, onDeleted, onStateChange }) {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL)
      .withAutomaticReconnect()
      .build();

    connection.on('CommentCreated', (comment) => onCreated?.(comment));
    connection.on('CommentUpdated', (payload) => onUpdated?.(payload));
    connection.on('CommentDeleted', (payload) => onDeleted?.(payload));

    connection.onreconnecting(() => onStateChange?.(false));
    connection.onreconnected(() => onStateChange?.(true));
    connection.onclose(() => onStateChange?.(false));

    return connection;
  }

  // Возвращает { page, pageSize, totalCount, totalPages, items }
  // items — комментарии верхнего уровня, у каждого вложенный replies.
  async getComments({ page = 1, pageSize = 10, sortBy = 'CreatedAt', sortDirection = 'desc' } = {}) {
    const query = new URLSearchParams({ page, pageSize, sortBy, sortDirection });
    const response = await request(
      `${COMMENTS_URL}/main?${query}`,
      undefined,
      'Не удалось загрузить комментарии'
    );
    return response.json();
  }

  async createComment({ userName, email, text, parentId, captchaToken }) {
    const response = await request(
      COMMENTS_URL,
      jsonOptions('POST', { userName, email, text, parentId, captchaToken }),
      'Не удалось отправить комментарий'
    );
    return response.json();
  }

  async updateComment(id, { userName, email, text, parentId }) {
    // Бекенд валидирует модель целиком, поэтому шлём все обязательные поля.
    await request(
      `${COMMENTS_URL}/${id}`,
      jsonOptions('PUT', { id, userName, email, text, parentId }),
      'Не удалось обновить комментарий'
    );
  }

  async deleteComment(id) {
    await request(`${COMMENTS_URL}/${id}`, { method: 'DELETE' }, 'Не удалось удалить комментарий');
  }
}

export default new CommentService();