# NexusITSM — ServiceDesk Enterprise

## Quick start
```bash
cd src/NexusITSM
dotnet build
dotnet run
```

## Stack
- Blazor Server .NET 9 (Interactive Server)
- PostgreSQL + EF Core 9 + Npgsql
- ASP.NET Identity (local users)
- CSS custom dal mockup (NO component library esterne)

## Project structure
```
src/NexusITSM/
├── Components/
│   ├── Layout/          # MainLayout, Sidebar, Topbar, NavItem
│   ├── Shared/          # StatusBadge, PrioBadge, KpiCard, SlaBar, ToastContainer
│   └── Pages/           # Una cartella per modulo (Dashboard, Incidents, etc.)
├── Data/                # AppDbContext
├── Models/
│   ├── Entities/        # AppUser, Ticket, Problem, Change, CI, SupportGroup, TimelineEvent
│   └── Enums/           # Tutti gli enum
├── Services/            # Business logic services
└── wwwroot/css/         # nexus.css (estratto dal mockup)
```

## Design rules
- Il CSS viene dal mockup `.mockup/nexus-itsm-mockup.html` — NON modificare lo stile
- Usare le classi CSS del mockup: `.btn`, `.card`, `.kpi`, `.tbl`, `.status`, `.prio`, etc.
- Il capo vuole risultato pixel-perfect rispetto al mockup
- Fonts: IBM Plex Sans + IBM Plex Mono (da Google Fonts)
- Theme: dark, variabili CSS custom (--bg, --acc, --t1, etc.)

## Conventions
- Entity IDs: string (es. "INC-1001", "PRB-001", "CHG-001", "CI-001")
- Enum per stati/priorità/tipi
- PostgreSQL jsonb per campi lista (Tags, Categories, Services)
- Italiano per UI labels, inglese per codice

## DB connection
Default in appsettings.json: `Host=localhost;Database=nexus_itsm;Username=postgres;Password=postgres`
