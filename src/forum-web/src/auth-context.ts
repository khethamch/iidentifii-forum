import { createContext } from "react";
import type { User } from "./api";
interface Session {
  user: User | null;
  expired: boolean;
  login: (token: string, user: User) => void;
  logout: () => void;
}
export const Context = createContext<Session | null>(null);
