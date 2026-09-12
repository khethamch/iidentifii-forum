import { Link, useSearchParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { getPosts } from "../api/postsApi";
import { useAuth } from "../useAuth";
import { PostFilters } from "../components/posts/PostFilters";
import { PostRow } from "../components/posts/PostRow";
import { ErrorMessage, Pagination } from "../components/shared";
export function Discussions() {
  const [params, setParams] = useSearchParams();
  const { user } = useAuth();
  const query = new URLSearchParams(params);
  if (query.get("from")) query.set("from", `${query.get("from")}T00:00:00Z`);

  if (query.get("to")) {
    const end = new Date(`${query.get("to")}T00:00:00Z`);
    if (!Number.isNaN(end.valueOf())) {
      end.setUTCDate(end.getUTCDate() + 1);
      query.set("to", end.toISOString());
    }
  }
  query.set("pageSize", "10");
  const posts = useQuery({

    queryKey: ["posts", query.toString(), user?.id],
    queryFn: ({ signal }) => getPosts(query, signal),
  });
  function change(key: string, value: string) {
    const next = new URLSearchParams(params);
    next.set(key, value);
    // A changed sort/filter can shrink the result set, so return to the first page.
    if (key !== "page") next.delete("page");
    setParams(next);
  }
  return (
    <>
      <div className="intro">
        <h1>Integration discussions</h1>
        <p>Ask questions. Share knowledge. Build with confidence.</p>
      </div>
      <div className="forum-layout">
        <PostFilters params={params} onApply={setParams} />
        <section aria-label="Discussions">
          <div className="list-toolbar">
            <span aria-live="polite">
              {posts.data
                ? `${posts.data.total} ${posts.data.total === 1 ? "discussion" : "discussions"}`
                : "Loading discussions…"}
            </span>
            <label>
              Sort by
              <select
                aria-label="Sort by"
                value={params.get("sort") || "newest"}
                onChange={(e) => change("sort", e.target.value)}
              >
                <option value="newest">Newest first</option>
                <option value="oldest">Oldest first</option>
                <option value="likes">Most liked</option>
              </select>
            </label>
          </div>
          <ErrorMessage error={posts.error} />
          {posts.isError && (
            <button onClick={() => posts.refetch()}>Try again</button>
          )}
          {posts.isPending && (
            <p role="status" className="empty">
              Loading the latest discussions…
            </p>
          )}
          {posts.data?.items.map((post) => (
            <PostRow key={post.id} post={post} />
          ))}
          {posts.data?.items.length === 0 && (
            <div className="empty">
              <h2>No discussions found</h2>
              <p>Try adjusting your filters or start a new discussion.</p>
              <Link to="/new">Create a post</Link>
            </div>
          )}
          {posts.data && (
            <Pagination
              page={posts.data.pageNumber}
              total={posts.data.total}
              size={posts.data.pageSize}
              onChange={(page) => change("page", String(page))}
            />
          )}
        </section>
      </div>
    </>
  );
}
