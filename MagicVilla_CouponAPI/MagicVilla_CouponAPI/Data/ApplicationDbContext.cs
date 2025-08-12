using Microsoft.EntityFrameworkCore;
using MagicVilla_CouponAPI.Models;

namespace MagicVilla_CouponAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Coupon> Coupons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Coupon>(entity =>
            {
                entity.Property(c => c.Created)
                    .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
                
                entity.Property(c => c.LastUpdated)
                    .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
                
                entity.HasData(
                    new Coupon
                    {
                        Id = 1,
                        Name = "10OFF",
                        Percent = 10,
                        IsActive = true,
                        Created = DateTime.UtcNow,
                        LastUpdated = DateTime.UtcNow
                    },
                    new Coupon
                    {
                        Id = 2,
                        Name = "20OFF",
                        Percent = 20,
                        IsActive = true,
                        Created = DateTime.UtcNow,
                        LastUpdated = DateTime.UtcNow
                    },
                    new Coupon
                    {
                        Id = 3,
                        Name = "30OFF",
                        Percent = 14,
                        IsActive = true,
                        Created = DateTime.UtcNow,
                        LastUpdated = DateTime.UtcNow
                    }
                );
            });
            }
        }
    }
