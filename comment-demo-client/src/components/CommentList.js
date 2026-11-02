import React, { useState } from 'react';
import CommentForm from './CommentForm';
import { decodeHtml, formatDate } from '../utils/format';
import '../styles/CommentList.css';
import '../styles/CommentTree.css';

function CommentItem({ comment, onReply, onEdit, onDelete }) {
  const [isEditing, setIsEditing] = useState(false);
  const [isReplying, setIsReplying] = useState(false);
  const [editText, setEditText] = useState('');
  const [actionError, setActionError] = useState(null);

  const run = async (action) => {
    setActionError(null);
    try {
      await action();
      return true;
    } catch (err) {
      setActionError(err.message);
      return false;
    }
  };

  const startEdit = () => {
    setEditText(decodeHtml(comment.text));
    setIsEditing(true);
  };

  const saveEdit = async () => {
    if (!editText.trim()) return;
    if (await run(() => onEdit(comment, editText.trim()))) {
      setIsEditing(false);
    }
  };

  const remove = () => {
    const hasReplies = comment.replies?.length > 0;
    const question = hasReplies
      ? 'Удалить комментарий вместе со всеми ответами?'
      : 'Удалить комментарий?';
    if (window.confirm(question)) {
      run(() => onDelete(comment.id));
    }
  };

  const handleReply = async (data) => {
    await onReply(data);
    setIsReplying(false);
  };

  return (
    <li className="comment-item">
      {isEditing ? (
        <div className="comment-edit">
          <textarea
            value={editText}
            onChange={(e) => setEditText(e.target.value)}
            maxLength={4000}
            className="edit-input"
          />
          <div className="edit-buttons">
            <button onClick={saveEdit} className="btn btn-save">Сохранить</button>
            <button onClick={() => setIsEditing(false)} className="btn btn-cancel">Отмена</button>
          </div>
        </div>
      ) : (
        <div className="comment-content">
          <div className="comment-header">
            <strong>{decodeHtml(comment.userName)}</strong>
            <span className="comment-date">{formatDate(comment.createdAt)}</span>
          </div>
          <p className="comment-text">{decodeHtml(comment.text)}</p>
          <div className="comment-actions">
            <button onClick={() => setIsReplying((v) => !v)} className="btn btn-reply">
              Ответить
            </button>
            <button onClick={startEdit} className="btn btn-edit">Редактировать</button>
            <button onClick={remove} className="btn btn-delete">Удалить</button>
          </div>
        </div>
      )}

      {actionError && <div className="error-message">{actionError}</div>}

      {isReplying && (
        <div className="reply-form">
          <CommentForm
            parentId={comment.id}
            onSubmit={handleReply}
            onCancel={() => setIsReplying(false)}
            submitLabel="Отправить ответ"
          />
        </div>
      )}

      {comment.replies?.length > 0 && (
        <ul className="comment-replies">
          {comment.replies.map((reply) => (
            <CommentItem
              key={reply.id}
              comment={reply}
              onReply={onReply}
              onEdit={onEdit}
              onDelete={onDelete}
            />
          ))}
        </ul>
      )}
    </li>
  );
}

function CommentList({
  comments,
  totalCount,
  page,
  totalPages,
  sortBy,
  sortDirection,
  onSortChange,
  onPageChange,
  onReply,
  onEdit,
  onDelete,
}) {
  return (
    <div className="comment-list">
      <h2>Комментарии ({totalCount})</h2>

      <div className="sort-bar">
        <label>
          Сортировка:{' '}
          <select value={sortBy} onChange={(e) => onSortChange(e.target.value, sortDirection)}>
            <option value="CreatedAt">По дате</option>
            <option value="UserName">По имени</option>
            <option value="Email">По email</option>
          </select>
        </label>
        <button
          className="btn btn-cancel"
          onClick={() => onSortChange(sortBy, sortDirection === 'asc' ? 'desc' : 'asc')}
        >
          {sortDirection === 'asc' ? '↑ По возрастанию' : '↓ По убыванию'}
        </button>
      </div>

      {comments.length === 0 ? (
        <p className="no-comments">Нет комментариев</p>
      ) : (
        <ul>
          {comments.map((comment) => (
            <CommentItem
              key={comment.id}
              comment={comment}
              onReply={onReply}
              onEdit={onEdit}
              onDelete={onDelete}
            />
          ))}
        </ul>
      )}

      {totalPages > 1 && (
        <div className="pagination">
          <button className="btn btn-cancel" disabled={page <= 1} onClick={() => onPageChange(page - 1)}>
            Назад
          </button>
          <span>Страница {page} из {totalPages}</span>
          <button className="btn btn-cancel" disabled={page >= totalPages} onClick={() => onPageChange(page + 1)}>
            Вперёд
          </button>
        </div>
      )}
    </div>
  );
}

export default CommentList;