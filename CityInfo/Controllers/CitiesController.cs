using Asp.Versioning;
using AutoMapper;
using CityInfo.Models;
using CityInfo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.Controllers
{
    [ApiController]
    [Authorize] // this is the default authorization policy, which requires the user to be authenticated.
                  // You can also specify a specific policy or role here, for example [Authorize(Policy = "MustBeFromAntwerp")] or [Authorize(Roles = "Admin")].

    [Route("api/v{version:apiVersion}/cities")] // this route template includes the API version as a route parameter, which allows you to specify the version of the API in the URL when making requests.
                                                // For example, you can make a request to /api/v1/cities to access version 1 of the API, or /api/v2/cities to access version 2 of the API.
    [ApiVersion("1.0")] // this attribute specifies that this controller is part of API version 1.0.
    [ApiVersion(2)]                    // You can also specify multiple versions for the same controller, for example [ApiVersion("1.0"), ApiVersion("2.0")].
    public class CitiesController : ControllerBase
    {
        private readonly ICityInfoRepository _cityInfoRepository;
        private readonly IMapper _mapper;
        const int maxCitiesPageSize = 20;

        public CitiesController(ICityInfoRepository cityInfoRepository, IMapper mapper)
        {
            _cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<CityWithoutPointsOfInterestDto>>> GetCities(string? name, string? searchQuery, int pageNumber = 1, int pageSize = 10)
        {
            // this code is not needed because we are using AutoMapper to map the entities to DTOs
            //var cityEntities = await _cityInfoRepository.GetCitiesAsync(); 
            //var results = new List<CityWithoutPointsOfInterestDto>();
            //foreach (var cityEntity in cityEntities)
            //{
            //    results.Add(new CityWithoutPointsOfInterestDto
            //    {
            //        Id = cityEntity.Id,
            //        Name = cityEntity.Name,
            //        Description = cityEntity.Description
            //    });
            //}
            if (pageSize > maxCitiesPageSize)
            {
                pageSize = maxCitiesPageSize;
            }

            var (cityEntities, paginationMetadata) = await _cityInfoRepository.GetCitiesAsync(name, searchQuery, pageNumber, pageSize);

            Response.Headers.Add("X-Pagination", System.Text.Json.JsonSerializer.Serialize(paginationMetadata)); // add pagination metadata to response headers

            return Ok(_mapper.Map<IEnumerable<CityWithoutPointsOfInterestDto>>(cityEntities));
        }

        /// <summary>
        /// Get a city by id
        /// </summary>
        /// <param name="cityId">The id of the city to get, right?</param>
        /// <param name="includePointsOfInterest">Include points of interest yes or no?</param>
        /// <returns>A single city based on the id and whether to include points of interest.</returns>
        /// <response code="200">Successfully returns the requested city.</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet("{cityId}", Name = "GetCity")]
        public async Task<IActionResult> GetCity(int cityId, bool includePointsOfInterest = false)
        {
            var city = await _cityInfoRepository.GetCityAsync(cityId, includePointsOfInterest);
            if (city == null)
            {
                return NotFound();
            }
            if (includePointsOfInterest)
            {
                return Ok(_mapper.Map<CityDto>(city));
            }
            else
            {
                return Ok(_mapper.Map<CityWithoutPointsOfInterestDto>(city));
            }
        }

        //[HttpGet("{id}")]
        //public async ActionResult<CityDto> GetCity(int id)
        //{
        //    // find city
        //    var cityToReturn = _cityInfoRepository.GetCityAsync(id, includePointsOfInterest: true)
        //        .Where(c => c.Id == id && );

        //    if (cityToReturn == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(cityToReturn);
        //}

    }
}
