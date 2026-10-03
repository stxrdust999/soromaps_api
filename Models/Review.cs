namespace soromaps_api.Models
{
    public class Review
    {
        public Guid ReviewId { get; set; }
        public Guid PlaceId { get; set; }
        public Guid UserId { get; set; }

        public string ReviewContent { get; set; }
        public float QtdStars { get; set; }

    }
}
