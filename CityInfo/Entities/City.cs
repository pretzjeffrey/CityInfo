
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CityInfo.Models;

namespace CityInfo.Entities
{
    public class City
    {
        [Key] // unnecessary if the property is named "Id" or "CityId", but it's good practice to be explicit
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // unnecessary if the property is named "Id" or "CityId", but it's good practice to be explicit
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }

        public ICollection<PointOfInterest> PointsOfInterest { get; set; } = new List<PointOfInterest>(); // avoid null reference exceptions by initializing the collection in the constructor

        public City(string name)
        {
            Name = name;
        }
    }
}
