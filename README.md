# Partner forum

A full-stack forum: ASP.NET Core REST API, React/TypeScript and SQL Server 2022.
Anonymous visitors browse; authenticated users post, comment and like; moderators
flag misleading or false information with an audit record.

## Run locally

Prerequisites: **.NET SDK 9.0.117 or later in the 9.0 line**, **Node.js 24 LTS**,
**npm**, Git, and **Docker Desktop** (or an accessible SQL Server instance).
Verify with `dotnet --version`, `node --version`, `npm --version`, `docker version`.

From the repository root:

```sh
docker compose up -d --wait
dotnet tool restore
dotnet restore src/backend/ForumApi.sln
npm --prefix src/forum-web ci
```

Terminal 1 (macOS/Linux):

```sh
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/backend/Forum.Api --no-launch-profile --urls http://localhost:5088
```

PowerShell equivalent:

```powershell
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run --project src/backend/Forum.Api --no-launch-profile --urls http://localhost:5088
```

Terminal 2:

```sh
npm --prefix src/forum-web run dev -- --host 127.0.0.1
```

Open **http://localhost:5173**. API: **http://localhost:5088/api/v1**.
OpenAPI JSON: **http://localhost:5088/openapi/v1.json**.
Vite proxies `/api` to the backend, keeping browser requests on the same origin.
If you change ports, update `vite.config.ts` and the Postman `baseUrl` together.

SQL Server runs on `localhost,14339` in a dedicated Docker container, with persistent
storage in a named volume. The public local-only database login is `sa` /
`LocalForum!2026Demo`; the port is bound to loopback. Development configuration is
included. For another server, set `ConnectionStrings__Forum` to its connection string.
Use separate credentials and a trusted certificate outside this local demo.

On Apple Silicon, the container uses AMD64 emulation. If Docker cannot run it, use
an external SQL Server instance and override the connection string.

The first Development startup applies the checked-in EF Core migrations and seeds
five fictional accounts, four discussions, comments, likes and a moderation tag.
Seeding is idempotent. `docker compose stop` preserves data; to deliberately reset
this project's disposable demo database, stop the API, run `docker compose down -v`,
then `docker compose up -d --wait` and restart the API.

Schema changes use `dotnet ef migrations add NAME --project src/backend/Forum.Infrastructure`.
For deployment, review an idempotent migration script before applying it:
`dotnet ef migrations script --idempotent --project src/backend/Forum.Infrastructure`.
Production startup does not migrate or seed the database automatically.

| Demo email | Role |
|---|---|
| alex@example.test | User |
| priya@example.test | User |
| marcus@example.test | User |
| taylor@example.test | User |
| moderator@example.test | Moderator |

All demo accounts use **ForumDemo!2026**. These are deliberately public, fictional
credentials. Real registration never accepts a requested role.

## Architecture

A **modular monolith with Clean Architecture**, using API controllers and a separate
React SPA. `src/backend/ForumApi.sln` contains four backend projects:

| Project | Responsibilities | Project dependencies |
|---|---|---|
| `Forum.Domain` | Plain entities, business rules, domain errors and role definitions | None |
| `Forum.Application` | DTOs, post/comment use cases, service and persistence interfaces | Domain |
| `Forum.Infrastructure` | EF Core repositories, `AppDbContext`, migrations, seeds, Identity-backed `User`, authentication and JWT implementations | Application |
| `Forum.Api` | Controllers, exception middleware, DI composition and API configuration | Application, Infrastructure |

Each project keeps focused folders and individual files. Application groups repository
interfaces under `Interfaces/Repositories/` and service interfaces under
`Interfaces/Services/`. Its use-case implementations live directly under `Services/`.
Infrastructure keeps database adapters in `Repositories/` and authentication/JWT
implementations directly under `Services/`. No EF context, Identity user, HTTP type
or JWT library crosses into Application or Domain. Infrastructure implements the
authentication and token interfaces owned by Application, using DTOs at that boundary.

Controllers use application service interfaces. `Program.cs` references Infrastructure
to register implementations and configure the host. This composition-root dependency
is intentional; business rules remain inside the inner layers. Project references
enforce the dependency direction.

Registration is grouped in `AddForumInfrastructure` and `AddForumApplication`.
The latter lives in the API composition root so Application needs no DI framework package.

The frontend uses feature API modules in `src/api/`, with focused post, comment and
moderation components in `src/components/`. Pages coordinate route state and mutations;
the shared HTTP client handles authentication and errors. React Query manages server
state, caching and invalidation. This keeps the app readable without introducing
command handlers or extra abstraction layers.

The frontend stays in `src/forum-web`; backend tests stay in the top-level `tests/` folder
and are included in the backend solution. Application unit tests use a recording
repository without a database; integration tests use the actual SQL Server adapters.

SQL Server aligns with the datastore choice named in the brief, has out-of-the-box
ASP.NET/EF Core integration, and lets integration tests exercise the real provider,
including filtering, pagination and concurrent writes. EF migrations version the
schema. Duplicate-key errors 2601 and 2627 map to HTTP 409; other database failures
remain server errors.

