// Starts React and supplies shared query caching, routing and authentication to the component tree.
import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { AuthProvider } from "./auth";
import App from "./App";
import "./index.css";
// A short freshness window avoids repeated reads; failures are surfaced rather than automatically retried.
const queryClient = new QueryClient({
  defaultOptions: { queries: { retry: false, staleTime: 15000 } },
});
createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <AuthProvider>
          <App />
        </AuthProvider>
      </BrowserRouter>
    </QueryClientProvider>
  </StrictMode>,
);
