import { api, send } from "../api";
import type { Comment, Page } from "../api";

export const getComments = (id: string, page: number) =>
  api<Page<Comment>>(`/posts/${id}/comments?page=${page}&pageSize=10`);
export const createComment = (id: string, body: string) =>
  api<{ id: number }>(`/posts/${id}/comments`, send({ body }));
