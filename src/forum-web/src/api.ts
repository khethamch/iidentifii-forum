// Shared HTTP client and response shapes. Feature API modules reuse it for token headers and error handling.
export interface User {
  id: string;
  name: string;
  isModerator: boolean;
}
export interface Tag {
  label: string;
  moderator: string;
  createdAt: string;
}
export interface Post {
  id: number;
  authorId: string;
  author: string;
  title: string;
  body: string;
  createdAt: string;
  likeCount: number;
  commentCount: number;
  likedByMe: boolean;
  tags: Tag[];
}
export interface Comment {
  id: number;
  author: string;
  body: string;
  createdAt: string;
}
export interface Page<T> {
  items: T[];
  total: number;
  pageNumber: number;
  pageSize: number;
}
export interface Author {
  id: string;
  name: string;
}
// Memory-only by design: a full page reload requires login and no token is written to browser storage.
let accessToken: string | null = null;
export function setToken(value: string | null) {
  accessToken = value;
}
export class ApiError extends Error {
  status: number;
  constructor(message: string, status: number) {
    super(message);
    this.status = status;
  }
}
export async function api<T>(
  path: string,
  options: RequestInit = {},
): Promise<T> {
  const headers = new Headers(options.headers);
  if (options.body) headers.set("Content-Type", "application/json");
  if (accessToken) headers.set("Authorization", `Bearer ${accessToken}`);
  const response = await fetch(`/api/v1${path}`, { ...options, headers });
  if (!response.ok) {
    const error = await response.json().catch(() => ({}));
    // Notify React about an expired authenticated session without importing React into the HTTP helper.
    if (response.status === 401 && accessToken)
      window.dispatchEvent(new Event("forum:expired"));
    const detail = error.errors
      ? Object.values(error.errors).flat().join(" ")
      : error.title;
    throw new ApiError(
      detail || `Request failed (${response.status}). Please try again.`,
      response.status,
    );
  }
  // A successful like/flag has no JSON body, so do not try to parse one.
  return response.status === 204 ? (undefined as T) : response.json();
}
export const send = (body: unknown): RequestInit => ({
  method: "POST",
  body: JSON.stringify(body),
});
// SQL timestamps in this API represent UTC even when the serialized value has no trailing Z.
export const date = (value: string) =>
  new Date(value.endsWith("Z") ? value : `${value}Z`).toLocaleDateString(
    undefined,
    { year: "numeric", month: "short", day: "numeric" },
  );
