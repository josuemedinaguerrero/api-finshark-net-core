using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace api.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser> {
        public ApplicationDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions) {
        }

        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);

            List<IdentityRole> roles = new List<IdentityRole> {
                new IdentityRole {
                    Id = "5f7b5b0b-7e8c-4b5a-a7f2-1e3c6a9d2e4f",
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                },
                new IdentityRole {
                    Id = "9d4f2e3a-6b1c-4d5e-8f2a-2b3c6d7e8f9g",
                    Name = "User",
                    NormalizedName = "USER"
                }
            };
            
            builder.Entity<IdentityRole>().HasData(roles);
        }
    }
}
