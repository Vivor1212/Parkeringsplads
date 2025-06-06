﻿using Microsoft.AspNetCore.Mvc.Rendering;
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
            return await _context.School
                .Select(s => new SelectListItem
                {
                    Value = s.SchoolId.ToString(),
                    Text = s.SchoolName
                }).ToListAsync();
        }

        public async Task CreateSchoolAsync(string schoolName, string addressRoad, string addressNumber, int cityId)
        {
            var newAddress = new Address
            {
                AddressRoad = addressRoad,
                AddressNumber = addressNumber,
                CityId = cityId
            };

            _context.Address.Add(newAddress);
            await _context.SaveChangesAsync(); 

            var newSchool = new School
            {
                SchoolName = schoolName,
                AddressId = newAddress.AddressId
            };

            _context.School.Add(newSchool);
            await _context.SaveChangesAsync(); 
        }

        public async Task DeleteSchoolAsync(int schoolId)
        {
            var school = await _context.School
                .Include(s => s.Address)
                .FirstOrDefaultAsync(s => s.SchoolId == schoolId);

            if (school == null)
            {
                throw new Exception("Skolen blev ikke fundet.");
            }

            _context.School.Remove(school);

            var addressInUse = await _context.School
                .AnyAsync(s => s.AddressId == school.AddressId && s.SchoolId != schoolId);

            if (!addressInUse)
            {
                _context.Address.Remove(school.Address);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<School?> GetSchoolWithAddressAsync(int schoolId)
        {
            return await _context.School.Include(s => s.Address).ThenInclude(a => a.City).FirstOrDefaultAsync(s => s.SchoolId == schoolId);
        }

        public async Task<bool> UpdateSchoolAsync(int schoolId, string schoolName, int addressId)
        {
            var school = await _context.School.FirstOrDefaultAsync(s => s.SchoolId == schoolId);
            if (school == null)
            {
                return false;
            }

            school.SchoolName = schoolName;
            school.AddressId = addressId;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<School>> GetSchoolsWithAddressAsync(string? serachTerm = null)
        {
            var query = _context.School.Include(s => s.Address).ThenInclude(a => a.City).AsQueryable();
            if (!string.IsNullOrWhiteSpace(serachTerm))
            {
                var lower = serachTerm.ToLower();
                query = query.Where(s => s.SchoolName.ToLower().Contains(lower) ||
                s.Address.AddressRoad.ToLower().Contains(lower) ||
                s.Address.AddressNumber.ToLower().Contains(lower) ||
                s.Address.City.CityName.ToLower().Contains(lower) ||
                s.Address.City.PostalCode.Contains(lower));
            }

            return await query.ToListAsync();
        }
    }
}
