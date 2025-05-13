using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;

namespace Parkeringsplads.Services.EFServices
{
    public class EFSchoolService : ISchoolService
    {

        private readonly ParkeringspladsContext _context;
        public EFSchoolService(ParkeringspladsContext context)
        {
            _context = context;
        }


        public async Task<List<SelectListItem>> SchoolDropDownAsync()
        {
            // Til brug hvor der er skole dropdowns, metoden sørger for at skoler bliver vist i dropdown, se eksempel på createuser
            return await _context.School
                .Select(s => new SelectListItem
                {
                    Value = s.SchoolId.ToString(),
                    Text = s.SchoolName
                }).ToListAsync();
        

        }
    }
}
