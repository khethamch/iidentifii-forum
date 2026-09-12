import { Link, Route, Routes } from "react-router-dom";
import { useAuth } from "./useAuth";
import { Discussions } from "./pages/Discussions";
import { Login } from "./pages/Login";

export default function App() {
  const { user, expired, logout } = useAuth();
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
          <div className="account">
            {user ? (
              <>
                <span className="user-name">
                  {user.name}
                  {user.isModerator ? " · Moderator" : ""}
                </span>
                <button onClick={logout}>Log out</button>
              </>
            ) : (
              <Link className="button" to="/login">
                Log in
              </Link>
            )}
          </div>
        </div>
      </header>
      <main id="main">
        {expired && (
          <p className="error" role="alert">
            Your session expired. Please log in again.
          </p>
        )}
        <Routes>
          <Route path="/" element={<Discussions />} />
          <Route path="/login" element={<Login key="login" />} />
          <Route path="/register" element={<Login key="register" register />} />
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
