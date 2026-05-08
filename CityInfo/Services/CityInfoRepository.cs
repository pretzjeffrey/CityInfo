using CityInfo.DbContexts;
using CityInfo.Entities;
using Microsoft.EntityFrameworkCore;

namespace CityInfo.Services
{
    public class CityInfoRepository : ICityInfoRepository
    {
        private readonly CityInfoContext _context;

        public CityInfoRepository(CityInfoContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<City>> GetCitiesAsync()
        {
            return await _context.Cities.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<bool> CityNameMatchesCityId(string? cityName, int cityId)
        {
            return await _context.Cities.AnyAsync(c => c.Id == cityId && c.Name == cityName);
        }

        // use a tuple to return more than one value easily
        public async Task<(IEnumerable<City>, PaginationMetadata)> GetCitiesAsync(string? name, string? searchQuery, int pageNumber, int pageSize)
        {
            // for the line below,  this is for deferred execution, meaning that the query won't be executed
            // against the database until we actually enumerate over the collection (e.g., by calling ToListAsync),
            // which allows us to build up the query dynamically based on the provided parameters.
            var collection = _context.Cities as IQueryable<City>; // Start with the full collection of cities as an IQueryable to allow for deferred execution and efficient querying.

            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim();
                collection = collection.Where(c => c.Name == name); // Filter the collection to include only cities with the specified name
            }

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.Trim();

                // Filter the collection to include only cities or descriptions that match the search query
                collection = collection.Where(c => c.Name.Contains(searchQuery) 
                || (c.Description != null && c.Description.Contains(searchQuery))); 
            }

            var totalItemCount = await collection.CountAsync(); // Get the total count of items in the filtered collection to calculate pagination metadata.
            var paginationMetadata = new PaginationMetadata(totalItemCount, pageSize, pageNumber); // Create a PaginationMetadata object to hold the pagination information based on
                                                                                                   // the total item count, pageSize and pageNumber.

            // we should add paging just before the deferred execution (ToListAsync) to ensure that the paging is applied to the filtered results, not the entire collection.
            var collectionToReturn =  await collection.OrderBy(c => c.Name) // Order the collection by city name to ensure consistent ordering of results across pages.
                .Skip(pageSize * (pageNumber -1)) // Skip the appropriate number of records based on the page number and page size to implement pagination.
                .Take(pageSize) // Take the specified number of records for the current page to implement pagination.
                .ToListAsync(); // Execute the query against the database and return the results as a list of cities.

            return (collectionToReturn, paginationMetadata); // Return the filtered and paginated collection of cities along with the pagination metadata as a tuple.
        }

        public async Task<City?> GetCityAsync(int cityId, bool includePointsOfInterest)
        {
            if (includePointsOfInterest)
            {
                return await _context.Cities.Include(c => c.PointsOfInterest)
                    .FirstOrDefaultAsync(c => c.Id == cityId);
            }
            else
            {
                return await _context.Cities.FirstOrDefaultAsync(c => c.Id == cityId);
            }
        }

        public async Task<PointOfInterest?> GetPointOfInterestForCityAsync(int cityId, int pointOfInterestId)
        {
            return await _context.PointsOfInterest
                .Where(p => p.CityId == cityId && p.Id == pointOfInterestId)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<PointOfInterest>> GetPointsOfInterestForCityAsync(int cityId)
        {
            return await _context.PointsOfInterest
                .Where(p => p.CityId == cityId)
                .ToListAsync();
        }

        public async Task AddPointOfInterestForCityAsync(int cityId, PointOfInterest pointOfInterest)
        {
            var city = await GetCityAsync(cityId, false);
            if (city != null)
            {
                city.PointsOfInterest.Add(pointOfInterest); // AddAsync isn't needed here because this is in memory, not an IO database operation.
                                                            // EF Core will track this change and save it to the database when SaveChangesAsync is called.
            }
        }

        public void DeletePointOfInterest(PointOfInterest pointOfInterest)
        {
            _context.PointsOfInterest.Remove(pointOfInterest); // Remove is used to mark the entity for deletion. The actual deletion happens when SaveChangesAsync is called.
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync() >= 0); // SaveChangesAsync returns the number of state entries written to the database,
                                                             // so we check if it's greater than or equal to 0 to determine if the save was successful.
        }


        public async Task<bool> CityExistsAsync(int cityId)
        {
            return await _context.Cities.AnyAsync(c => c.Id == cityId);
        }
    }
}
 