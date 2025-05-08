using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;

namespace Parkeringsplads.Services.Interfaces
{
    public interface ISchoolService
    {


        Task<List<SelectListItem>> SchoolDropDownAsync();




    }
}
