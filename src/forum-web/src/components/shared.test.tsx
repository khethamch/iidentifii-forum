import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { expect, it, vi } from "vitest";
import { Pagination } from "./shared";
it("Given first page, When next is selected, Then request page two", async () => {
  const change = vi.fn();
  render(<Pagination page={1} total={21} size={10} onChange={change} />);
  expect(screen.getByRole("button", { name: "Previous" })).toBeDisabled();
  await userEvent.click(screen.getByRole("button", { name: "Next" }));
  expect(change).toHaveBeenCalledWith(2);
});
it("Given last page, Then next is unavailable", () => {
  render(<Pagination page={3} total={21} size={10} onChange={vi.fn()} />);
  expect(screen.getByRole("button", { name: "Next" })).toBeDisabled();
});
