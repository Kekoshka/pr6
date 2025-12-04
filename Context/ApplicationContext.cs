using Microsoft.EntityFrameworkCore;
using pr6.Models;
namespace pr6.Context
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions options) : base(options) 
        {
            Database.EnsureCreated();
        }
        public DbSet<User> Users { get; set; }
    }
}
