import { useContext } from "react";
import { Context } from "./auth-context";
export function useAuth() {
  const auth = useContext(Context);
  if (!auth) throw new Error("AuthProvider is missing");
  return auth;
}