using NexusITSM.Models.Entities;
using NexusITSM.Models.Enums;

namespace NexusITSM.Services;

public class DemoDataService
{
    public List<AppUser> Agents { get; }
    public List<SupportGroup> Groups { get; }
    public List<Ticket> Tickets { get; }
    public List<Problem> Problems { get; }
    public List<Change> Changes { get; }

    private int _ticketSeq = 1020;

    public DemoDataService()
    {
        Agents = InitAgents();
        Groups = InitGroups();
        Tickets = InitTickets();
        Problems = InitProblems();
        Changes = InitChanges();
    }

    public string NextTicketId() => $"INC-{++_ticketSeq}";

    private List<AppUser> InitAgents() =>
    [
        new() { Id = "a1", FullName = "Marco Rossi",   Initials = "MR", Color = "#4B9EFF", Role = "L2 Engineer",   Department = "IT Ops" },
        new() { Id = "a2", FullName = "Sara Bianchi",  Initials = "SB", Color = "#30C48D", Role = "L1 Support",    Department = "Helpdesk" },
        new() { Id = "a3", FullName = "Luca Ferrari",  Initials = "LF", Color = "#FFB020", Role = "Sysadmin",      Department = "Infra" },
        new() { Id = "a4", FullName = "Anna Marino",   Initials = "AM", Color = "#C97BFF", Role = "Network Eng.",  Department = "NetOps" },
        new() { Id = "a5", FullName = "Paolo Conti",   Initials = "PC", Color = "#FF4D6D", Role = "Security Eng.", Department = "SecOps" },
        new() { Id = "a6", FullName = "Elena Russo",   Initials = "ER", Color = "#FF7846", Role = "Change Mgr",    Department = "ITSM" },
    ];

    private List<SupportGroup> InitGroups() =>
    [
        new() { Id = "g01", Name = "L1 — Help Desk",        Level = GroupLevel.L1, Color = "#30C48D", Icon = "🎧" },
        new() { Id = "g02", Name = "L2 — Sistemisti",       Level = GroupLevel.L2, Color = "#4B9EFF", Icon = "🖥" },
        new() { Id = "g03", Name = "L2 — Network",          Level = GroupLevel.L2, Color = "#FFB020", Icon = "🌐" },
        new() { Id = "g04", Name = "L3 — Dev",              Level = GroupLevel.L3, Color = "#C97BFF", Icon = "💻" },
        new() { Id = "g05", Name = "Security Team",         Level = GroupLevel.L2, Color = "#FF4D6D", Icon = "🔒" },
        new() { Id = "g06", Name = "Change Advisory Board", Level = GroupLevel.L3, Color = "#FF7846", Icon = "🔄" },
    ];

