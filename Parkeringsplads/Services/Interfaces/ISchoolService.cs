using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;

namespace Parkeringsplads.Services.Interfaces
{
    public interface ISchoolService
    {


        Task<List<SelectListItem>> SchoolDropDownAsync();

        public Task CreateSchoolAsync(string schoolName, string addressRoad, string addressNumber, int cityId);

        public Task DeleteSchoolAsync(int schoolId);

    }
}
