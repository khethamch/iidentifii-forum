import { useEffect, useState } from "react";
import type { ReactNode } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { setToken } from "./api";
import type { User } from "./api";
import { Context } from "./auth-context";
export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [expired, setExpired] = useState(false);
  const cache = useQueryClient();
  // Register once per query client and remove the listener when this provider unmounts.
  useEffect(() => {
    const onExpired = () => {
      setToken(null);
      setUser(null);
      setExpired(true);
      // Expired credentials must not leave viewer-specific cached data available.
      cache.clear();
    };
    window.addEventListener("forum:expired", onExpired);
    return () => window.removeEventListener("forum:expired", onExpired);
  }, [cache]);
  const login = (token: string, next: User) => {
    setToken(token);
    setUser(next);
    setExpired(false);
    // A different account must not inherit the previous account's likedByMe values.
    cache.clear();
  };
  const logout = () => {
    setToken(null);
    setUser(null);
    setExpired(false);
    // Anonymous browsing starts with fresh results after logout.
    cache.clear();
  };
  return (
    <Context.Provider value={{ user, expired, login, logout }}>
      {children}
    </Context.Provider>
  );
}
