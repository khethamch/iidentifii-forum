import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi } from "vitest";
import { LikeButton } from "./LikeButton";
describe("Like behaviour", () => {
  it("Given my own post, Then liking is unavailable", () => {
    render(
      <LikeButton count={2} own liked={false} authenticated onLike={vi.fn()} />,
    );
    expect(screen.getByRole("button")).toBeDisabled();
    expect(screen.getByRole("button")).toHaveAccessibleName(/own post/i);
  });
  it("Given a liked post, Then another like is unavailable", () => {
    render(
      <LikeButton count={2} own={false} liked authenticated onLike={vi.fn()} />,
    );
    expect(screen.getByRole("button")).toBeDisabled();
  });
  it("Given another author, When I like, Then submit once", async () => {
    const like = vi.fn();
    render(
      <LikeButton
        count={2}
        own={false}
        liked={false}
        authenticated
        onLike={like}
      />,
    );
    await userEvent.click(screen.getByRole("button"));
    expect(like).toHaveBeenCalledOnce();
  });
  it("Given anonymous viewing, Then explain login is required", () => {
    render(
      <LikeButton
        count={2}
        own={false}
        liked={false}
        authenticated={false}
        onLike={vi.fn()}
      />,
    );
    expect(screen.getByRole("button")).toHaveAccessibleName(/log in/i);
  });
});
