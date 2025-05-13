using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;
using System.Threading.Tasks;

namespace Parkeringsplads.Services.EFServices
{
    public class EFRequestService : IRequestService
    {
        private readonly ParkeringspladsContext _context;

        public EFRequestService(ParkeringspladsContext context)
        {
            _context = context;
        }

        public async Task<List<Request>> GetAllRequestsForUser(User user)

        {

            return await _context.Request
           .Where(r => r.UserId == user.UserId)
        .Include(r => r.Trip)
            .ThenInclude(t => t.Driver)
                .ThenInclude(d => d.User)
        .ToListAsync();
        

        }

        public async Task DeleteRequestAsync(int requestId)
        {
            var request = await _context.Request.FindAsync(requestId);
            if (request != null)
            {
                _context.Request.Remove(request);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Request> GetRequestByIdAsync(int requestId)
        {
            return await _context.Request
                .Include(r => r.Trip)
                    .ThenInclude(t => t.Driver)
                        .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(r => r.RequestId == requestId);
        }



    }
}
