# NexusITSM — ServiceDesk Enterprise

Enterprise IT Service Management platform built with Blazor Server (.NET 9) and PostgreSQL.

## Screenshots

Dark theme, pixel-perfect from custom mockup. IBM Plex Sans/Mono fonts.

## Features

### Core ITSM (16 modules)
- **Dashboard** — KPI live, ticket recenti, workload gruppi, categorie
- **Incidents** — Lista/Kanban, filtri, detail panel con timeline, CRUD completo, escalation
- **Problem Management** — Card grid, Root Cause Analysis, Known Error Database (KEDB)
- **Change Management** — RFC register, Risk Matrix 5x5, CAB calendar, approval workflow
- **Escalation Matrix** — Timeline visiva 48h, path escalation, regole configurabili
- **SLA Engine** — Policy management, compliance tracking, breach monitoring
- **Service Catalog** — 8 categorie servizio, wizard richiesta
- **User Portal** — Self-service wizard 3 step per utenti finali

### Asset & CMDB
- **CMDB / Assets** — 4 tab: Inventario, Discovery, Agent management, SNMP config
- **Email Grabber** — Configurazione IMAP, coda email, auto-conversione in ticket

### Analytics & Governance
- **Reports** — Grafici per status/priorità, top agents, export
- **Governance** — Compliance policies, audit findings, risk register
- **Integrations** — 8 connettori (AD, SMTP, Wazuh, Zabbix, vCenter, Slack, Jira, Azure AD)
- **Settings** — Configurazione globale, notifiche, SLA, gruppi

### Security & Auth
- ASP.NET Identity con login/registrazione
- 3 ruoli: Admin, Operator, User
- Session-based authentication

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Frontend | Blazor Server (.NET 9), Interactive Server rendering |
| Styling | Custom CSS from mockup (dark theme, no UI framework) |
| Backend | ASP.NET Core 9 |
| Database | PostgreSQL + Entity Framework Core 9 + Npgsql |
| Auth | ASP.NET Identity (local users) |
| Fonts | IBM Plex Sans + IBM Plex Mono |
| Testing | xUnit + EF Core InMemory |

## Quick Start

### Prerequisites
- .NET 9 SDK
- PostgreSQL 14+

### Setup
```bash
# Clone
git clone https://github.com/YOUR_USERNAME/NexusITSM.git
cd NexusITSM

# Configure database (edit connection string if needed)
# Default: Host=localhost;Database=nexus_itsm;Username=postgres;Password=postgres

# Create database and apply migrations
cd src/NexusITSM
dotnet ef database update

# Run
dotnet run
```

Open `http://localhost:5150`

### Default Login
| Email | Password | Role |
|-------|----------|------|
| admin@nexusitsm.local | Nexus2025! | Admin |
| marco.rossi@nexusitsm.local | Nexus2025! | Admin |
| sara.bianchi@nexusitsm.local | Nexus2025! | Operator |

### Run Tests
```bash
dotnet test
```

## Project Structure

```
NexusITSM/
├── src/NexusITSM/
│   ├── Components/
│   │   ├── Layout/          # MainLayout, Sidebar, Topbar, NavItem
│   │   ├── Shared/          # StatusBadge, PrioBadge, KpiCard, SlaBar, Modal, Toast
│   │   └── Pages/           # 16 module pages
│   ├── Data/                # AppDbContext, DbSeeder
│   ├── Models/
│   │   ├── Entities/        # AppUser, Ticket, Problem, Change, CI, SupportGroup
│   │   └── Enums/           # All enums
│   ├── Services/            # TicketService, DemoDataService, AuthEndpoints
│   └── wwwroot/css/         # nexus.css (from mockup)
├── tests/NexusITSM.Tests/   # xUnit tests (22 tests)
├── .mockup/                 # Original HTML mockup
├── ROADMAP.md               # Sprint tracking
└── CHANGELOG.md             # Release history
```

## License

Proprietary — Internal use only.
