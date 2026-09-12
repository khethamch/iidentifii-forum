# Assessment walkthrough

1. Start from the README on a clean checkout. Show anonymous browsing, author/date/
   moderation filtering, sorting, and the separate post detail/comments page.
2. Log in as Alex. Show that his own post cannot be liked. Like another author's
   post; the control disables and a second API attempt returns 409.
3. Create a discussion and comment. Explain that the API assigns author identity.
4. Log in as the moderator and flag a post. Show moderator and timestamp persisted.
   Log out and show that the warning remains visible to anonymous readers.
5. Run Postman independently of the frontend. Show regular-user moderation denied.
6. Run backend and frontend tests, particularly concurrent duplicate likes, expired
   sessions and role restrictions. Show that a failed form submission retains its draft.
7. Walk through API controllers → Application services → persistence interfaces.
   Show how Infrastructure implements those interfaces with EF Core and SQL Server,
   while Domain contains the rules. Explain the inward project dependencies and the
   API composition root, then demonstrate a service unit test without a database.

## Decisions to explain

- Relational constraints matter even if UI controls prevent duplicate actions.
- API permissions matter even when a moderator button is hidden.
- SQL Server matches the role; Docker supplies reproducible local setup and tests use the same provider.
- Pagination is executed in SQL. Count queries and response projection avoid loading full entity graphs.
- In-memory tokens avoid persistent browser token storage, with a login-on-reload trade-off.
- A moderation record is more useful for auditability than an unexplained boolean.
- Focused frontend components and feature API modules separate rendering from HTTP calls.
- Dependency registration helpers keep the API startup readable without adding dependencies to inner layers.
- Tests are grouped by feature, with each test describing one behaviour.
- Domain business rules (`ForumRules`) were written and run failing before being implemented;
  most other code was written alongside its tests rather than strictly test-first throughout.

## Limitations and next steps

No production hosting, refresh-token revocation, password recovery, email verification,
full-text search, editing/deletion, or moderation reversal. Checked-in EF migrations provide the
initial schema; production changes require reviewed migration scripts.
Rate limiting is per process and would need shared enforcement behind multiple instances.
Auth state is deliberately memory-only; a new browser page load requires login.
Pagination uses offsets and may shift if new posts are inserted between page requests.

## Verification

Verified locally: `dotnet test src/backend/ForumApi.sln` passes 44 backend tests
(11 unit, 33 integration against a real, disposable SQL Server database per test
fixture); `npm test` passes 16 frontend tests; `npm run build` and `npm run lint`
both complete cleanly with zero warnings. The live API was started against the
Docker SQL Server container and checked directly: the health endpoint, the posts
endpoint (returning real seeded data) and a moderator login (returning a valid JWT)
all responded correctly. Each frontend feature — anonymous browsing with filters,
sorting and paging; login/registration; post detail with comments, likes and
moderator tagging; and post creation — was also exercised manually in a browser
against the running API as it was built, not only through the automated test suite.
