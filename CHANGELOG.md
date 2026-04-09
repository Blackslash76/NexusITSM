# Changelog

All notable changes to NexusITSM will be documented in this file.

## [0.6.0] - 2026-04-09

### Added — Sprint 6: Real-time & Background
- SLA Background Service: recalculates SLA percentages every minute for all active tickets
- Automatic SLA breach detection with SignalR push notifications
- SignalR NotificationHub at `/hubs/notifications`
- NotificationService with typed methods (ticket created, escalated, resolved, SLA breach)
- Real-time toast notifications: listen on SignalR, clickable (navigates to incidents)
- Dashboard auto-refresh: reloads data when SLA updates or ticket events arrive via SignalR
- TicketService now sends push notifications on create/escalate/resolve

### Changed — Sprint 5 completion
- Dashboard refactored from DemoDataService to PostgreSQL via DbContext
- Incidents page refactored to use TicketService with full DB persistence
- All 15 protected pages now require `[Authorize]`
- Login/Register accessible without authentication

## [0.5.0] - 2026-04-09

### Added — Sprint 5: Persistenza & Auth
- PostgreSQL database with EF Core migrations
- Full seed data: 12 tickets, 6 groups, 7 users, 2 problems, 4 changes, 10 CIs, timeline events
- Login page (`/login`) with ASP.NET Identity, dark theme
- Registration page (`/register`) with auto-role assignment
- Auth endpoints: POST login, POST register, GET logout
- 3 roles: Admin, Operator, User
- Sidebar shows logged-in user (name, role, logout button)
- TicketService with full DB operations (CRUD, escalate, notes, stats)
- xUnit test project with 22 tests (entities, DbContext, TicketService)
- Solution file with src + tests projects
- .gitignore, README.md, CHANGELOG.md

## [0.4.0] - 2026-04-09

### Added — Sprint 4: Advanced Modules
- CMDB / Assets — 4 tabs: Inventory (10 CIs), Discovery, Agent management, SNMP config
- SLA Engine — KPI dashboard, 4 policies, active tracker
- Email Grabber — IMAP config, email queue, convert-to-ticket, parsing rules
- Reports — CSS bar charts by status/priority, top agents table, export button
- Settings — 4 tabs: General, Notifications, SLA, Groups
- Integrations — 8 connectors (AD, SMTP, Wazuh, Zabbix, vCenter, Slack, Jira, Azure AD), log, API keys
- Governance — Compliance policies, audit findings, risk register

## [0.3.0] - 2026-04-09

### Added — Sprint 3: Process Modules
- Problem Management — Card grid, RCA, KEDB, detail modal with 4 tabs
- Change Management — RFC register, 5x5 Risk Matrix, CAB calendar, detail modal with 4 tabs
- Escalation Matrix — 48h visual timeline, escalation path, configurable rules, statistics
- Service Catalog — 8 service categories, click-to-create-ticket
- User Portal — Hero section, 3-step wizard, personal ticket sidebar

## [0.2.0] - 2026-04-09

### Added — Sprint 2: Core ITSM
- Incidents list with filters (status, priority, search)
- Kanban view with 5 status columns
- Ticket detail panel with SLA box, assignee, timeline
- Full CRUD: create, assign, escalate, resolve, close, add notes
- Bulk actions
- New ticket modal with complete form
- DemoDataService with 12 realistic tickets, 6 agents, 6 groups

## [0.1.0] - 2026-04-09

### Added — Sprint 1: Foundations
- Blazor Server .NET 9 project
- PostgreSQL + EF Core 9 + Npgsql configuration
- ASP.NET Identity setup
- CSS design system from mockup (480 lines, dark theme, IBM Plex fonts)
- Component library: StatusBadge, PrioBadge, KpiCard, SlaBar, ToastContainer, Modal, GroupBadge
- Layout shell: collapsible Sidebar (16 nav items), Topbar (search, notifications), NavItem
- Entity model: AppUser, Ticket, Problem, Change, ConfigurationItem, SupportGroup, TimelineEvent
- DbContext with relationships and JSON value converters
- Dashboard page with live KPIs
- 14 placeholder pages with routing
