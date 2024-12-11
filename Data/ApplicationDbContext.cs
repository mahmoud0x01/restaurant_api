using Mahmoud_Restaurant.Models;
using Microsoft.EntityFrameworkCore;


namespace Mahmoud_Restaurant.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Dish> Dishes { get; set; }  // gen db table
    }
}
