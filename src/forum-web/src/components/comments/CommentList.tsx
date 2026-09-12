import { date } from "../../api";
import type { Comment } from "../../api";

export function CommentList({ comments }: { comments: Comment[] }) {
  return comments.map((comment) => (
    <article className="comment" key={comment.id}>
      <p className="byline">
        <strong>{comment.author}</strong>
        <span>·</span>
        {date(comment.createdAt)}
      </p>
      <p>{comment.body}</p>
    </article>
  ));
}
