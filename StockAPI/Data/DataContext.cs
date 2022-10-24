using Microsoft.EntityFrameworkCore;
using StockAPI.Model;

namespace StockAPI.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<CompanyInventory> Inventories { get; set; }
    }
}
