using System;
using System.ComponentModel.DataAnnotations;

namespace IDS.Data.Models
{
    /// <summary>
    /// Represents a support ticket submitted by users for IT Support assistance.
  /// </summary>
    public class SupportTicket
    {
        public int Id { get; set; }

        /// <summary>
        /// Unique ticket reference number (e.g., TKT-20240115-001)
   /// </summary>
        [Required]
        [MaxLength(50)]
        public string TicketNumber { get; set; } = string.Empty;

        /// <summary>
      /// The user who submitted the ticket.
        /// </summary>
        [Required]
        [MaxLength(450)]
        public string SubmittedByUserId { get; set; } = string.Empty;

        /// <summary>
 /// Email of the user who submitted (for display purposes).
        /// </summary>
   [MaxLength(256)]
        public string SubmittedByEmail { get; set; } = string.Empty;

        /// <summary>
        /// Name of the user who submitted.
    /// </summary>
        [MaxLength(200)]
  public string SubmittedByName { get; set; } = string.Empty;

        /// <summary>
        /// Brief subject/title of the ticket.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        /// <summary>
   /// Detailed description of the issue.
        /// </summary>
        [Required]
   [MaxLength(4000)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Category of the issue.
        /// </summary>
        [Required]
 [MaxLength(50)]
   public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Priority level: Low, Medium, High, Critical
      /// </summary>
        [Required]
    [MaxLength(20)]
        public string Priority { get; set; } = "Medium";

    /// <summary>
        /// Current status: Open, InProgress, Resolved, Closed
    /// </summary>
 [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Open";

        /// <summary>
        /// ID of the support staff assigned to this ticket (nullable).
        /// </summary>
        [MaxLength(450)]
        public string? AssignedToUserId { get; set; }

        /// <summary>
        /// Name of the support staff assigned.
      /// </summary>
        [MaxLength(200)]
     public string? AssignedToName { get; set; }

        /// <summary>
  /// When the ticket was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
    /// When the ticket was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When the ticket was resolved (nullable).
        /// </summary>
        public DateTime? ResolvedAt { get; set; }

        /// <summary>
        /// Resolution notes from support staff.
        /// </summary>
        [MaxLength(4000)]
        public string? ResolutionNotes { get; set; }
    }

    /// <summary>
    /// Represents a comment/reply on a support ticket.
    /// </summary>
    public class TicketComment
    {
      public int Id { get; set; }

        /// <summary>
/// The ticket this comment belongs to.
  /// </summary>
        public int TicketId { get; set; }

        /// <summary>
        /// User who posted the comment.
   /// </summary>
        [Required]
    [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;

    /// <summary>
        /// Name of the user who posted.
        /// </summary>
        [MaxLength(200)]
  public string UserName { get; set; } = string.Empty;

 /// <summary>
 /// Whether this comment is from support staff.
        /// </summary>
        public bool IsFromSupport { get; set; }

        /// <summary>
        /// The comment content.
        /// </summary>
        [Required]
        [MaxLength(2000)]
   public string Content { get; set; } = string.Empty;

        /// <summary>
        /// When the comment was posted.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Static helper for ticket-related constants.
    /// </summary>
    public static class TicketConstants
    {
        public static readonly string[] Categories = new[]
        {
        "Account Issue",
    "Password Reset",
            "Access Request",
        "Technical Problem",
 "Security Concern",
            "General Inquiry",
   "Other"
        };

        public static readonly string[] Priorities = new[]
        {
            "Low",
            "Medium",
"High",
        "Critical"
        };

        public static readonly string[] Statuses = new[]
        {
         "Open",
 "InProgress",
            "Resolved",
       "Closed"
        };

        /// <summary>
        /// Generates a unique ticket number.
        /// </summary>
   public static string GenerateTicketNumber()
        {
      return $"TKT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
        }
    }
}
