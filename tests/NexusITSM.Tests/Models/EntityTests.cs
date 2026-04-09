using NexusITSM.Models.Entities;
using NexusITSM.Models.Enums;

namespace NexusITSM.Tests.Models;

public class EntityTests
{
    [Fact]
    public void Ticket_DefaultValues_AreCorrect()
    {
        var ticket = new Ticket();

        Assert.Equal(TicketStatus.Open, ticket.Status);
        Assert.Equal(TicketPriority.Medium, ticket.Priority);
        Assert.Equal(EscalationLevel.None, ticket.EscalationLevel);
        Assert.Equal(SlaStatus.Ok, ticket.SlaStatus);
        Assert.False(ticket.IsEscalated);
        Assert.Empty(ticket.Tags);
        Assert.Empty(ticket.Timeline);
    }

    [Fact]
    public void Problem_DefaultValues_AreCorrect()
    {
        var problem = new Problem();

        Assert.Equal(ProblemStatus.Open, problem.Status);
        Assert.Equal(TicketPriority.Medium, problem.Priority);
        Assert.False(problem.IsKnownError);
        Assert.Empty(problem.Incidents);
    }

    [Fact]
    public void Change_DefaultValues_AreCorrect()
    {
        var change = new Change();

        Assert.Equal(ChangeType.Standard, change.Type);
        Assert.Equal(ChangeStatus.Draft, change.Status);
        Assert.Equal(1, change.Risk);
        Assert.Equal(1, change.Impact);
    }

    [Fact]
    public void ConfigurationItem_DefaultValues_AreCorrect()
    {
        var ci = new ConfigurationItem();

        Assert.Equal(CIStatus.Active, ci.Status);
        Assert.Equal(CICriticality.Medium, ci.Criticality);
        Assert.False(ci.HasAgent);
        Assert.Empty(ci.Tags);
        Assert.Empty(ci.Services);
    }

    [Fact]
    public void SupportGroup_DefaultValues_AreCorrect()
    {
        var group = new SupportGroup();

        Assert.False(group.AutoAssign);
        Assert.Empty(group.Categories);
        Assert.Empty(group.Members);
        Assert.Empty(group.Tickets);
    }

    [Fact]
    public void AppUser_DefaultValues_AreCorrect()
    {
        var user = new AppUser();

        Assert.True(user.IsActive);
        Assert.Equal("#4B9EFF", user.Color);
        Assert.Equal(string.Empty, user.FullName);
    }
}
