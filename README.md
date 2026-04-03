# CrewClock

A weekly shift scheduling app for restaurants built with ASP.NET Core MVC and SQLite.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Run the app

```bash
cd FourthAssignment
dotnet run
```

The app will start at `https://localhost:5001` (or the port shown in the console). The database is created and seeded automatically on first run.

## Run the tests

```bash
cd FourthAssignment.Tests
dotnet test
```

## Project structure

```
FourthAssignment/
├── Controllers/          # HTTP shell — routing, model state, redirects
│   ├── HomeController    # Weekly schedule view
│   └── ShiftsController  # Shift CRUD operations
├── Providers/            # Data access layer
│   ├── IShiftsProvider   # Interface
│   └── ShiftsProvider    # EF Core implementation
├── Validators/           # Business rules
│   └── ShiftValidator    # Time range, overlap, role checks
├── Models/               # One class per file
│   ├── Employee, Role, Shift        # Domain entities
│   └── *ViewModel                   # View models
├── Data/
│   ├── AppDbContext       # EF Core context
│   └── DbInitializer     # Seed data
└── Program.cs             # DI registration and startup

FourthAssignment.Tests/
├── ShiftsProviderTests    # Data access tests
├── ShiftValidatorTests    # Business rule tests
├── ShiftsControllerTests  # HTTP behavior tests
└── HomeControllerTests    # Home controller tests
```

## Database

SQLite file (`schedule.db`) created in the project root on first run. See `db-diagram.html` for the ER diagram — open it in a browser.

### Entities

- **Employee** — has a name and many roles
- **Role** — Waiter, Barman, Chef, Cleaner
- **Shift** — links an employee to a role on a specific date/time
- **EmployeeRoles** — many-to-many join table

## Business rules

1. Shift start time must be before end time
2. No overlapping shifts for the same employee on the same day
3. An employee can only work a shift for a role they hold
