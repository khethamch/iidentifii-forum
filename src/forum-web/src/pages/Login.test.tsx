import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, describe, expect, it, vi } from "vitest";
import { MemoryRouter } from "react-router-dom";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { AuthProvider } from "../auth";
import { setToken } from "../api";
import { Login } from "./Login";
afterEach(() => {
  vi.unstubAllGlobals();
  setToken(null);
});
function renderLogin(register = false) {
  render(
    <QueryClientProvider client={new QueryClient()}>
      <MemoryRouter>
        <AuthProvider>
          <Login register={register} />
        </AuthProvider>
      </MemoryRouter>
    </QueryClientProvider>,
  );
}
describe("Authentication form behaviour", () => {
  it("Given invalid credentials, Then show an error and preserve the form", async () => {
    vi.stubGlobal(
      "fetch",
      vi
        .fn()
        .mockResolvedValue(
          new Response(JSON.stringify({ title: "Invalid credentials." }), {
            status: 401,
          }),
        ),
    );
    renderLogin();
    await userEvent.type(
      screen.getByLabelText("Email"),
      "partner@example.test",
    );
    await userEvent.type(screen.getByLabelText("Password"), "WrongPassword!");
    await userEvent.click(screen.getByRole("button", { name: "Log in" }));
    expect(await screen.findByRole("alert")).toHaveTextContent(
      "Invalid credentials.",
    );
    expect(screen.getByLabelText("Email")).toHaveValue("partner@example.test");
    expect(screen.getByRole("button", { name: "Log in" })).toBeEnabled();
  });
  it("Given registration, Then explain the password requirements", () => {
    renderLogin(true);
    expect(screen.getByLabelText("Display name")).toBeRequired();
    expect(screen.getByLabelText("Password")).toHaveAttribute(
      "minlength",
      "12",
    );
    expect(screen.getByText(/At least 12 characters/)).toBeVisible();
  });
});
