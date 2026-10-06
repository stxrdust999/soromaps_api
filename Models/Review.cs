using System.ComponentModel.DataAnnotations.Schema;

namespace soromaps_api.Models
{
    public class Review
    {
        public Guid ReviewId { get; set; }

        [ForeignKey("Place")]
        public Guid PlaceId { get; set; }
        public Place Place{ get; set; }

        [ForeignKey("User")]
        public Guid UserId { get; set; }
        public User User{ get; set; }

        public string ReviewContent { get; set; }
        public float QtdStars { get; set; }

    }
}
