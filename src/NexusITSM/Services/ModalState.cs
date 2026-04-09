namespace NexusITSM.Services;

public class ModalState
{
    public event Action? OnNewTicketRequested;
    public void OpenNewTicket() => OnNewTicketRequested?.Invoke();
}
