using Promobile.Models;
using Microsoft.EntityFrameworkCore;

namespace Promobile.Data
{
    public class PromobileContext : DbContext
    {
        public PromobileContext(DbContextOptions o) : base(o)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Banner> Banners { get; set; }

        public DbSet<PromotionalProduct> PromotionalProducts { get; set; }

        public DbSet<ProductCategory> ProductCategories { get; set; }

    }
}