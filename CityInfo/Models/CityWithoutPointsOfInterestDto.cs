namespace CityInfo.Models
{
    /// <summary>
    /// A city without points of interest. This is used to return a list of cities without the points of interest, 
    /// which can be used to reduce the amount of data returned in the response when we only need the basic information about the city.
    /// </summary>
    public class CityWithoutPointsOfInterestDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}
