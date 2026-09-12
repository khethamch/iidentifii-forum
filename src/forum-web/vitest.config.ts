// jsdom supplies a browser-like DOM for component tests; setup adds matchers and test cleanup.
import { defineConfig } from "vitest/config";
export default defineConfig({
  test: { environment: "jsdom", setupFiles: "./src/test-setup.ts" },
});
