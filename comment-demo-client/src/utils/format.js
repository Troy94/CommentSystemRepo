// Бекенд HTML-энкодит текст (&lt; &amp; ...). Для показа и для редактирования
// его нужно декодировать. Результат выводим как обычный текст (React экранирует
// сам), поэтому XSS невозможен, а при сохранении текст не кодируется дважды.
export function decodeHtml(value) {
  const el = document.createElement('textarea');
  el.innerHTML = value ?? '';
  return el.value;
}


export function formatDate(value) {
  if (!value) return '';
  const hasZone = /(Z|[+-]\d\d:?\d\d)$/.test(value);
  return new Date(hasZone ? value : `${value}Z`).toLocaleString();
}