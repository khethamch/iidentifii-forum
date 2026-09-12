import { useState } from "react";
import type { FormEvent } from "react";
import { Link, Navigate, useNavigate } from "react-router-dom";
import { createPost } from "../api/postsApi";
import { useAuth } from "../useAuth";
import { ErrorMessage } from "../components/shared";
export function CreatePost() {
  const { user } = useAuth();
  const navigate = useNavigate();
  const [error, setError] = useState<Error | null>(null);
  const [busy, setBusy] = useState(false);
  if (!user) return <Navigate to="/login?next=/new" replace />;
  async function submit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setError(null);
    setBusy(true);
    try {
      const result = await createPost(
        Object.fromEntries(new FormData(e.currentTarget)),
      );
      navigate(`/posts/${result.id}`);
    } catch (e) {
      setError(e as Error);
    } finally {
      setBusy(false);
    }
  }
  return (
    <section className="form-page wide">
      <Link to="/" className="back">
        Back to discussions
      </Link>
      <h1>Start a discussion</h1>
      <p className="muted">
        Share a clear question and enough context for others to help. Keep
        credentials and personal information out of your post.
      </p>
      <form onSubmit={submit}>
        <label>
          Title
          <input name="title" required maxLength={180} />
        </label>
        <label>
          Your question
          <textarea name="body" required maxLength={10000} rows={9} />
        </label>
        <ErrorMessage error={error} />
        <button className="primary" disabled={busy}>
          {busy ? "Publishing…" : "Publish post"}
        </button>
      </form>
    </section>
  );
}
