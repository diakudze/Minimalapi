using MagicVilla_CouponAPI.Data;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace MagicVilla_CouponAPI.Repository
{
    public class CouponRepository : ICouponRepository
    {
        private readonly ApplicationDbContext _db;

        public CouponRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task CreateAsync(Coupon coupon)
        {
            await _db.Coupons.AddAsync(coupon);
        }

        public async Task<ICollection<Coupon>> GetAllAsync()
        {
            return await _db.Coupons.ToListAsync(); 
        }

        public async Task<Coupon> GetAsync(int id)
        {
            return await _db.Coupons.FirstOrDefaultAsync(u => u.Id == id); 
        }

        public async Task<Coupon> GetAsync(string couponName)
        {
            return await _db.Coupons.FirstOrDefaultAsync(u => u.Name.ToLower() == couponName.ToLower());
        }

        public Task RemoveAsync(Coupon coupon)
        {
            _db.Coupons.Remove(coupon);
            return Task.CompletedTask;
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }

        public Task UpdateAsync(Coupon coupon)
        {
            // Проверяем состояние сущности
            var entry = _db.Entry(coupon);
            if (entry.State == EntityState.Detached)
            {
                // Если сущность не отслеживается, прикрепляем ее
                _db.Coupons.Attach(coupon);
            }
            // Помечаем сущность как измененную
            entry.State = EntityState.Modified;
            return Task.CompletedTask;
        }
    }
}
