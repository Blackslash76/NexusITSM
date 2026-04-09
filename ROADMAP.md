# NexusITSM — Roadmap

## Sprint 1: Fondamenta ✅ COMPLETATO
- [x] Progetto Blazor Server .NET 9
- [x] PostgreSQL + EF Core + Identity setup
- [x] CSS mockup portato nel progetto (480 righe, pixel-perfect)
- [x] Component library (StatusBadge, PrioBadge, KpiCard, SlaBar, Toast, Modal, GroupBadge)
- [x] Layout Shell (Sidebar collapsible, Topbar, NavItem con icone SVG)
- [x] Entity model completo (7 entities)
- [x] DbContext con relazioni + JSON value converters
- [x] DemoDataService singleton con 12 ticket, 6 agenti, 6 gruppi, 2 problemi, 4 change
- [x] Dashboard con KPI live calcolati dai dati

## Sprint 2: Core ITSM ✅ COMPLETATO
- [x] Incidents — lista con filtri (status, priorità, search)
- [x] Incidents — vista Kanban (5 colonne)
- [x] Incidents — detail panel con SLA box + timeline
- [x] Incidents — CRUD completo (crea, assegna, escalate, risolvi, chiudi, note)
- [x] Incidents — bulk actions
- [x] Incidents — modal nuovo ticket con form completo

## Sprint 3: Processi ✅ COMPLETATO
- [x] Problem Management — card grid + RCA + KEDB + detail modal 4 tab
- [x] Change Management — register + Risk Matrix 5x5 + CAB calendar + detail modal 4 tab
- [x] Escalation Matrix — timeline 48h + path escalation + regole toggle + stats
- [x] Service Catalog — 8 categorie + click = crea ticket
- [x] User Portal — hero + wizard 3 step + sidebar "I miei ticket"

## Sprint 4: Advanced ✅ COMPLETATO
- [x] CMDB — 4 tab (Inventario 10 CI, Discovery, Agents, SNMP)
- [x] SLA Engine — KPI + 4 policies + tracker attivo
- [x] Email Grabber — config IMAP + coda 5 email + converti in ticket + parsing rules
- [x] Reports — grafici CSS bar chart + top agents + export
- [x] Settings — 4 tab (Generale, Notifiche, SLA, Gruppi)
- [x] Integrations — 8 integrazioni + log + API keys
- [x] Governance — compliance policies + audit findings + risk register

## Sprint 5: Persistenza & Auth ✅ COMPLETATO
- [x] Migration EF Core iniziale su PostgreSQL
- [x] Seed data completo (12 ticket, 6 gruppi, 7 utenti, 2 problemi, 4 change, 10 CI, timeline)
- [x] Login page `/login` con ASP.NET Identity (dark theme, pixel-perfect)
- [x] Registrazione `/register` con creazione utente + ruolo User
- [x] Auth endpoints (POST login, POST register, GET logout)
- [x] 3 ruoli: Admin, Operator, User
- [x] Sidebar mostra utente loggato (nome, ruolo, logout)
- [x] TicketService con operazioni DB (CRUD, escalate, note, stats)
- [ ] **Refactor pagine: da DemoDataService a TicketService/DbContext**
- [ ] **Protezione pagine con [Authorize]**

---

## Sprint 6: Real-time & Background
- [ ] SLA Background Service — HostedService che ricalcola SLA ogni minuto
- [ ] SignalR Hub — notifiche push per: nuovo ticket, escalation, SLA breach
- [ ] Toast notifiche real-time nel layout
- [ ] Auto-refresh dashboard e incident list

## Sprint 7: Funzionalità avanzate
- [ ] Workflow Builder — canvas drag & drop (Blazor.Diagrams o JS interop custom)
- [ ] CMDB Topology graph — visualizzazione relazioni CI (SVG/Canvas)
- [ ] Export CSV reale (file download)
- [ ] Export PDF report (con libreria tipo QuestPDF)
- [ ] Email Grabber reale — connessione IMAP con MailKit
- [ ] CMDB Discovery reale — ping sweep + SNMP walk

## Sprint 8: Enterprise
- [ ] Multi-tenancy (se serve)
- [ ] Active Directory integration
- [ ] Audit log completo
- [ ] Dashboard personalizzabili per utente
- [ ] API REST per integrazioni esterne
- [ ] Webhook in/out
