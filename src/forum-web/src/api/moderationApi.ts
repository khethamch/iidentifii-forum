import { api } from "../api";

export const flagPost = (id: string) =>
  api<void>(`/posts/${id}/tags`, { method: "POST" });
