# Application review and test report

Prepared 7 October 2026. This is an engineering test report, not a certification of hospital production readiness.

## Subsequent owner-requested database change

After the initial isolated test pass, the owner requested dummy data in the real database and deletion of the test database. A copy-only backup of `SAMI-UDDIN / AttendanceManagement` was created and passed SQL Server `RESTORE VERIFYONLY` with checksums. The two pending migrations (employee shift schedules and nullable department company) were applied. Five labelled demo employees, 20 punches, two schedules, and related sample master/workflow records were added under the existing company. Original company `Teknoloje`, employee `23455`, and company-admin credentials were preserved. Supervisor `234567` successfully authenticated through HTTP against the real database.

The former LocalDB `AttendanceManagement_Demo` database was backed up and verified, then dropped; the database listing confirmed zero remaining matches. The complete 145-check destructive QA suite was not rerun against the real database. Its results below describe the prior isolated test pass.

The locking Debug app process was stopped. The preview was published into a separate folder, and a Debug build succeeded while that preview was running, with zero warnings/errors. Database-specific authentication cookie and Data Protection identities prevent sessions from crossing between the old demo and real databases. Backup paths and current startup instructions are in `README.md`.

## Verified results

- 145/145 HTTP and SQL persistence checks passed against the running application and isolated SQL Server LocalDB demo database.
- 15/15 unit regression cases passed: salted admin hashes, legacy password compatibility, malformed hash rejection, overnight and unmatched punches, overtime duration, valid CSV parsing, and invalid CSV rejection.
- Existing migrations successfully created a fresh SQL Server demo database, which was populated with synthetic hospital data. The supplied database backup and configured original database were not restored, modified, or used as test fixtures.
- Browser login and visual inspection confirmed administrator navigation and the recorded attendance report, including day shifts and 20:00–04:00 overnight shifts.
- Release tests and publishing completed successfully with no compiler warnings. The local deployment bundle is in `artifacts/publish`.
- NuGet's known-vulnerability check reported no vulnerable direct or transitive packages for the web project using the configured sources. This does not assess browser/CDN assets or replace a security review.

## HTTP / persistence coverage

| Area | Verified |
| --- | --- |
| Authentication | Admin, employee, supervisor login; inactive employee rejection; anonymous protected-page redirect; password change, old-password rejection, new-password login, and restoration of the test credential |
| Access control | Employee blocked from management and SuperUser routes; CSRF rejection; group role grants view access; read-only role denies create; permission revocation takes effect immediately |
| Company | Index/create screens and create/edit/delete persistence |
| Lookups | Category, designation, department create/edit/delete |
| Employees | Index/create screens, create/edit/delete, referenced-employee deletion returns conflict without deleting the employee |
| Shifts | Index/create screens, create/edit/delete, future schedule creation, historical schedule protection, nonteam access rejection |
| Leave setup | Policy and leave create/edit/delete |
| Leave workflow | Submit, invalid type rejection, overlap rejection, approval/rejection status persistence, cancellation, prevention of repeat decisions on approved/cancelled requests |
| Groups/locations | Create/edit/delete; group assignment create/edit/delete |
| Group shifts | Future assignment preserves current shift and creates a schedule; nonmembers rejected |
| Calendar | Mark-day create/edit/delete; supervisor override persistence and employee visibility; invalid month rejection; other-team calendar access rejection |
| Supervisor pages | Team, employee view, leave requests, schedules, timings, leave sheet, calendar pages render |
| Employee pages | Calendar, password screen, leave application, balances, history, policy, timings, help pages render |
| Roles/modules | Role create/edit/delete, module create/edit/delete, group and individual role assignment/edit/deletion |
| Import/report/list | Pages render; regular employees denied; CSV upload and repeat upload succeed without duplicate punches |

## Changes delivered

- Removed the administrator role from employee login. Added request-level module permission enforcement and employee-company query/write restrictions for delegated access. Supervisor calendar access now checks team membership. Employee identity lookups use authenticated IDs and company claims rather than a name/email match.
- Enforced active-company employee login, active-account checks on protected routes, global antiforgery validation, a login request limit, and explicit access-denied responses.
- Replaced new company-admin SHA-256 hashes with salted PBKDF2. Existing legacy hashes are upgraded on successful login. Malformed employee password hashes fail verification safely.
- Removed embedded database credentials from production configuration and added a guarded, repeatable development seeder.
- Fixed leave decisions updating only supervisor status: the overall leave status now changes too. Pending leave reserves balance; invalid leave types and overlapping active requests are rejected. Generated references no longer depend on a race-prone count.
- Fixed overnight punch pairing, unmatched-entry handling, future group-shift scheduling, selected-group membership validation, and missing shift labels.
- Added calendar input checks, leap-day birthday handling, group-specific employee holidays, and display of saved calendar overrides.
- Added employee-number and related-company validation, marked-day date/status validation, cancellation reason validation, and controlled database-conflict responses.
- Added a transactional CSV attendance importer, duplicate prevention, a recorded-attendance report, and a working List route.
- Added repeatable regression tests and deployment documentation.

## Remaining limits and decisions

The hospital has not purchased its device and requested live device integration later. No claim is made that any device protocol, biometric device, real export format, or network connection has been tested. CSV import is an interim testing/export path.

The owner does not yet know the hospital's attendance and leave rules. The existing eight-hour extra-hours and four-hour half-day calculations remain baselines, not approved payroll policy. Annual leave renewal/carry-forward, partial-day leave, payroll periods, scheduled absences, holiday pay, and overtime approval require HR requirements and further implementation/validation. Leave-balance displays still derive from recorded requests and assigned entitlement; an annual entitlement ledger has not been implemented.

Company admins retain global administration as in the original design. Separate, mutually untrusted hospital administrators are not isolated tenants. Employee company restrictions were implemented, but a full multi-company adversarial security assessment was not performed.

Tests cover representative positive and negative workflows, not every field combination or every screen in every browser. Simultaneous conflicting writes, peak hospital load, actual-host deployment, backup restoration, full audit-history retention, accessibility, and comprehensive mobile/cross-browser behavior have not been certified.

The existing backup should receive a separate staging migration/data-quality rehearsal. Existing employee-number duplication, inconsistent historical leave statuses, and other legacy data are not automatically rewritten by these fixes. Historical data correction needs a reviewed reconciliation on that staging copy.

Before go-live: approve HR rules, verify staging migration and recovery, rotate old SQL credentials, configure production HTTPS/secrets/keys/timezone, and complete hospital user acceptance and deployment/load testing.

