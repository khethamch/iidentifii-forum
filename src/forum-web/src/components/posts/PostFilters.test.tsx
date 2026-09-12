import { render, screen } from "@testing-library/react";
import { fireEvent } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { afterEach, expect, it, vi } from "vitest";
import { PostFilters } from "./PostFilters";

afterEach(() => vi.unstubAllGlobals());
function setup() {
  vi.stubGlobal("fetch", vi.fn().mockResolvedValue(new Response("[]")));
  const apply = vi.fn();
  render(
    <QueryClientProvider client={new QueryClient()}>
      <PostFilters
        params={new URLSearchParams("page=3&sort=likes")}
        onApply={apply}
      />
    </QueryClientProvider>,
  );
  return apply;
}
it("rejects a reversed date range before applying filters", async () => {
  const apply = setup();
  fireEvent.change(screen.getByLabelText("From"), {
    target: { value: "2026-09-20" },
  });
  fireEvent.change(screen.getByLabelText("To"), {
    target: { value: "2026-09-10" },
  });
  await userEvent.click(screen.getByRole("button", { name: "Apply filters" }));
  expect(screen.getByRole("alert")).toHaveTextContent(
    "From must be on or before To.",
  );
  expect(apply).not.toHaveBeenCalled();
});
it("resets paging when applying a moderation filter and preserves sorting", async () => {
  const apply = setup();
  await userEvent.selectOptions(screen.getByLabelText("Moderation"), "flagged");
  await userEvent.click(screen.getByRole("button", { name: "Apply filters" }));
  const query = apply.mock.calls[0][0] as URLSearchParams;
  expect(query.get("tag")).toBe("flagged");
  expect(query.get("sort")).toBe("likes");
  expect(query.has("page")).toBe(false);
});
