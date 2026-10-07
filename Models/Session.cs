using System.ComponentModel.DataAnnotations.Schema;

namespace soromaps_api.Models
{
    public class Session
    {
        public long SessionId { get; set; }

        [ForeignKey("User")]
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public string TokenHash { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public DateTime? RevokedAt { get; set; }

        public string? UserAgent { get; set; }

        public DateTime CreatedAt { get; set; }

    }
}
