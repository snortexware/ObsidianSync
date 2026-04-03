using G.Sync.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace G.Sync.Repository
{
    public class DatabaseInitializer(GSyncContext context) : IDatabaseInitializer
    {
        private readonly GSyncContext _context = context;

        public void Initialize()
        {
            _context.Database.Migrate();
        }
    }
}