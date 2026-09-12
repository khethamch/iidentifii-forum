import { render, screen, act } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { afterEach, expect, it, vi } from "vitest";
import { AuthProvider } from "./auth";
import { useAuth } from "./useAuth";
import { api, setToken } from "./api";

afterEach(() => {
  setToken(null);
  vi.unstubAllGlobals();
});
it("clears identity, token and cached private data after an authenticated 401", async () => {
  const cache = new QueryClient();
  function SessionProbe() {
    const auth = useAuth();
    return (
      <>
        <button
          onClick={() =>
            auth.login("expired-token", {
              id: "user",
              name: "Example",
              isModerator: false,
            })
          }
        >
          Sign in
        </button>
        <p>{auth.user?.name ?? "Anonymous"}</p>
        <p>{auth.expired ? "Expired" : "Active"}</p>
      </>
    );
  }
  render(
    <QueryClientProvider client={cache}>
      <AuthProvider>
        <SessionProbe />
      </AuthProvider>
    </QueryClientProvider>,
  );
  await userEvent.click(screen.getByRole("button", { name: "Sign in" }));
  cache.setQueryData(["private"], "private data");
  const fetch = vi
    .fn()
    .mockResolvedValueOnce(new Response('{"title":"Expired"}', { status: 401 }))
    .mockResolvedValueOnce(new Response("{}"));
  vi.stubGlobal("fetch", fetch);
  await act(async () => {
    await expect(api("/auth/me")).rejects.toThrow("Expired");
  });
  expect(screen.getByText("Anonymous")).toBeVisible();
  expect(screen.getByText("Expired")).toBeVisible();
  expect(cache.getQueryData(["private"])).toBeUndefined();
  await api("/posts");
  expect(fetch.mock.calls[1][1].headers.has("Authorization")).toBe(false);
});
