namespace SmartBar.Domain.Entities;

public class InboxMessage
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public string Name { get; set; }
    public DateTime ProcessedAtUtc { get; set; }
}