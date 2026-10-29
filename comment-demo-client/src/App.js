import React, { useState, useEffect, useCallback, useRef } from 'react';
import './App.css';
import CommentService, { HUB_URL } from './services/commentService';
import CommentForm from './components/CommentForm';
import CommentList from './components/CommentList';
import ConnectionStatus from './components/ConnectionStatus';
import { decodeHtml } from './utils/format';

const PAGE_SIZE = 10;

function App() {
  const [comments, setComments] = useState([]);
  const [paging, setPaging] = useState({ totalCount: 0, totalPages: 1 });
  const [page, setPage] = useState(1);
  const [sortBy, setSortBy] = useState('CreatedAt');
  const [sortDirection, setSortDirection] = useState('desc');
  const [isConnected, setIsConnected] = useState(false);
  const [error, setError] = useState(null);

  // Загружаем текущую страницу дерева комментариев с сервера.
  const loadComments = useCallback(async () => {
    try {
      const data = await CommentService.getComments({
        page,
        pageSize: PAGE_SIZE,
        sortBy,
        sortDirection,
      });
      setComments(data.items ?? []);
      setPaging({ totalCount: data.totalCount, totalPages: data.totalPages });
      if (data.page !== page) setPage(data.page); // сервер поправил номер страницы
      setError(null);
    } catch (err) {
      console.error('Failed to load comments:', err);
      setError('Не удалось загрузить комментарии');
    }
  }, [page, sortBy, sortDirection]);

  useEffect(() => {
    loadComments();
  }, [loadComments]);

  // SignalR-обработчики всегда вызывают актуальную версию loadComments.
  const loadRef = useRef(loadComments);
  useEffect(() => {
    loadRef.current = loadComments;
  }, [loadComments]);

  // Подключение к хабу. Любое событие (создание/правка/удаление) просто
  // перезагружает страницу: так вложенные ответы, сортировка и пагинация
  // всегда совпадают с сервером.
  useEffect(() => {
    let cancelled = false;
    const refresh = () => loadRef.current();

    const connection = CommentService.createConnection({
      onCreated: refresh,
      onUpdated: refresh,
      onDeleted: refresh,
      onStateChange: (connected) => {
        if (cancelled) return;
        setIsConnected(connected);
        if (connected) refresh(); // после переподключения подтягиваем пропущенное
      },
    });

    connection
      .start()
      .then(() => {
        if (!cancelled) setIsConnected(true);
      })
      .catch((err) => {
        if (!cancelled) {
          console.error('SignalR connection failed:', err);
          setIsConnected(false);
        }
      });

    return () => {
      cancelled = true;
      connection.stop();
    };
  }, []);

  // Обработчики пробрасывают ошибки наверх: форма/комментарий покажут их сами.
  const handleAddComment = async (data) => {
    await CommentService.createComment(data);
    await loadComments();
  };

  const handleEditComment = async (comment, newText) => {
    await CommentService.updateComment(comment.id, {
      // Бекенд отдаёт HTML-энкодированные значения и кодирует их снова при
      // сохранении, поэтому перед отправкой декодируем.
      userName: decodeHtml(comment.userName),
      email: decodeHtml(comment.email),
      text: newText,
      parentId: comment.parentId,
    });
    await loadComments();
  };

  const handleDeleteComment = async (commentId) => {
    await CommentService.deleteComment(commentId);
    await loadComments();
  };

  const handleSortChange = (newSortBy, newDirection) => {
    setSortBy(newSortBy);
    setSortDirection(newDirection);
    setPage(1);
  };

  return (
    <div className="App">
      <header className="App-header">
        <h1>💬 Система комментариев</h1>
        <p>Демонстрация Real-time комментариев с SignalR</p>
      </header>

      <main className="App-main">
        <ConnectionStatus isConnected={isConnected} hubUrl={HUB_URL} />

        {error && <div className="error-message">{error}</div>}

        <CommentForm onSubmit={handleAddComment} />

        <CommentList
          comments={comments}
          totalCount={paging.totalCount}
          page={page}
          totalPages={paging.totalPages}
          sortBy={sortBy}
          sortDirection={sortDirection}
          onSortChange={handleSortChange}
          onPageChange={setPage}
          onReply={handleAddComment}
          onEdit={handleEditComment}
          onDelete={handleDeleteComment}
        />
      </main>

      <footer className="App-footer">
        <p>© 2026 Система комментариев. Все права защищены.</p>
      </footer>
    </div>
  );
}

export default App;