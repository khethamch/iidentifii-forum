// Reusable accessible error and paging controls; their parent owns fetching and page state.
import { ChevronLeft, ChevronRight } from "lucide-react";
export function ErrorMessage({ error }: { error: Error | null }) {
  return error ? (
    <p className="error" role="alert">
      {error.message}
    </p>
  ) : null;
}
export function Pagination({
  page,
  total,
  size,
  onChange,
}: {
  page: number;
  total: number;
  size: number;
  onChange: (page: number) => void;
}) {
  // Keep an empty result on page 1 of 1 rather than presenting a confusing page zero.
  const pages = Math.max(1, Math.ceil(total / size));
  return (
    <nav className="pagination" aria-label="Pagination">
      <button disabled={page <= 1} onClick={() => onChange(page - 1)}>
        <ChevronLeft size={17} />
        Previous
      </button>
      <span aria-live="polite">
        Page {page} of {pages}
      </span>
      <button disabled={page >= pages} onClick={() => onChange(page + 1)}>
        Next
        <ChevronRight size={17} />
      </button>
    </nav>
  );
}
