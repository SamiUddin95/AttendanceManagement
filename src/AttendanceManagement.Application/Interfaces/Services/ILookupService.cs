using AttendanceManagement.Application.Contracts.Lookups;

namespace AttendanceManagement.Application.Interfaces.Services;

public interface ILookupService
{
    Task<IEnumerable<CountryDto>> GetCountriesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<CityDto>> GetCitiesByCountryAsync(int countryId, CancellationToken cancellationToken = default);
}
