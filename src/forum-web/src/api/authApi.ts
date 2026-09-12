import { api, send } from "../api";
import type { User } from "../api";

export const registerUser = (values: Record<string, FormDataEntryValue>) =>
  api<{ id: string; displayName: string }>("/auth/register", send(values));
export const loginUser = (values: Record<string, FormDataEntryValue>) =>
  api<{ token: string; user: User }>("/auth/login", send(values));
