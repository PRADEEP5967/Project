using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public sealed class AuditLog
{
    public long Id { get; set; }
    [MaxLength(100)] public string Action { get; set; } = string.Empty;
    [MaxLength(256)] public string? UserEmail { get; set; }
    [MaxLength(45)] public string? IpAddress { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