    private List<Ticket> InitTickets()
    {
        var a = Agents;
        var g = Groups;
        var now = DateTime.UtcNow;
        return
        [
            MakeTicket("INC-1001", "Exchange server non risponde — email bloccate per tutti gli utenti",
                TicketPriority.Critical, TicketStatus.Escalated, TicketCategory.Email, a[0], g[1],
                "IT Manager", "IT", now.AddHours(-6), 92, SlaStatus.Breach, EscalationLevel.L2,
                ["exchange","email","critical"], [
                    new() { Type = TimelineEventType.Created, Actor = "Sistema", Message = "Ticket creato automaticamente da monitoring", CreatedAt = now.AddHours(-6) },
                    new() { Type = TimelineEventType.Assigned, Actor = "Auto-routing", Message = "Assegnato a L2 — Sistemisti / Marco Rossi", CreatedAt = now.AddHours(-6) },
                    new() { Type = TimelineEventType.Escalated, Actor = "Marco Rossi", Message = "Escalation a L2 Engineering — server non risponde a restart", CreatedAt = now.AddHours(-3) },
                    new() { Type = TimelineEventType.Note, Actor = "Marco Rossi", Message = "Memory dump in corso, possibile memory leak su transport service", CreatedAt = now.AddHours(-1) },
                ]),
            MakeTicket("INC-1002", "VPN disconnessioni frequenti sede Milano",
                TicketPriority.High, TicketStatus.InProgress, TicketCategory.VPN, a[3], g[2],
                "Direttore Commerciale", "Sales", now.AddHours(-8), 65, SlaStatus.Warning, EscalationLevel.None,
                ["vpn","network","milano"], [
                    new() { Type = TimelineEventType.Created, Actor = "Direttore Commerciale", Message = "Ticket aperto via portale", CreatedAt = now.AddHours(-8) },
                    new() { Type = TimelineEventType.Assigned, Actor = "Auto-routing", Message = "Assegnato a L2 — Network / Anna Marino", CreatedAt = now.AddHours(-8) },
                    new() { Type = TimelineEventType.Updated, Actor = "Anna Marino", Message = "Analisi log concentratore VPN in corso", CreatedAt = now.AddHours(-4) },
                ]),
            MakeTicket("INC-1003", "SAP GUI crash all'apertura modulo FI",
                TicketPriority.High, TicketStatus.InProgress, TicketCategory.Application, a[0], g[1],
                "Controller", "Finance", now.AddHours(-5), 45, SlaStatus.Ok, EscalationLevel.None,
                ["sap","application"], [
                    new() { Type = TimelineEventType.Created, Actor = "Controller", Message = "SAP GUI si chiude immediatamente aprendo il modulo FI", CreatedAt = now.AddHours(-5) },
                    new() { Type = TimelineEventType.Assigned, Actor = "Auto-routing", Message = "Assegnato a L2 — Sistemisti / Marco Rossi", CreatedAt = now.AddHours(-5) },
                ]),
            MakeTicket("INC-1004", "Stampante HP piano 3 — errore carta inceppata persistente",
                TicketPriority.Medium, TicketStatus.Open, TicketCategory.Printer, a[1], g[0],
                "Segreteria", "Admin", now.AddHours(-3), 30, SlaStatus.Ok, EscalationLevel.None,
                ["printer","hardware"], [
                    new() { Type = TimelineEventType.Created, Actor = "Segreteria", Message = "Stampante segnala carta inceppata ma non c'è carta bloccata", CreatedAt = now.AddHours(-3) },
                ]),
            MakeTicket("INC-1005", "Active Directory — account lockout multipli reparto HR",
                TicketPriority.High, TicketStatus.InProgress, TicketCategory.Security, a[4], g[4],
                "HR Manager", "HR", now.AddHours(-4), 78, SlaStatus.Warning, EscalationLevel.None,
                ["security","ad","lockout"], [
                    new() { Type = TimelineEventType.Created, Actor = "HR Manager", Message = "Più account HR bloccati simultaneamente", CreatedAt = now.AddHours(-4) },
                    new() { Type = TimelineEventType.Assigned, Actor = "Auto-routing", Message = "Assegnato a Security Team / Paolo Conti", CreatedAt = now.AddHours(-4) },
                    new() { Type = TimelineEventType.Note, Actor = "Paolo Conti", Message = "Verifico log SIEM per possibile brute force", CreatedAt = now.AddHours(-2) },
                ]),
            MakeTicket("INC-1006", "WiFi intermittente sala riunioni B2",
                TicketPriority.Low, TicketStatus.Waiting, TicketCategory.Network, a[3], g[2],
                "Office Manager", "Facilities", now.AddHours(-24), 20, SlaStatus.Ok, EscalationLevel.None,
                ["wifi","network"], [
                    new() { Type = TimelineEventType.Created, Actor = "Office Manager", Message = "WiFi si disconnette ogni 10-15 min in sala B2", CreatedAt = now.AddHours(-24) },
                    new() { Type = TimelineEventType.Updated, Actor = "Anna Marino", Message = "In attesa sostituzione AP — ordine effettuato", CreatedAt = now.AddHours(-12) },
                ]),
            MakeTicket("INC-1007", "Backup fallito — NAS principale non raggiungibile",
                TicketPriority.Critical, TicketStatus.Open, TicketCategory.Backup, a[2], g[1],
                "Sistema", "IT Ops", now.AddHours(-2), 88, SlaStatus.Breach, EscalationLevel.None,
                ["backup","nas","critical"], [
                    new() { Type = TimelineEventType.Created, Actor = "Monitoring", Message = "Alert: backup notturno fallito — NAS unreachable", CreatedAt = now.AddHours(-2) },
                    new() { Type = TimelineEventType.Assigned, Actor = "Auto-routing", Message = "Assegnato a L2 — Sistemisti / Luca Ferrari", CreatedAt = now.AddHours(-2) },
                ]),
            MakeTicket("INC-1008", "Outlook non sincronizza calendario condiviso",
                TicketPriority.Medium, TicketStatus.Open, TicketCategory.Email, a[1], g[0],
                "Marketing Coord.", "Marketing", now.AddHours(-7), 35, SlaStatus.Ok, EscalationLevel.None,
                ["email","outlook"], [
                    new() { Type = TimelineEventType.Created, Actor = "Marketing Coord.", Message = "Calendario team non si aggiorna da ieri", CreatedAt = now.AddHours(-7) },
                ]),
            MakeTicket("INC-1009", "Richiesta nuovo laptop per neoassunto",
                TicketPriority.Low, TicketStatus.InProgress, TicketCategory.Hardware, a[1], g[0],
                "HR", "HR", now.AddHours(-48), 15, SlaStatus.Ok, EscalationLevel.None,
                ["hardware","onboarding"], [
                    new() { Type = TimelineEventType.Created, Actor = "HR", Message = "Nuovo dipendente — inizio 15/04, serve laptop + account", CreatedAt = now.AddHours(-48) },
                    new() { Type = TimelineEventType.Updated, Actor = "Sara Bianchi", Message = "Laptop ordinato, account AD creato", CreatedAt = now.AddHours(-24) },
                ]),
            MakeTicket("INC-1010", "Database SQL Server — query lente su tabella ordini",
                TicketPriority.High, TicketStatus.Escalated, TicketCategory.Database, a[0], g[3],
                "Dev Lead", "Development", now.AddHours(-10), 80, SlaStatus.Warning, EscalationLevel.L2,
                ["database","performance","sql"], [
                    new() { Type = TimelineEventType.Created, Actor = "Dev Lead", Message = "Query su tabella ordini passata da 200ms a 15s", CreatedAt = now.AddHours(-10) },
                    new() { Type = TimelineEventType.Escalated, Actor = "Sara Bianchi", Message = "Escalation a L3 Dev — problema applicativo", CreatedAt = now.AddHours(-8) },
                    new() { Type = TimelineEventType.Note, Actor = "Marco Rossi", Message = "Index fragmentation al 98%, rebuild in corso", CreatedAt = now.AddHours(-4) },
                ]),
            MakeTicket("INC-1011", "Telefono VoIP non funziona — interno 2145",
                TicketPriority.Medium, TicketStatus.Resolved, TicketCategory.Hardware, a[3], g[2],
                "Contabilità", "Finance", now.AddHours(-30), 100, SlaStatus.Ok, EscalationLevel.None,
                ["voip","network"], [
                    new() { Type = TimelineEventType.Created, Actor = "Contabilità", Message = "Telefono VoIP morto, nessun segnale", CreatedAt = now.AddHours(-30) },
                    new() { Type = TimelineEventType.Resolved, Actor = "Anna Marino", Message = "Porta switch difettosa, spostato su porta 15", CreatedAt = now.AddHours(-26) },
                ]),
            MakeTicket("INC-1012", "Accesso negato a cartella condivisa \\\\fs01\\progetti",
                TicketPriority.Medium, TicketStatus.Open, TicketCategory.AccessManagement, a[1], g[0],
                "Project Manager", "PMO", now.AddHours(-1), 10, SlaStatus.Ok, EscalationLevel.None,
                ["access","share"], [
                    new() { Type = TimelineEventType.Created, Actor = "Project Manager", Message = "Non riesco più ad accedere alla cartella progetti dal mio PC", CreatedAt = now.AddHours(-1) },
                ]),
        ];
    }

