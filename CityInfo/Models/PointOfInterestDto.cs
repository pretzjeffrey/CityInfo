using System.ComponentModel.DataAnnotations;

namespace CityInfo.Models
{
    public class PointOfInterestDto
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
         
        public ICollection<PointOfInterestDto> PointsOfInterests { get; set; } = new List<PointOfInterestDto>();

        public int NumberOfPointsOfInterest
        {
            get
            {
                return PointsOfInterests.Count;
            }
        }
    }
}
