using System;
using System.Collections.Generic;
using System.Text;

namespace SmartBar.Domain.Entities;

public class InboxMessage
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime ProcessedAtUtc { get; set; }
}