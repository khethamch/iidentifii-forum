import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { expect, it, vi } from "vitest";
import { ModerationTagControl } from "./ModerationTagControl";

it.each([
  [false, false],
  [true, true],
])("hides tagging for moderator=%s flagged=%s", (isModerator, flagged) => {
  render(
    <ModerationTagControl
      isModerator={isModerator}
      flagged={flagged}
      pending={false}
      onFlag={vi.fn()}
    />,
  );
  expect(screen.queryByRole("button")).not.toBeInTheDocument();
});
it("allows a moderator to submit a flag", async () => {
  const flag = vi.fn();
  render(
    <ModerationTagControl
      isModerator
      flagged={false}
      pending={false}
      onFlag={flag}
    />,
  );
  await userEvent.click(
    screen.getByRole("button", {
      name: "Flag misleading or false information",
    }),
  );
  expect(flag).toHaveBeenCalledOnce();
});
