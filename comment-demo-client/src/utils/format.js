
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