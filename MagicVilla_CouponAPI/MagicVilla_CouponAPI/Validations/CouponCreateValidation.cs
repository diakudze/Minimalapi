using FluentValidation;
using MagicVilla_CouponAPI.Models.DTO;

namespace MagicVilla_CouponAPI.Validations
{
    public class CouponCreateValidation : AbstractValidator<CouponCreateDTO>
    {

        public CouponCreateValidation()
        {
            RuleFor(model => model.Name).NotEmpty().WithMessage("Coupon name is required.");
            RuleFor(model => model.Percent).InclusiveBetween(1, 100).WithMessage("Discount percent must be between 1 and 100.");
        }

    }
    
     public class CouponPutValidation : AbstractValidator<CouponPutDTO>
    {

        public CouponPutValidation()
        {
            RuleFor(model => model.Name).NotEmpty().WithMessage("Coupon name is required.");
            RuleFor(model => model.Percent).InclusiveBetween(1, 100).WithMessage("Discount percent must be between 1 and 100.");
        }
        
    }
}