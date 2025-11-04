using System;

namespace IDS.Data.Models
{
 public class RoleEntry
 {
 public int Id { get; set; }
 public string Name { get; set; } = default!;
 public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
 }
}
