using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AttendanceManagement.Application.Contracts.Lookups;
using AttendanceManagement.Application.Interfaces.Services;
using AttendanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Infrastructure.Services;

internal class LookupService : ILookupService
{
    private readonly AttendanceDbContext _dbContext;

    public LookupService(AttendanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<CountryDto>> GetCountriesAsync(CancellationToken cancellationToken = default)
    {
        var countries = await _dbContext.Countries
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        return countries.Select(c => new CountryDto(c.Id, c.Name));
    }

    public async Task<IEnumerable<CityDto>> GetCitiesByCountryAsync(int countryId, CancellationToken cancellationToken = default)
    {
        var cities = await _dbContext.Cities
            .AsNoTracking()
            .Where(c => c.CountryId == countryId)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        return cities.Select(c => new CityDto(c.Id, c.Name, c.CountryId));
    }
}
