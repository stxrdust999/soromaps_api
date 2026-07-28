using System.ComponentModel.DataAnnotations.Schema;

namespace Soromaps.Models
{

    [Table("markers")]
    public class Marker
    {
        [Column("id")]
        public int Id { get; set; }
        
        [Column("nome")]
        public string Nome { get; set; }
        
        [Column("lat")]
        public double Lat { get; set; }
        
        [Column("lng")]
        public double Lng { get; set; }

    }
}