    private static Ticket MakeTicket(string id, string title, TicketPriority prio, TicketStatus status,
        TicketCategory cat, AppUser assignee, SupportGroup group, string requester, string dept,
        DateTime created, int slaPct, SlaStatus slaStatus, EscalationLevel escLevel,
        List<string> tags, List<TimelineEvent> timeline)
    {
        var slaHours = prio switch { TicketPriority.Critical => 2, TicketPriority.High => 4, TicketPriority.Medium => 8, _ => 24 };
        return new Ticket
        {
            Id = id, Title = title, Priority = prio, Status = status, Category = cat,
            Assignee = assignee, AssigneeId = assignee.Id, Group = group, GroupId = group.Id,
            Requester = requester, Department = dept, CreatedAt = created,
            SlaPercent = slaPct, SlaStatus = slaStatus, SlaBreachAt = created.AddHours(slaHours),
            EscalationLevel = escLevel, IsEscalated = escLevel != EscalationLevel.None,
            Tags = tags, Timeline = timeline,
        };
    }

    private List<Problem> InitProblems()
    {
        var now = DateTime.UtcNow;
        return
        [
            new() { Id = "PRB-001", Title = "Memory leak su Exchange Server 2019 — Transport Service",
                Status = ProblemStatus.InAnalysis, Priority = TicketPriority.Critical, Assignee = Agents[0],
                RootCauseAnalysis = "Il servizio Microsoft Exchange Transport presenta un memory leak dopo l'ultimo CU. Il processo EdgeTransport.exe cresce fino a consumare tutta la RAM disponibile.",
                Workaround = "Restart schedulato del servizio ogni 6 ore tramite Task Scheduler",
                IsKnownError = true, CreatedAt = now.AddDays(-5),
                Incidents = Tickets.Where(t => t.Id is "INC-1001" or "INC-1008").ToList() },
            new() { Id = "PRB-002", Title = "Latenza intermittente switch core piano 2",
                Status = ProblemStatus.Open, Priority = TicketPriority.High, Assignee = Agents[3],
                CreatedAt = now.AddDays(-3),
                Incidents = Tickets.Where(t => t.Id is "INC-1002" or "INC-1006").ToList() },
        ];
    }

