import { Heart } from "lucide-react";
export function LikeButton({
  count,
  own,
  liked,
  authenticated,
  pending,
  onLike,
}: {
  count: number;
  own: boolean;
  liked: boolean;
  authenticated: boolean;
  pending?: boolean;
  onLike: () => void;
}) {
  const label = !authenticated
    ? "Log in to like"
    : own
      ? "You cannot like your own post"
      : liked
        ? "Already liked"
        : "Like post";
  return (
    <button
      className={`like ${liked ? "selected" : ""}`}
      type="button"
      disabled={own || liked || pending}
      onClick={onLike}
      title={label}
      aria-label={`${label} (${count} likes)`}
    >
      <Heart size={21} fill={liked ? "currentColor" : "none"} />
      <span>{count}</span>
    </button>
  );
}
