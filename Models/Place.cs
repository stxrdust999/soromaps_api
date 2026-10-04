using System.ComponentModel.DataAnnotations.Schema;

namespace soromaps_api.Models
{
    public class Place
    {
        public Guid PlaceId { get; set; }

        [ForeignKey("Category")]
        public Guid CategoryId { get; set; }
        public Category Category { get; set; }

        [ForeignKey("Author")]
        public Guid AuthorId { get; set; }
        public User Author { get; set; }

        public string PlaceName { get; set; }
        public float Lat { get; set; }
        public float Lng { get; set; }
        public string Neighborhood { get; set; }
        public string About { get; set; } 
        public string Description { get; set; } 
        public bool HasWifi { get; set; }
        public bool PetFriendly { get; set; }
        public Status Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }


    public enum Status
    {
        pending,
        returned,
        approved,
        rejected

    }
}
