# Internal Services Roadmap

## Product direction

- **Admin Portal:** A collection of small, useful tools for the Admin team.
- **Desktop Manager:** The restricted app for managing users, roles, permissions, and audit history.
- **InternalServices library:** Shared authentication, authorization, data, and administration logic.

The portal and desktop manager are separate user experiences. The desktop manager will eventually replace the portal's user/role administration pages; the portal remains a home for small internal tools.

## Stage 0 — Stabilize the authorization library

**Goal:** Make the existing authorization and audit behavior reliable before adding more applications.

- [ ] Fix SQL syntax in `AdminSql.InsertAudit` (`VALUES`, not `VALUE`).
- [ ] Fix `AdminSql.CountAdminAccessExcludingRole` to return a scalar count.
- [ ] Exercise the admin safeguards and audit writes against SQL Server, not only unit tests.
- [ ] Verify the last-active-admin and self-lockout rules for role removal, permission revocation, and account deactivation.
- [ ] Confirm failed database operations are shown clearly in the UI and do not appear successful.
- [ ] Keep unit tests for the shared admin-access decision logic.

**Done when:** The Admin Portal can perform and audit admin changes, and SQL-backed tests cover the guard queries and audit insert.

## Stage 1 — Build the desktop manager

**Goal:** Provide a dedicated interface for managing access, before removing those functions from the portal.

Recommended initial platform: **WPF on .NET 10**.

Build in this order:

1. [ ] Read-only user, role, tool, permission, and audit views.
2. [ ] Search and filtering for users and audit records.
3. [ ] User activation and role assignment/removal.
4. [ ] Role description and permission management.
5. [ ] Last-admin and self-lockout protections, with clear error messages.
6. [ ] Confirm each action records the correct actor in the audit log.

**Security requirements:**

- Use Windows Integrated Authentication for the SQL connection; do not embed a database username or password in the app.
- Grant database write rights only to the intended Windows/Active Directory group.
- Resolve the current Windows user to an `auth.Users` record for audit actor IDs; do not let the UI choose an arbitrary actor ID.
- Treat UI permission checks as usability controls, not a database security boundary. A user with direct SQL write access can bypass application checks.
- Restrict direct SQL access separately. If database-level guarantees are needed, enforce them in the database or through stored procedures.

**Done when:** The intended administrators can manage users, roles, permissions, and audit history in the desktop app, and changes are correctly protected and recorded.

## Stage 2 — Transition the portal to a tool launcher

**Goal:** Make the portal home the starting point for small Admin-team tools.

- [x] Add a configurable launcher catalog with categories, descriptions, internal routes, and required permissions.
- [x] Filter launcher cards using the signed-in user's permissions.
- [ ] Confirm every tool page independently enforces its required permission; hiding a card is not access control.
- [ ] Once the desktop manager is ready, remove or clearly deprecate the portal's user, role, tool, and audit administration pages.
- [ ] Keep a clear “no tools assigned” state for users without tool permissions.

**Done when:** The portal provides a permission-filtered launcher, and access to each tool is enforced at its page.

## Stage 3 — Choose and scope portal tools

Before coding each tool, record:

- Tool name and short description
- Who will use it
- The repetitive task or problem it solves
- Expected frequency and time saved
- Data it reads or changes
- Required permission name
- Route and category
- Owner and acceptance criteria

Prioritize small, low-risk tools that solve frequent Admin-team tasks. Build one tool at a time rather than adding placeholder cards.

**Done when:** Each planned tool has a clear user, purpose, permission, and measurable completion criteria.

## Stage 4 — Implement portal tools incrementally

For each approved tool:

1. [ ] Add its route, page, and catalog entry.
2. [ ] Add or reuse its permission under the `admin-portal` tool.
3. [ ] Protect the page itself with `[RequirePermission("...")]`.
4. [ ] Add validation and explicit error reporting for operations.
5. [ ] Add tests for important business rules.
6. [ ] Walk through the tool with an Admin-team user and refine the UI.

## Stage 5 — Package and deploy

Do this after the library and app boundaries settle:

- [ ] Split the library and portal into their intended solutions/projects.
- [ ] Define how the library is versioned and consumed.
- [ ] Configure production Entra sign-in with IT.
- [ ] Configure production SQL access and secrets securely.
- [ ] Add deployment and recovery instructions.
- [ ] Verify logging, audit retention, backups, and access review procedures.