## Business and API semantics

All routes are under `/api/v1`. The Postman collection documents runnable examples.

| Method | Route | Access |
|---|---|---|
| POST | `/auth/register`, `/auth/login` | Public; rate limited |
| GET | `/auth/me` | Authenticated |
| GET | `/posts`, `/posts/{id}`, `/posts/{id}/comments`, `/authors` | Public |
| POST | `/posts`, `/posts/{id}/comments`, `/posts/{id}/likes` | Authenticated |
| POST | `/posts/{id}/tags` | Moderator |

Post filters: `author` (user ID), `tag=flagged|unflagged`, `from` (inclusive UTC),
`to` (exclusive UTC). The UI treats its To date as inclusive and sends the next
midnight. Sort: `newest`, `oldest`, `likes` (descending). Ties use ID for deterministic
ordering. Posts and comments have independent `page` and `pageSize` (1–50);
comments are chronological. Filters apply to posts; comments belong to the selected post.

Responses use `items`, `total`, `pageNumber`, `pageSize`. Queries apply filters,
ordering and paging in SQL; collection projection avoids per-row database requests.
Indexes cover post chronology/author, comment paging and moderation tags. Likes use
a composite primary key `(PostId, UserId)` so concurrent duplicate attempts cannot
persist twice. Self-likes and duplicate likes return 409. Duplicate flags return 409.
Moderation stores moderator identity and UTC time. Tags are visible anonymously.
Posts remain visible after flagging.

Validation and domain errors return Problem Details; 401/403 can have an empty body.
Unexpected errors return a trace ID without exposing exceptions to clients.

## Authentication and security

ASP.NET Core Identity hashes passwords and enforces a 12-character policy. Login
has a five-attempt account lockout and a per-IP rate limit. API endpoints take author
identity from validated claims and enforce roles server-side. No external auth provider.
Bearer JWTs validate signature, issuer, audience and expiration (30 minutes).

The React app stores tokens **only in memory**. Reloading requires logging in again.
Logout clears local credentials and cached data; a copied JWT remains valid until
expiry. In Development/Testing a random signing key is generated at startup, so
restarting the API invalidates existing tokens. Deployment requires a random `Jwt__Key`
of at least 32 characters and HTTPS; a production signing key is never checked into Git.

User content is rendered as text, input lengths are bounded, EF parameterizes
queries, and logs do not include request bodies or sensitive parameter values.
Demo accounts/seeding run only in Development/Testing.

## Known limitations

- **No refresh tokens or central token revocation.** A copied bearer token stays
  valid until its 30-minute expiry even after logout.
- **No password reset flow.**
- **Session does not survive a page reload**, since the token is kept in memory
  only and never written to browser storage.
- **Posts, comments and moderation tags cannot be edited, deleted or unflagged**
  once created — only creation and the fixed moderation flag are supported.
- **No 2FA or social login**, as the brief allows but does not require it.
- The production environment requires separate database provisioning and role
  setup; this project is built for local evaluation, not an unattended production
  deployment.

## Tests

```sh
dotnet test src/backend/ForumApi.sln
npm --prefix src/forum-web test
npm --prefix src/forum-web run build
npm --prefix src/forum-web run lint
```

Domain business-rule tests (`ForumRulesTests`) were written and run failing before
`ForumRules` was implemented; most other tests were written alongside their
implementation. The commit history reflects incremental work rather than a single
combined change.

Start SQL Server with `docker compose up -d --wait` before running backend tests.
Integration tests use a uniquely named real SQL Server database and an in-process
HTTP host. Each test fixture applies migrations and drops only its generated
`ForumTests_<guid>` database on disposal. Set `FORUM_TEST_SQLSERVER` to use another
server; the supplied database name is always replaced. The test login needs
permission to create and drop databases. Unit tests do not require SQL Server:
`dotnet test tests/Forum.UnitTests`.

They check auth boundaries, registration role escalation, posting, comments,
self-likes, concurrent duplicate likes, moderation audits, filtering and paging.
Integration tests are grouped by authentication, posts, comments, likes and
moderation; unit tests by Domain and Application. Authentication coverage also
verifies tampered/expired tokens, account lockout and request rate limiting.
Frontend tests cover like permissions, pagination, failed login/comment state
retention, filter validation, moderator controls and expired-session cache/token
clearing.

## Postman

Import `postman/Forum.postman_collection.json` and `postman/Local.postman_environment.json`
into Postman, then select **Forum Local**. The environment supplies only `baseUrl`;
login tokens and generated IDs remain collection variables. Each request includes a
description and an illustrative saved response (synthetic data, not captured credentials).
Start the Development API, then run the collection in order. Login captures tokens,
creation captures post IDs, and expected negative cases assert 401/403/409. Each run
creates fictional test data. It can also be run using Newman:

```sh
npx newman run postman/Forum.postman_collection.json -e postman/Local.postman_environment.json
```

See `docs/walkthrough.md` for the demo sequence, architecture explanations and
trade-offs, and `docs/code-reading-guide.md` for a tour of the source and how a
request flows through it.
