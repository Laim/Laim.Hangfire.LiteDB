# Laim.Hangfire.LiteDB

A maintained fork of [Hangfire.LiteDB](https://github.com/codeyu/Hangfire.LiteDB), providing LiteDB job storage for [Hangfire](https://www.hangfire.io/).

**NuGet package:** `Laim.Hangfire.LiteDB`

**Compatibility:** The assembly name and C# namespace remain `Hangfire.LiteDB`. Replace the old **package reference**, not your `using Hangfire.LiteDB;` statements or existing `UseLiteDbStorage(...)` calls.

## Dependencies and targets

| Component | Version / target |
| --- | --- |
| Library target | .NET Standard 2.0 (usable from .NET 10) |
| Hangfire.Core | 1.8.25 |
| LiteDB | 5.0.21 |
| Newtonsoft.Json | 13.0.4 |
| Sample applications and tests | .NET 10 |

This fork targets Hangfire 1.8 and removes the obsolete .NET Framework 4.5 / .NET Core 2.x build targets.

## Installation

Install the package from [NuGet.org](https://www.nuget.org/) using the .NET CLI:

```powershell
dotnet add package Laim.Hangfire.LiteDB
```

Or add the package reference directly to your project file:

```xml
<PackageReference Include="Laim.Hangfire.LiteDB" Version="1.0.0" />
```

If you currently use the original `Hangfire.LiteDB` package, remove it before installing `Laim.Hangfire.LiteDB`. **Do not reference both packages**, as they share the same assembly name.

No changes to your existing `using Hangfire.LiteDB;` statements or `UseLiteDbStorage(...)` calls are needed.

## Configure Hangfire

> [!TIP]
If this is your first time using Laim.Hangfire.LiteDB OR Hangfire.LiteDB in ASP.NET Core, I wrote a tutorial a while ago. [You can find it here](https://laim.scot/blog/hangfire-aspdotnet-core-8-and-10-sqlite)

In ASP.NET Core:

```csharp
using Hangfire;
using Hangfire.LiteDB;

builder.Services.AddHangfire(config =>
{
    config.UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseLiteDbStorage("app/hangfire.db"); // You can make this whatever you want and store the string however you want, i.e appsettings.json 
});

builder.Services.AddHangfireServer();
```

Enqueue and schedule jobs through Hangfire as usual:

```csharp
BackgroundJob.Enqueue(() => Console.WriteLine("Hello from Hangfire"));
BackgroundJob.Schedule(() => Console.WriteLine("Later"), TimeSpan.FromDays(1));


var recurringJobManager = app.ApplicationServices.GetRequiredService<IRecurringJobManager>();

// run the job every minute, if you don't understand cron you can use https://cronguru.com
// reconisiable-job-id shows up in the hangfire dashboard, so name it something you'll recognize such as
// weekly-db-backup, monthly-statistics-report, etc.
recurringJobManager.AddOrUpdate("recognizable-job-id", () => Test(), "* * * * *");
```

You can also enable the Hangfire Dashboard using the below, it is recommended to wrap this in an [Authentication](https://laim.scot/blog/hangfire-aspdotnet-core-8-and-10-sqlite#optional-hangfire-dashboard), as it is unauthenticated by default.

```csharp
// This is unauthenticated by default, anyone will be able to view and run jobs
//  if you do not put an AuthorizationFilter infront of it!
app.UseHangfireDashboard();
```

For console applications, explicitly instantiate `BackgroundJobServer` (see `samples/ConsoleSample`).
## Migration and operational considerations

- **Back up the existing LiteDB database** before switching packages. This fork retains the storage schema, assembly name, default table prefixes and public API, but a migration must be tested with your actual data and workload.

- Hangfire 1.8 changes some scheduling and serialization defaults. See the [official Hangfire 1.8 upgrade guide](https://docs.hangfire.io/en/latest/upgrade-guides/upgrading-to-hangfire-1.8.html). Avoid simultaneously enabling new 1.8 data-compatibility/serializer settings during the package swap; change them separately and test existing jobs.

- A Hangfire distributed-lock timeout must report the **lock resource**, not a formatted diagnostic string. The provider contains this required Hangfire 1.8 compatibility adjustment.

- LiteDB is a file-based store. Its performance, single-file concurrency and multi-process access characteristics are different from SQL Server/Redis. Upgrading dependencies does **not** guarantee that existing storage-provider issues (e.g., queue visibility, stuck locks or monitoring API coverage) are resolved. Test job processing, retries, recurring jobs, locks, dashboard queries and shutdown/restart behaviour before production rollout.

- **Do not run the old and new package in different server instances against the same database** until you have tested that exact deployment arrangement. Stop workers and take a backup during the migration.

## Build and test

Install the **.NET 10 SDK**.

```powershell
# Build all projects, including samples
dotnet restore Hangfire.LiteDB.sln
dotnet build Hangfire.LiteDB.sln --configuration Release --no-restore

# Run the existing test suite
dotnet test test/Hangfire.LiteDB.Test/Hangfire.LiteDB.Test.csproj --configuration Release --no-build

# Check known NuGet advisories (including transitive dependencies)
dotnet list Hangfire.LiteDB.sln package --vulnerable --include-transitive
```

## Attribution and license

Forked from [codeyu/Hangfire.LiteDB](https://github.com/codeyu/Hangfire.LiteDB) and distributed under the MIT License. See [`LICENSE`](LICENSE).
