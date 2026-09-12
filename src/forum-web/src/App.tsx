// Defines page routes and the shared header. Page components handle their own data and forms.
import { Link, Route, Routes } from "react-router-dom";

export default function App() {
  return (
    <>
      <a className="skip" href="#main">
        Skip to content
      </a>
      <header>
        <div className="header-inner">
          <Link className="brand" to="/">
            Partner forum
          </Link>
          <Link className="nav-link" to="/">
            Discussions
          </Link>
        </div>
      </header>
      <main id="main">
        <Routes>
          <Route
            path="/"
            element={
              <div className="intro">
                <h1>Partner forum</h1>
                <p>Browsing, login and posting are coming in the next few commits.</p>
              </div>
            }
          />
          <Route
            path="*"
            element={
              <>
                <h1>Page not found</h1>
                <Link to="/">Return to discussions</Link>
              </>
            }
          />
        </Routes>
      </main>
    </>
  );
}
