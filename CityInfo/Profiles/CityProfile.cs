using AutoMapper;

namespace CityInfo.Profiles
{
    public class CityProfile : Profile
    {
        public CityProfile()
        {
            CreateMap<Entities.City, Models.CityWithoutPointsOfInterestDto>(); // this will automatically map properties with the same name and type. For example, Id, Name and Description will be mapped automatically.
            CreateMap<Entities.City, Models.CityDto>();
            //CreateMap<Entities.PointOfInterest, Models.PointOfInterestDto>();
        }
    }
}
