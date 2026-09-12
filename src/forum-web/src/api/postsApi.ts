import { api, send } from "../api";
import type { Author, Page, Post } from "../api";

export const getAuthors = () => api<Author[]>("/authors");
export const getPosts = (query: URLSearchParams, signal?: AbortSignal) =>
  api<Page<Post>>(`/posts?${query}`, { signal });
export const getPost = (id: string) => api<Post>(`/posts/${id}`);
export const createPost = (values: Record<string, FormDataEntryValue>) =>
  api<{ id: number }>("/posts", send(values));
export const likePost = (id: string | number) =>
  api<void>(`/posts/${id}/likes`, { method: "POST" });
