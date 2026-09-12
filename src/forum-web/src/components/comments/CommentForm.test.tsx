import { useState } from "react";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { expect, it, vi } from "vitest";
import { CommentForm } from "./CommentForm";

it("keeps the comment draft visible when submission fails", async () => {
  const submit = vi.fn();
  function Harness() {
    const [body, setBody] = useState("");
    const [error, setError] = useState<Error | null>(null);
    return (
      <CommentForm
        body={body}
        onBodyChange={setBody}
        pending={false}
        error={error}
        onSubmit={(event) => {
          event.preventDefault();
          submit(body);
          setError(new Error("Unable to save comment."));
        }}
      />
    );
  }
  render(<Harness />);
  await userEvent.type(
    screen.getByLabelText("Add a comment"),
    "Keep this draft",
  );
  await userEvent.click(screen.getByRole("button", { name: "Post comment" }));
  expect(submit).toHaveBeenCalledWith("Keep this draft");
  expect(screen.getByRole("alert")).toHaveTextContent(
    "Unable to save comment.",
  );
  expect(screen.getByLabelText("Add a comment")).toHaveValue("Keep this draft");
});

it("disables submission while a comment is being saved", () => {
  render(
    <CommentForm
      body="Draft"
      onBodyChange={vi.fn()}
      onSubmit={vi.fn()}
      pending
      error={null}
    />,
  );
  expect(screen.getByRole("button", { name: "Posting…" })).toBeDisabled();
});
