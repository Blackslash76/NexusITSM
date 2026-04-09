namespace NexusITSM.Models.Enums;

public enum TicketStatus
{
    Open,
    InProgress,
    Waiting,
    Escalated,
    Resolved,
    Closed
}

public enum TicketPriority
{
    Critical = 1,
    High = 2,
    Medium = 3,
    Low = 4
}

public enum TicketCategory
{
    Hardware,
    Software,
    Network,
    VPN,
    Email,
    Printer,
    Mobile,
    Server,
    Database,
    Cloud,
    Application,
    Backup,
    Security,
    AccessManagement
}

public enum ProblemStatus
{
    Open,
    InAnalysis,
    KnownError,
    Resolved,
    Closed
}

public enum ChangeType
{
    Standard,
    Minor,
    Major,
    Emergency
}

public enum ChangeStatus
{
    Draft,
    PendingCAB,
    Approved,
    Scheduled,
    Implementing,
    Completed,
    Rejected,
    RolledBack
}

public enum CIStatus
{
    Active,
    Degraded,
    Offline,
    Maintenance,
    Decommissioned
}

public enum CICriticality
{
    Low,
    Medium,
    High,
    Critical
}

public enum SlaStatus
{
    Ok,
    Warning,
    Breach
}

public enum GroupLevel
{
    L1,
    L2,
    L3
}

public enum TimelineEventType
{
    Created,
    Updated,
    Escalated,
    Resolved,
    Note,
    Assigned,
    StatusChanged
}

public enum EscalationLevel
{
    None = 0,
    L1 = 1,
    L2 = 2,
    L3 = 3
}

public enum WorkflowNodeType
{
    Trigger,
    Condition,
    Action,
    Notify,
    Assign,
    Escalate,
    Resolve,
    Delay
}
