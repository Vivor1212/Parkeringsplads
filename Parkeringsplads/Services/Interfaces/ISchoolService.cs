using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;

namespace Parkeringsplads.Services.Interfaces
{
    public interface ISchoolService
    {
        Task<List<SelectListItem>> SchoolDropDownAsync();
        Task<List<School>> GetSchoolsWithAddressAsync(string? serachTerm = null);
        Task CreateSchoolAsync(string schoolName, string addressRoad, string addressNumber, int cityId);

        Task DeleteSchoolAsync(int schoolId);
        
        Task<School?> GetSchoolWithAddressAsync(int schoolId);
        Task<bool> UpdateSchoolAsync(int schoolId, string schoolName, int addressId);
    }
}
