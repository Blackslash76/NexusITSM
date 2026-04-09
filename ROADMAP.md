# NexusITSM — Roadmap

## Sprint 1: Fondamenta ✅
- [x] Progetto Blazor Server .NET 9
- [x] PostgreSQL + EF Core + Identity setup
- [x] CSS mockup portato nel progetto (480 righe, pixel-perfect)
- [x] Component library (StatusBadge, PrioBadge, KpiCard, SlaBar, Toast, Modal, GroupBadge)
- [x] Layout Shell (Sidebar collapsible, Topbar, NavItem con icone SVG)
- [x] Entity model completo (7 entities)
- [x] DbContext con relazioni + JSON value converters
- [x] DemoDataService singleton
- [x] Dashboard con KPI live calcolati dai dati

## Sprint 2: Core ITSM ✅
- [x] Incidents — lista con filtri (status, priorità, search)
- [x] Incidents — vista Kanban (5 colonne)
- [x] Incidents — detail panel con SLA box + timeline
- [x] Incidents — CRUD completo (crea, assegna, escalate, risolvi, chiudi, note)
- [x] Incidents — bulk actions + modal nuovo ticket

## Sprint 3: Processi ✅
- [x] Problem Management — card grid + RCA + KEDB + detail modal 4 tab
- [x] Change Management — register + Risk Matrix 5x5 + CAB calendar + detail modal 4 tab
- [x] Escalation Matrix — timeline 48h + path escalation + regole toggle + stats
- [x] Service Catalog — 8 categorie + click = crea ticket
- [x] User Portal — hero + wizard 3 step + sidebar "I miei ticket"

## Sprint 4: Advanced ✅
- [x] CMDB — 4 tab (Inventario 10 CI, Discovery, Agents, SNMP)
- [x] SLA Engine — KPI + 4 policies + tracker attivo
- [x] Email Grabber — config IMAP + coda email + converti in ticket + parsing rules
- [x] Reports — grafici CSS + top agents + export
- [x] Settings — 4 tab (Generale, Notifiche, SLA, Gruppi)
- [x] Integrations — 8 integrazioni + log + API keys
- [x] Governance — compliance policies + audit findings + risk register

## Sprint 5: Persistenza & Auth ✅
- [x] Migration EF Core su PostgreSQL
- [x] Seed data completo (12 ticket, 6 gruppi, 7 utenti, 2 problemi, 4 change, 10 CI)
- [x] Login/Register con ASP.NET Identity + 3 ruoli
- [x] TicketService con operazioni DB
- [x] Dashboard + Incidents refactored su DB reale
- [x] [Authorize] su tutte le pagine protette
- [x] xUnit test project (22 test)

## Sprint 6: Real-time & Background ✅
- [x] SLA Background Service (ricalcolo ogni minuto + breach detection)
- [x] SignalR NotificationHub per push notifications
- [x] Toast real-time (ticket created/escalated/resolved/breach)
- [x] Dashboard auto-refresh via SignalR

## Sprint 7: Funzionalità Avanzate ✅
- [x] Export CSV reale (file download, UTF-8 BOM)
- [x] Export PDF reale (QuestPDF, A4 landscape, tabella + KPI)
- [x] Email Grabber reale (MailKit IMAP + auto-categorizzazione)
- [x] Workflow Builder interattivo (canvas con nodi, connessioni SVG, property editor)

## Sprint 8: Enterprise ✅
- [x] Audit Log (entity + service)
- [x] API REST completa (/api/v1/tickets, problems, changes, cmdb, groups, stats, audit, webhooks)
- [x] Webhook service con HMAC signature + auto-disable after 10 failures
- [x] User Dashboard Config entity (personalizzazione layout)
- [x] Migration Enterprise applicata

---

## COMPLETATO — Tutti gli 8 sprint chiusi.

### Statistiche progetto
- 120+ file sorgente
- 80.000+ righe di codice
- 22 test xUnit passanti
- 16 moduli UI funzionanti
- API REST con 15+ endpoint
- SignalR real-time
- Background services
- Export CSV/PDF
