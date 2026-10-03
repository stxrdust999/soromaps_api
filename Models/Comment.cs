namespace soromaps_api.Models
{
    public class Comment
    {
        public Guid CommentId { get; set; }
        public Guid UserId { get; set; }
        public Guid ReviewId { get; set; }

        public string CommentContent { get; set; }
    }
}
