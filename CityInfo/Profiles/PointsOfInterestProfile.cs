using AutoMapper;

namespace CityInfo.Profiles
{
    public class PointsOfInterestProfile : Profile
    {
        public PointsOfInterestProfile()
        {
            CreateMap<Entities.PointOfInterest, Models.PointOfInterestDto>();
            CreateMap<Models.PointOfInterestForCreationDto, Entities.PointOfInterest>(); // reversed because this is a post/create operation, so we want to map from the DTO to the entity.
            CreateMap<Models.PointOfInterestForUpdateDto, Entities.PointOfInterest>();
            CreateMap<Entities.PointOfInterest, Models.PointOfInterestForUpdateDto>(); // this is needed for the patch operation,
                                                                                       // because we need to map from the entity to the DTO to apply the patch document to the DTO,
                                                                                       // and then map back to the entity after applying the patch document.
        }
    }
}
