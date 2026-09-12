import { useState } from "react";
import type { FormEvent } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { date } from "../api";
import { getPost, likePost } from "../api/postsApi";
import { getComments, createComment } from "../api/commentsApi";
import { flagPost } from "../api/moderationApi";
import { CommentForm } from "../components/comments/CommentForm";
import { CommentList } from "../components/comments/CommentList";
import { ModerationTagControl } from "../components/moderation/ModerationTagControl";
import { useAuth } from "../useAuth";
import { LikeButton } from "../components/LikeButton";
import { ErrorMessage, Pagination } from "../components/shared";
export function PostDetail() {
  const { id = "" } = useParams();
  const { user } = useAuth();
  const cache = useQueryClient();
  const navigate = useNavigate();
  const [page, setPage] = useState(1);
  const [body, setBody] = useState("");
  const [notice, setNotice] = useState("");
  const post = useQuery({
    queryKey: ["post", id, user?.id],
    queryFn: () => getPost(id),
  });
  const comments = useQuery({
    queryKey: ["comments", id, page],
    queryFn: () => getComments(id, page),
  });
  // Counts and like/tag state appear on both detail and list pages; invalidate both after a mutation.
  const refresh = () => {
    cache.invalidateQueries({ queryKey: ["post", id] });
    cache.invalidateQueries({ queryKey: ["posts"] });
  };
  const like = useMutation({
    mutationFn: () => likePost(id),
    onSuccess: refresh,
  });
  const tag = useMutation({
    mutationFn: () => flagPost(id),
    onSuccess: () => {
      refresh();
      setNotice("Post flagged. Your moderation action has been recorded.");
    },
  });
  const comment = useMutation({
    mutationFn: () => createComment(id, body),
    onSuccess: () => {
      setBody("");
      setNotice("Comment added.");
      setPage(Math.ceil(((comments.data?.total || 0) + 1) / 10));
      cache.invalidateQueries({ queryKey: ["comments", id] });
      refresh();
    },
  });
  function submit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    comment.mutate();
  }
  if (post.isPending) return <p role="status">Loading discussion…</p>;
  if (post.error)
    return (
      <>
        <Link to="/">Back to discussions</Link>
        <ErrorMessage error={post.error} />
      </>
    );
  const p = post.data!;
  return (
    <section className="detail">
      <Link className="back" to="/">
        Back to discussions
      </Link>
      <h1>{p.title}</h1>
      <p className="byline">
        {p.author}
        <span>·</span>
        {date(p.createdAt)}
      </p>
      {p.tags.map((t) => (
        <div className="moderation" key={t.label}>
          <strong>{t.label}</strong>
          <p>
            Flagged by {t.moderator} on {date(t.createdAt)}.
          </p>
        </div>
      ))}
      <p className="post-body">{p.body}</p>
      <div className="actions">
        <LikeButton
          count={p.likeCount}
          own={user?.id === p.authorId}
          liked={p.likedByMe}
          authenticated={!!user}
          pending={like.isPending}
          onLike={() =>
            user ? like.mutate() : navigate(`/login?next=/posts/${id}`)
          }
        />
        <ModerationTagControl
          isModerator={!!user?.isModerator}
          flagged={p.tags.length > 0}
          pending={tag.isPending}
          onFlag={() => tag.mutate()}
        />
      </div>
      <ErrorMessage error={like.error || tag.error} />
      <p role="status" className="success">
        {notice}
      </p>
      <section className="comments">
        <h2>
          Comments{" "}
          <span className="muted">
            ({comments.data?.total ?? p.commentCount})
          </span>
        </h2>
        <ErrorMessage error={comments.error} />
        {comments.isPending && <p role="status">Loading comments…</p>}
        <CommentList comments={comments.data?.items ?? []} />
        {comments.data?.total === 0 && (
          <p className="muted">No comments yet. Share the first response.</p>
        )}
        {comments.data && comments.data.total > 10 && (
          <Pagination
            page={page}
            total={comments.data.total}
            size={10}
            onChange={setPage}
          />
        )}
        {user ? (
          <CommentForm
            body={body}
            onBodyChange={setBody}
            onSubmit={submit}
            pending={comment.isPending}
            error={comment.error}
          />
        ) : (
          <p className="sign-in-prompt">
            <Link to={`/login?next=/posts/${id}`}>Log in</Link> to join the
            conversation.
          </p>
        )}
      </section>
    </section>
  );
}
