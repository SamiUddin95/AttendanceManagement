# Attendance Management

ASP.NET Core 10 MVC application with SQL Server and separate administrator, employee, and supervisor workflows.

## Current database and preview (7 October 2026)

At the owner's request, development now connects to `SAMI-UDDIN / AttendanceManagement` using Windows authentication. Sample data was added to the existing `Teknoloje` company. The original employee `23455` and existing company-admin credentials were preserved. The sample supervisor is `234567`; sample staff are `DEMO-001`, `DEMO-002`, and `DEMO-003`, with inactive account `DEMO-OFF`. Their test-only password is `DemoHospital!2026`.

The former LocalDB `AttendanceManagement_Demo` database was backed up, verified, and deleted. Its backup is `backups/AttendanceManagement_Demo_retired_20261007.bak`. A verified copy-only backup of the real database before this change is stored on SQL Server at `C:\Program Files\Microsoft SQL Server\MSSQL17.MSSQLSERVER\MSSQL\Backup\AttendanceManagement_before_samples_20261007.bak`.

The preview runs from `artifacts/local-preview`, so it does not lock the DLLs in the project's Debug build directory. For future previews, stop the existing preview and run `./scripts/Start-LocalPreview.ps1`. It publishes to a separate folder. When using Visual Studio debugging or `dotnet run` directly, stop that running app before rebuilding. Login sessions are bound to the configured server/database; switching databases requires login again.

Do not run the destructive synthetic-data workflow tests against the real database. The isolated-demo instructions below are optional instructions for recreating a separate QA database; they are no longer the current default configuration.

## Local demonstration

To deliberately recreate an isolated QA environment, override the connection as shown below. The original database was not touched during the initial test pass; the subsequent owner-requested sample-data change is described above. Isolated demo seeding refuses to run outside Development, outside LocalDB, or against a database whose name does not start with `AttendanceManagement_Demo`.

From this directory in PowerShell:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:ConnectionStrings__DefaultConnection = 'Server=(localdb)\MSSQLLocalDB;Database=AttendanceManagement_Demo;Integrated Security=true;Encrypt=false;TrustServerCertificate=true'
$env:Demo__Password = 'Choose-A-Local-Test-Password!'
dotnet restore
dotnet run --project src/AttendanceManagement.Web --no-launch-profile -- --seed-demo
dotnet run --project src/AttendanceManagement.Web --no-launch-profile --urls http://127.0.0.1:5088
```

Seeding applies the existing migrations and creates synthetic data only when the database contains no company. Repeating it does not reset passwords or overwrite records. Choose the test password before the first seed.

| Login | Identifier | Password |
| --- | --- | --- |
| Company admin | Company: `demo.hospital`; user: `demo.admin` | Password supplied during seeding |
| Supervisor | `DEMO-SUP` | Same demo password |
| Employee | `DEMO-001` | Same demo password |
| Night-shift employee | `DEMO-002` | Same demo password |
| Employee outside supervisor's team | `DEMO-003` | Same demo password |
| Inactive account | `DEMO-OFF` | Login must be rejected |

The retired demo database used the test-only password `DemoHospital!2026`. Never reuse this password for real staff accounts. Dummy data includes day/night shifts, staff, groups, a supervisor, leave policies, requests in multiple states, attendance punches, schedules, and calendar overrides.

## Repeatable tests

```powershell
dotnet test --logger 'trx;LogFileName=regression.trx' --results-directory tests/results
```

With the demo application running on port 5088, in a second PowerShell terminal:

```powershell
$env:Demo__Password = 'DemoHospital!2026' # Use the password actually used to seed your database.
./tests/Smoke.ps1
```

The HTTP suite requires PowerShell 7 and `sqlcmd`. It only accepts a loopback HTTP URL; database verification is fixed to `AttendanceManagement_Demo` on LocalDB. It writes and removes synthetic QA records, leaves demo workflow examples, and overwrites its own results. Do not change it to target a real database. SQL Server LocalDB runs under the current Windows user, so start the app and tests under the same account.

Results are written to `tests/results/smoke.json` and `tests/results/regression.trx`. The test sources and `TEST_REPORT.md` describe the coverage; generated results and published binaries are ignored by Git.

## Attendance import

Live device integration is deferred until the hospital chooses a device. The Process page currently supports CSV upload from a device export or a synthetic fixture:

```csv
EmployeeNo,PunchDateTime,PunchType,DeviceId
DEMO-001,2026-01-05 08:00:00,In,QA-DEVICE
DEMO-001,2026-01-05 16:00:00,Out,QA-DEVICE
```

Timestamps must be hospital local time in `yyyy-MM-dd HH:mm:ss` format. Direction must be explicit `In` or `Out`; the importer does not guess direction from alternating punches. Invalid rows or unknown/ambiguous employee numbers reject the entire import. Identical employee/time/direction combinations are skipped when reimported. Maximum: 1 MB and 10,000 rows.

The Report page shows recorded punches for a bounded date range, including overnight pairs. It does not determine absences on dates with no punches, payroll, payable overtime, or leave-renewal entitlement. The List entry opens the existing employee listing.

## Deployment preparation

1. Set `ASPNETCORE_ENVIRONMENT=Production` and supply `ConnectionStrings__DefaultConnection` through the hosting environment or a protected secret store. Production has no embedded connection string and fails to start without one. Rotate the SQL credentials previously present in source control; use a dedicated least-privilege database account rather than `sa`.
2. Use a staging copy of the hospital database to validate the existing migrations and data. Back up and verify a restore before changing the real database. Production startup does not automatically migrate or seed.
3. Apply reviewed migrations with the infrastructure project and web startup project, using a migration identity authorized to change the schema. Example, when the matching EF command-line tool is available:

   ```powershell
   dotnet ef database update --project src/AttendanceManagement.Infrastructure --startup-project src/AttendanceManagement.Web
   ```

4. Publish the web project with `dotnet publish src/AttendanceManagement.Web -c Release -o artifacts/publish`. Configure HTTPS, a restricted host name, appropriate reverse-proxy settings, persistent/protected ASP.NET Data Protection keys, backups, logging, and service restart/health monitoring. Verify logout, session persistence, and redirects on the actual host.
5. Configure the server's local timezone consistently with the hospital (Asia/Karachi). Existing date calculations and imported timestamps use local civil time; deployment in a different timezone needs explicit testing.
6. Confirm business rules with hospital HR: leave period/renewal and carry-forward, holiday and weekly-off treatment, supervisor authority, missing/duplicate punch handling, overtime entitlement, shift lengths, grace periods, and payroll cutoffs. The current eight-hour extra-hours and four-hour half-day baseline must not be interpreted as approved payroll policy.
7. Company admin accounts retain the application's original global company/role administration powers. Use only trusted central administrators. Independent hospitals requiring separately isolated administrators need a different administrator/tenant model before sharing one deployment.
8. Run hospital user acceptance tests with representative employees and supervisors, then load/concurrency, recovery, and deployment-specific security checks. Device integration and real device acceptance testing remain deferred at the owner's request.

See `TEST_REPORT.md` for what was actually verified and what remains outside the current validation scope.

