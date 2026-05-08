using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CityInfo.Entities
{
    public class PointOfInterest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(200)]
        public string? Description { get; set; }

        [ForeignKey("CityId")] // optional if using conventions, but can be included for clarity
        public City? City { get; set; } // foreign key relationship to City; ID will be automatically created by EF Core based on this navigation property

        public int CityId { get; set; } // foreign key property to hold the ID of the related City; this is optional but can be useful for queries and updates

        public PointOfInterest(string name)
        {
            Name = name;
        }
    }
}
