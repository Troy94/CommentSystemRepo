import React, { useState, useId } from 'react';
import '../styles/CommentForm.css';

// Используется и для нового комментария, и для ответа (тогда передаётся parentId).
function CommentForm({
  onSubmit,
  parentId = null,
  onCancel,
  submitLabel = 'Отправить комментарий',
}) {
  const uid = useId();
  const [userName, setUserName] = useState('Пользователь');
  const [email, setEmail] = useState('');
  const [text, setText] = useState('');
  const [captchaToken, setCaptchaToken] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [formError, setFormError] = useState(null);

  const canSubmit = text.trim() && userName.trim() && email.trim() && !submitting;

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!canSubmit) return;

    setSubmitting(true);
    setFormError(null);
    try {
      await onSubmit({
        userName: userName.trim(),
        email: email.trim(),
        text: text.trim(),
        parentId,
        captchaToken: captchaToken.trim() || null,
      });
      setText('');
      setCaptchaToken('');
    } catch (err) {
      setFormError(err.message);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="comment-form">
      <div className="form-group">
        <label htmlFor={`${uid}-name`}>Имя:</label>
        <input
          id={`${uid}-name`}
          type="text"
          value={userName}
          onChange={(e) => setUserName(e.target.value)}
          placeholder="Ваше имя"
          maxLength={128}
          className="form-input"
        />
      </div>
      <div className="form-group">
        <label htmlFor={`${uid}-email`}>Email:</label>
        <input
          id={`${uid}-email`}
          type="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          placeholder="you@example.com"
          maxLength={320}
          className="form-input"
        />
      </div>
      <div className="form-group">
        <label htmlFor={`${uid}-text`}>Комментарий:</label>
        <textarea
          id={`${uid}-text`}
          value={text}
          onChange={(e) => setText(e.target.value)}
          placeholder="Напишите комментарий..."
          rows="4"
          maxLength={4000}
          className="form-textarea"
        />
      </div>
      {/* TODO: заменить на виджет  captcha,
          он должен вызывать setCaptchaToken(token). */}
      <div className="form-group">
        <label htmlFor={`${uid}-captcha`}>Captcha:</label>
        <input
          id={`${uid}-captcha`}
          type="text"
          value={captchaToken}
          onChange={(e) => setCaptchaToken(e.target.value)}
          placeholder="Токен captcha"
          className="form-input"
        />
      </div>

      {formError && <div className="error-message">{formError}</div>}

      <div className="form-actions">
        <button type="submit" disabled={!canSubmit} className="btn btn-primary">
          {submitting ? 'Отправка...' : submitLabel}
        </button>
        {onCancel && (
          <button type="button" onClick={onCancel} className="btn btn-cancel">
            Отмена
          </button>
        )}
      </div>
    </form>
  );
}

export default CommentForm;