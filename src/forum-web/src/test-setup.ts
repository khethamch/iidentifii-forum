// Adds DOM matchers and cleans rendered trees between tests so component tests do not share mounted UI.
import "@testing-library/jest-dom/vitest";
import { cleanup } from "@testing-library/react";
import { afterEach } from "vitest";
afterEach(cleanup);
