import { useState } from "react";
import type { FormEvent } from "react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { loginUser, registerUser } from "../api/authApi";
import { useAuth } from "../useAuth";
import { ErrorMessage } from "../components/shared";
export function Login({ register = false }: { register?: boolean }) {
  const [error, setError] = useState<Error | null>(null);
  const [busy, setBusy] = useState(false);
  const auth = useAuth();
  const navigate = useNavigate();
  const [params] = useSearchParams();
  async function submit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setError(null);
    setBusy(true);
    const values = Object.fromEntries(new FormData(e.currentTarget));
    try {
      if (register) await registerUser(values);
      const result = await loginUser(values);
      auth.login(result.token, result.user);

      const next = params.get("next");
      navigate(next?.startsWith("/") && !next.startsWith("//") ? next : "/");

    } catch (e) {
      setError(e as Error);
    } finally {
      setBusy(false);
    }
  }
  return (
    <section className="form-page">
      <Link to="/" className="back">
        Back to discussions
      </Link>
      <h1>{register ? "Join the conversation" : "Welcome back"}</h1>
      <p className="muted">
        {register
          ? "Create your partner forum account."
          : "Log in to post, comment and share your knowledge."}
      </p>
      <form onSubmit={submit}>
        {register && (
          <label>
            Display name
            <input
              name="displayName"
              required
              maxLength={80}
              autoComplete="nickname"
            />
          </label>
        )}
        <label>
          Email
          <input
            name="email"
            type="email"
            required
            maxLength={254}
            autoComplete="email"
          />
        </label>
        <label>
          Password
          <input
            name="password"
            type="password"
            required
            minLength={register ? 12 : 1}
            autoComplete={register ? "new-password" : "current-password"}
          />
        </label>
        {register && (
          <p className="muted small">
            At least 12 characters, including uppercase, lowercase, a number and
            a symbol.
          </p>
        )}
        <ErrorMessage error={error} />
        <button className="primary" disabled={busy}>
          {busy ? "Please wait…" : register ? "Create account" : "Log in"}
        </button>
      </form>
      <p>
        {register ? "Already registered? " : "New to the forum? "}
        <Link to={register ? "/login" : "/register"}>
          {register ? "Log in" : "Create an account"}
        </Link>
      </p>
    </section>
  );
}
