using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NexusITSM.Models.Entities;
using NexusITSM.Models.Enums;

namespace NexusITSM.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Roles
        string[] roles = ["Admin", "Operator", "User"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Skip if already seeded
        if (await db.SupportGroups.AnyAsync()) return;

        // Groups
        var groups = new List<SupportGroup>
        {
            new() { Id = "g01", Name = "L1 — Help Desk",        Level = GroupLevel.L1, Color = "#30C48D", Icon = "🎧", Email = "helpdesk@nexusitsm.local",  EscalateToGroupId = "g02", AutoAssign = true,  Categories = ["Hardware", "Software", "Printer", "Mobile"] },
            new() { Id = "g02", Name = "L2 — Sistemisti",       Level = GroupLevel.L2, Color = "#4B9EFF", Icon = "🖥",  Email = "sysadmin@nexusitsm.local",  EscalateToGroupId = "g04", AutoAssign = false, Categories = ["Server", "Database", "Backup", "Email"] },
            new() { Id = "g03", Name = "L2 — Network",          Level = GroupLevel.L2, Color = "#FFB020", Icon = "🌐", Email = "network@nexusitsm.local",   EscalateToGroupId = "g04", AutoAssign = false, Categories = ["Network", "VPN"] },
            new() { Id = "g04", Name = "L3 — Dev",              Level = GroupLevel.L3, Color = "#C97BFF", Icon = "💻", Email = "dev@nexusitsm.local",       Categories = ["Application"] },
            new() { Id = "g05", Name = "Security Team",         Level = GroupLevel.L2, Color = "#FF4D6D", Icon = "🔒", Email = "security@nexusitsm.local",  Categories = ["Security", "AccessManagement"] },
            new() { Id = "g06", Name = "Change Advisory Board", Level = GroupLevel.L3, Color = "#FF7846", Icon = "🔄", Email = "cab@nexusitsm.local" },
        };
        db.SupportGroups.AddRange(groups);
        await db.SaveChangesAsync();

        // Users
        var users = new (string Id, string Email, string Full, string Init, string Color, string Role, string Dept, string GroupId, string AppRole)[]
        {
            ("a1", "marco.rossi@nexusitsm.local",   "Marco Rossi",   "MR", "#4B9EFF", "L2 Engineer",   "IT Ops",   "g02", "Admin"),
            ("a2", "sara.bianchi@nexusitsm.local",   "Sara Bianchi",  "SB", "#30C48D", "L1 Support",    "Helpdesk", "g01", "Operator"),
            ("a3", "luca.ferrari@nexusitsm.local",   "Luca Ferrari",  "LF", "#FFB020", "Sysadmin",      "Infra",    "g02", "Operator"),
            ("a4", "anna.marino@nexusitsm.local",    "Anna Marino",   "AM", "#C97BFF", "Network Eng.",  "NetOps",   "g03", "Operator"),
            ("a5", "paolo.conti@nexusitsm.local",    "Paolo Conti",   "PC", "#FF4D6D", "Security Eng.", "SecOps",   "g05", "Operator"),
            ("a6", "elena.russo@nexusitsm.local",    "Elena Russo",   "ER", "#FF7846", "Change Mgr",    "ITSM",     "g06", "Operator"),
        };

        var agentMap = new Dictionary<string, AppUser>();
        foreach (var u in users)
        {
            var user = new AppUser
            {
                Id = u.Id, UserName = u.Email, Email = u.Email, FullName = u.Full,
                Initials = u.Init, Color = u.Color, Role = u.Role,
                Department = u.Dept, GroupId = u.GroupId, IsActive = true, EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(user, "Nexus2025!");
            if (result.Succeeded) await userManager.AddToRoleAsync(user, u.AppRole);
            agentMap[u.Id] = user;
        }

        // Admin account
        var admin = new AppUser
        {
            Id = "admin", UserName = "admin@nexusitsm.local", Email = "admin@nexusitsm.local",
            FullName = "Admin Marco", Initials = "AM", Color = "#4B9EFF",
            Role = "Super Admin", Department = "IT", IsActive = true, EmailConfirmed = true
        };
        var adminResult = await userManager.CreateAsync(admin, "Nexus2025!");
        if (adminResult.Succeeded) await userManager.AddToRoleAsync(admin, "Admin");

        // Tickets
        var now = DateTime.UtcNow;
        var tickets = new List<Ticket>
        {
            MakeTicket("INC-1001", "Exchange server non risponde — email bloccate per tutti gli utenti",
                TicketPriority.Critical, TicketStatus.Escalated, TicketCategory.Email, "a1", "g02",
                "IT Manager", "IT", now.AddHours(-6), 92, SlaStatus.Breach, EscalationLevel.L2, ["exchange","email","critical"]),
            MakeTicket("INC-1002", "VPN disconnessioni frequenti sede Milano",
                TicketPriority.High, TicketStatus.InProgress, TicketCategory.VPN, "a4", "g03",
                "Direttore Commerciale", "Sales", now.AddHours(-8), 65, SlaStatus.Warning, EscalationLevel.None, ["vpn","network","milano"]),
            MakeTicket("INC-1003", "SAP GUI crash all'apertura modulo FI",
                TicketPriority.High, TicketStatus.InProgress, TicketCategory.Application, "a1", "g02",
                "Controller", "Finance", now.AddHours(-5), 45, SlaStatus.Ok, EscalationLevel.None, ["sap","application"]),
            MakeTicket("INC-1004", "Stampante HP piano 3 — errore carta inceppata persistente",
                TicketPriority.Medium, TicketStatus.Open, TicketCategory.Printer, "a2", "g01",
                "Segreteria", "Admin", now.AddHours(-3), 30, SlaStatus.Ok, EscalationLevel.None, ["printer","hardware"]),
            MakeTicket("INC-1005", "Active Directory — account lockout multipli reparto HR",
                TicketPriority.High, TicketStatus.InProgress, TicketCategory.Security, "a5", "g05",
                "HR Manager", "HR", now.AddHours(-4), 78, SlaStatus.Warning, EscalationLevel.None, ["security","ad","lockout"]),
            MakeTicket("INC-1006", "WiFi intermittente sala riunioni B2",
                TicketPriority.Low, TicketStatus.Waiting, TicketCategory.Network, "a4", "g03",
                "Office Manager", "Facilities", now.AddHours(-24), 20, SlaStatus.Ok, EscalationLevel.None, ["wifi","network"]),
            MakeTicket("INC-1007", "Backup fallito — NAS principale non raggiungibile",
                TicketPriority.Critical, TicketStatus.Open, TicketCategory.Backup, "a3", "g02",
                "Sistema", "IT Ops", now.AddHours(-2), 88, SlaStatus.Breach, EscalationLevel.None, ["backup","nas","critical"]),
            MakeTicket("INC-1008", "Outlook non sincronizza calendario condiviso",
                TicketPriority.Medium, TicketStatus.Open, TicketCategory.Email, "a2", "g01",
                "Marketing Coord.", "Marketing", now.AddHours(-7), 35, SlaStatus.Ok, EscalationLevel.None, ["email","outlook"]),
            MakeTicket("INC-1009", "Richiesta nuovo laptop per neoassunto",
                TicketPriority.Low, TicketStatus.InProgress, TicketCategory.Hardware, "a2", "g01",
                "HR", "HR", now.AddHours(-48), 15, SlaStatus.Ok, EscalationLevel.None, ["hardware","onboarding"]),
            MakeTicket("INC-1010", "Database SQL Server — query lente su tabella ordini",
                TicketPriority.High, TicketStatus.Escalated, TicketCategory.Database, "a1", "g04",
                "Dev Lead", "Development", now.AddHours(-10), 80, SlaStatus.Warning, EscalationLevel.L2, ["database","performance","sql"]),
            MakeTicket("INC-1011", "Telefono VoIP non funziona — interno 2145",
                TicketPriority.Medium, TicketStatus.Resolved, TicketCategory.Hardware, "a4", "g03",
                "Contabilità", "Finance", now.AddHours(-30), 100, SlaStatus.Ok, EscalationLevel.None, ["voip","network"]),
            MakeTicket("INC-1012", "Accesso negato a cartella condivisa \\\\fs01\\progetti",
                TicketPriority.Medium, TicketStatus.Open, TicketCategory.AccessManagement, "a2", "g01",
                "Project Manager", "PMO", now.AddHours(-1), 10, SlaStatus.Ok, EscalationLevel.None, ["access","share"]),
        };
        db.Tickets.AddRange(tickets);

        // Timeline events for key tickets
        db.TimelineEvents.AddRange(
            new() { TicketId = "INC-1001", Type = TimelineEventType.Created, Actor = "Sistema", Message = "Ticket creato automaticamente da monitoring", CreatedAt = now.AddHours(-6) },
            new() { TicketId = "INC-1001", Type = TimelineEventType.Assigned, Actor = "Auto-routing", Message = "Assegnato a L2 — Sistemisti / Marco Rossi", CreatedAt = now.AddHours(-6) },
            new() { TicketId = "INC-1001", Type = TimelineEventType.Escalated, Actor = "Marco Rossi", Message = "Escalation a L2 Engineering — server non risponde a restart", CreatedAt = now.AddHours(-3) },
            new() { TicketId = "INC-1001", Type = TimelineEventType.Note, Actor = "Marco Rossi", Message = "Memory dump in corso, possibile memory leak su transport service", CreatedAt = now.AddHours(-1) },
            new() { TicketId = "INC-1002", Type = TimelineEventType.Created, Actor = "Direttore Commerciale", Message = "Ticket aperto via portale", CreatedAt = now.AddHours(-8) },
            new() { TicketId = "INC-1005", Type = TimelineEventType.Created, Actor = "HR Manager", Message = "Più account HR bloccati simultaneamente", CreatedAt = now.AddHours(-4) },
            new() { TicketId = "INC-1005", Type = TimelineEventType.Note, Actor = "Paolo Conti", Message = "Verifico log SIEM per possibile brute force", CreatedAt = now.AddHours(-2) },
            new() { TicketId = "INC-1007", Type = TimelineEventType.Created, Actor = "Monitoring", Message = "Alert: backup notturno fallito — NAS unreachable", CreatedAt = now.AddHours(-2) },
            new() { TicketId = "INC-1010", Type = TimelineEventType.Escalated, Actor = "Sara Bianchi", Message = "Escalation a L3 Dev — problema applicativo", CreatedAt = now.AddHours(-8) },
            new() { TicketId = "INC-1011", Type = TimelineEventType.Resolved, Actor = "Anna Marino", Message = "Porta switch difettosa, spostato su porta 15", CreatedAt = now.AddHours(-26) }
        );

        // Problems
        db.Problems.AddRange(
            new() { Id = "PRB-001", Title = "Memory leak su Exchange Server 2019 — Transport Service",
                Status = ProblemStatus.InAnalysis, Priority = TicketPriority.Critical, AssigneeId = "a1",
                RootCauseAnalysis = "Il servizio Microsoft Exchange Transport presenta un memory leak dopo l'ultimo CU.",
                Workaround = "Restart schedulato del servizio ogni 6 ore tramite Task Scheduler",
                IsKnownError = true, CreatedAt = now.AddDays(-5) },
            new() { Id = "PRB-002", Title = "Latenza intermittente switch core piano 2",
                Status = ProblemStatus.Open, Priority = TicketPriority.High, AssigneeId = "a4",
                CreatedAt = now.AddDays(-3) }
        );

        // Changes
        db.Changes.AddRange(
            new() { Id = "CHG-001", Title = "Upgrade Exchange Server 2019 → 2022", Type = ChangeType.Major,
                Status = ChangeStatus.PendingCAB, Risk = 4, Impact = 4, Requester = "IT Manager",
                AssigneeId = "a6", CabDate = now.AddDays(3), CreatedAt = now.AddDays(-7) },
            new() { Id = "CHG-002", Title = "Patch firmware switch Cisco Catalyst 9300", Type = ChangeType.Standard,
                Status = ChangeStatus.Approved, Risk = 2, Impact = 3, Requester = "Network Team",
                AssigneeId = "a4", CabDate = now.AddDays(5), CreatedAt = now.AddDays(-4) },
            new() { Id = "CHG-003", Title = "Migrazione DNS a nuovo domain controller", Type = ChangeType.Minor,
                Status = ChangeStatus.Draft, Risk = 3, Impact = 4, Requester = "Sysadmin",
                AssigneeId = "a3", CabDate = now.AddDays(7), CreatedAt = now.AddDays(-2) },
            new() { Id = "CHG-004", Title = "Hotfix sicurezza CVE-2024-38124 su tutti i DC", Type = ChangeType.Emergency,
                Status = ChangeStatus.Approved, Risk = 3, Impact = 5, Requester = "Security Team",
                AssigneeId = "a5", CabDate = now.AddDays(1), CreatedAt = now.AddDays(-1) }
        );

        // Configuration Items
        db.ConfigurationItems.AddRange(
            new() { Id = "CI-001", Name = "SRV-PROD-01", Type = "Server", IpAddress = "10.0.1.10", OperatingSystem = "Windows Server 2022", Status = CIStatus.Active, Criticality = CICriticality.Critical, HasAgent = true, AgentVersion = "3.12.1", Location = "DC1 Rack A3", LastSeen = now.AddMinutes(-5), Tags = ["production","windows"], Services = ["Exchange","AD"] },
            new() { Id = "CI-002", Name = "SRV-DB-01", Type = "Database Server", IpAddress = "10.0.1.11", OperatingSystem = "Ubuntu 22.04", Status = CIStatus.Active, Criticality = CICriticality.Critical, HasAgent = true, AgentVersion = "3.12.1", Location = "DC1 Rack A4", LastSeen = now.AddMinutes(-3), Tags = ["production","linux","database"], Services = ["PostgreSQL","Redis"] },
            new() { Id = "CI-003", Name = "FW-FORTINET-01", Type = "Firewall", IpAddress = "10.0.0.1", Status = CIStatus.Active, Criticality = CICriticality.Critical, SnmpVersion = "v3", Location = "DC1 Rack A1", LastSeen = now.AddMinutes(-1), Tags = ["security","network"], Services = ["Firewall","VPN"] },
            new() { Id = "CI-004", Name = "SW-CORE-01", Type = "Switch", IpAddress = "10.0.0.10", OperatingSystem = "Cisco IOS 17.6", Status = CIStatus.Active, Criticality = CICriticality.High, SnmpVersion = "v2c", Location = "DC1 Rack B1", LastSeen = now.AddMinutes(-2), Tags = ["network","core"] },
            new() { Id = "CI-005", Name = "SRV-MAIL-01", Type = "Email Server", IpAddress = "10.0.1.20", OperatingSystem = "Windows Server 2019", Status = CIStatus.Degraded, Criticality = CICriticality.Critical, HasAgent = true, AgentVersion = "3.11.5", Location = "DC1 Rack A3", LastSeen = now.AddMinutes(-10), Tags = ["production","email"], Services = ["Exchange"] },
            new() { Id = "CI-006", Name = "DC-01", Type = "Domain Controller", IpAddress = "10.0.1.5", OperatingSystem = "Windows Server 2022", Status = CIStatus.Active, Criticality = CICriticality.Critical, HasAgent = true, AgentVersion = "3.12.1", Location = "DC1 Rack A2", LastSeen = now.AddMinutes(-1), Tags = ["production","ad"], Services = ["Active Directory","DNS","DHCP"] },
            new() { Id = "CI-007", Name = "NAS-01", Type = "File Server", IpAddress = "10.0.1.30", OperatingSystem = "Synology DSM 7.2", Status = CIStatus.Offline, Criticality = CICriticality.High, Location = "DC1 Rack C1", LastSeen = now.AddHours(-2), Tags = ["storage"], Services = ["NFS","SMB"] },
            new() { Id = "CI-008", Name = "AP-WIFI-01", Type = "Access Point", IpAddress = "192.168.1.1", OperatingSystem = "UniFi OS 3.1", Status = CIStatus.Active, Criticality = CICriticality.Medium, SnmpVersion = "v2c", Location = "Piano 2 Sala B2", LastSeen = now.AddMinutes(-5) },
            new() { Id = "CI-009", Name = "PC-MROSSI", Type = "Workstation", IpAddress = "10.0.1.45", OperatingSystem = "Windows 11 Pro", Status = CIStatus.Active, Criticality = CICriticality.Low, Location = "Ufficio IT", LastSeen = now.AddHours(-1) },
            new() { Id = "CI-010", Name = "PRINTER-HR", Type = "Printer", IpAddress = "10.0.1.200", OperatingSystem = "HP JetDirect", Status = CIStatus.Active, Criticality = CICriticality.Low, SnmpVersion = "v2c", Location = "Piano 3 Area HR", LastSeen = now.AddMinutes(-30) }
        );

        await db.SaveChangesAsync();
    }

    private static Ticket MakeTicket(string id, string title, TicketPriority prio, TicketStatus status,
        TicketCategory cat, string assigneeId, string groupId, string requester, string dept,
        DateTime created, int slaPct, SlaStatus slaStatus, EscalationLevel escLevel, List<string> tags)
    {
        var slaHours = prio switch { TicketPriority.Critical => 2, TicketPriority.High => 4, TicketPriority.Medium => 8, _ => 24 };
        return new Ticket
        {
            Id = id, Title = title, Priority = prio, Status = status, Category = cat,
            AssigneeId = assigneeId, GroupId = groupId, Requester = requester, Department = dept,
            CreatedAt = created, SlaPercent = slaPct, SlaStatus = slaStatus,
            SlaBreachAt = created.AddHours(slaHours), EscalationLevel = escLevel,
            IsEscalated = escLevel != EscalationLevel.None, Tags = tags,
        };
    }
}
