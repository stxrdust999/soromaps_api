using System.ComponentModel.DataAnnotations.Schema;

namespace Soromaps.Models
{
    [Table("tbUsuario")]
    public class User
    {
            [Column("id")]
            public int Id { get; set; }
        
            [Column("user_name")]
            public string UserName { get; set; }
        
            [Column("user_email")]
            public string Email { get; set; }

            [Column("user_password")]
            public string Password { get; set; }

            [Column("created_at")]
            public DateTime CreatedAt { get; set; }

            [Column("updated_at")]
            public DateTime UpdatedAt { get; set; }

    }
}
