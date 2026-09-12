interface Props {
  isModerator: boolean;
  flagged: boolean;
  pending: boolean;
  onFlag: () => void;
}

export function ModerationTagControl({
  isModerator,
  flagged,
  pending,
  onFlag,
}: Props) {
  if (!isModerator || flagged) return null;
  return (
    <button disabled={pending} onClick={onFlag}>
      {pending ? "Flagging…" : "Flag misleading or false information"}
    </button>
  );
}
