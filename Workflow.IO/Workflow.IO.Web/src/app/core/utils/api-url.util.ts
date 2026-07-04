/** Joins gateway base URL and path (`base` may be empty for dev proxy). */
export function apiUrl(base: string, path: string): string {
  if (!base) {
    return path.startsWith('/') ? path : `/${path}`;
  }

  const normalizedBase = base.replace(/\/$/, '');
  const normalizedPath = path.startsWith('/') ? path : `/${path}`;
  return `${normalizedBase}${normalizedPath}`;
}