    private List<Change> InitChanges()
    {
        var now = DateTime.UtcNow;
        return
        [
            new() { Id = "CHG-001", Title = "Upgrade Exchange Server 2019 → 2022", Type = ChangeType.Major,
                Status = ChangeStatus.PendingCAB, Risk = 4, Impact = 4, Requester = "IT Manager",
                Assignee = Agents[5], CabDate = now.AddDays(3), CreatedAt = now.AddDays(-7) },
            new() { Id = "CHG-002", Title = "Patch firmware switch Cisco Catalyst 9300", Type = ChangeType.Standard,
                Status = ChangeStatus.Approved, Risk = 2, Impact = 3, Requester = "Network Team",
                Assignee = Agents[3], CabDate = now.AddDays(5), CreatedAt = now.AddDays(-4) },
            new() { Id = "CHG-003", Title = "Migrazione DNS a nuovo domain controller", Type = ChangeType.Minor,
                Status = ChangeStatus.Draft, Risk = 3, Impact = 4, Requester = "Sysadmin",
                Assignee = Agents[2], CabDate = now.AddDays(7), CreatedAt = now.AddDays(-2) },
            new() { Id = "CHG-004", Title = "Hotfix sicurezza CVE-2024-38124 su tutti i DC", Type = ChangeType.Emergency,
                Status = ChangeStatus.Approved, Risk = 3, Impact = 5, Requester = "Security Team",
                Assignee = Agents[4], CabDate = now.AddDays(1), CreatedAt = now.AddDays(-1) },
        ];
    }
}
