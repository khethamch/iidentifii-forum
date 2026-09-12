import { useState } from "react";
import type { FormEvent } from "react";
import { useQuery } from "@tanstack/react-query";
import { getAuthors } from "../../api/postsApi";
import { ErrorMessage } from "../shared";
export function PostFilters({
  params,
  onApply,
}: {
  params: URLSearchParams;
  onApply: (p: URLSearchParams) => void;
}) {
  const authors = useQuery({
    queryKey: ["authors"],
    queryFn: getAuthors,
  });
  const [invalid, setInvalid] = useState<Error | null>(null);
  function apply(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    const values = new FormData(e.currentTarget);
    const from = String(values.get("from") || "");
    const to = String(values.get("to") || "");
    if (from && to && from > to) {
      setInvalid(new Error("From must be on or before To."));
      return;
    }
    setInvalid(null);
    const next = new URLSearchParams(params);

    next.delete("page");
    for (const [key, value] of values.entries()) {
      if (value) next.set(key, String(value));
      else next.delete(key);
    }
    onApply(next);
  }
  return (
    <aside>
      <form className="filters" onSubmit={apply} key={params.toString()}>
        <h2>Filters</h2>
        <label>
          From
          <input
            name="from"
            type="date"
            defaultValue={params.get("from") || ""}
          />
        </label>
        <label>
          To
          <input name="to" type="date" defaultValue={params.get("to") || ""} />
        </label>
        <label>
          Author
          <select name="author" defaultValue={params.get("author") || ""}>
            <option value="">All authors</option>
            {authors.data?.map((a) => (
              <option key={a.id} value={a.id}>
                {a.name}
              </option>
            ))}
          </select>
        </label>
        <label>
          Moderation
          <select name="tag" defaultValue={params.get("tag") || ""}>
            <option value="">All posts</option>
            <option value="flagged">Flagged posts</option>
            <option value="unflagged">Unflagged posts</option>
          </select>
        </label>
        <ErrorMessage error={invalid || authors.error} />
        <button className="primary">Apply filters</button>
        <button
          type="button"
          onClick={() => {
            setInvalid(null);
            onApply(new URLSearchParams());
          }}
        >
          Reset
        </button>
      </form>
    </aside>
  );
}
