import type { FormEvent } from "react";
import { ErrorMessage } from "../shared";

interface Props {
  body: string;
  onBodyChange: (value: string) => void;
  onSubmit: (event: FormEvent<HTMLFormElement>) => void;
  pending: boolean;
  error: Error | null;
}

export function CommentForm({
  body,
  onBodyChange,
  onSubmit,
  pending,
  error,
}: Props) {
  return (
    <form onSubmit={onSubmit}>
      <label>
        Add a comment
        <textarea
          required
          maxLength={3000}
          rows={4}
          value={body}
          onChange={(e) => onBodyChange(e.target.value)}
        />
      </label>
      <ErrorMessage error={error} />
      <button className="primary" disabled={pending}>
        {pending ? "Posting…" : "Post comment"}
      </button>
    </form>
  );
}
