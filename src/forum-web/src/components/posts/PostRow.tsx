import { Link, useNavigate } from "react-router-dom";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { MessageSquare } from "lucide-react";
import { date } from "../../api";
import type { Post } from "../../api";
import { likePost } from "../../api/postsApi";
import { useAuth } from "../../useAuth";
import { LikeButton } from "../LikeButton";
import { ErrorMessage } from "../shared";
export function PostRow({ post }: { post: Post }) {
  const { user } = useAuth();
  const navigate = useNavigate();
  const cache = useQueryClient();
  const like = useMutation({
    mutationFn: () => likePost(post.id),
    onSuccess: () => cache.invalidateQueries({ queryKey: ["posts"] }),
  });
  return (
    <article className="post-row">
      <div className="post-copy">
        <div className="post-title">
          <Link to={`/posts/${post.id}`}>{post.title}</Link>
          {post.tags.length > 0 && (
            <span className="tag">Misleading or false information</span>
          )}
        </div>
        <p className="preview">{post.body}</p>
        <p className="byline">
          {post.author}
          <span>·</span>
          {date(post.createdAt)}
        </p>
        <ErrorMessage error={like.error} />
      </div>
      <div className="post-stats">
        <LikeButton
          count={post.likeCount}
          own={user?.id === post.authorId}
          liked={post.likedByMe}
          authenticated={!!user}
          pending={like.isPending}
          onLike={() => (user ? like.mutate() : navigate("/login"))}
        />
        <Link
          to={`/posts/${post.id}`}
          aria-label={`${post.commentCount} comments`}
        >
          <MessageSquare size={21} />
          {post.commentCount}
        </Link>
      </div>
    </article>
  );
}
