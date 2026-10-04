using System.ComponentModel.DataAnnotations.Schema;

namespace soromaps_api.Models
{
    public class Comment
    {
        public Guid CommentId { get; set; }

        [ForeignKey("User")]
        public Guid UserId { get; set; }
        public User User{ get; set; }

        [ForeignKey("Review")]
        public Guid ReviewId { get; set; }
        public Review Review { get; set; }

        public string CommentContent { get; set; }
    }
}
