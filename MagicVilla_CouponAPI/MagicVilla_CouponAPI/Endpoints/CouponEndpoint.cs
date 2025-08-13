using AutoMapper;
using FluentValidation;
using MagicVilla_CouponAPI.Models.DTO;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Repository.IRepository;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Azure;
using MagicVilla_CouponAPI;



namespace MagicVilla_CouponAPI.Endpoints

{
    public static class CouponEndpoints
    {


        public static void ConfigureCouponEndpoints(this WebApplication app)
        {

            // Получение списка всех купонов
            app.MapGet("/api/coupon", GetAllCoupon).WithName("GetCoupons").Produces<APIResponse>(200);

            // Получение купона по его идентификатору
            app.MapGet("/api/coupon/{id:int}", async (ICouponRepository _couponRepo, ILogger<Program> _logger, int id) =>
            {
                APIResponse response = new APIResponse();
                Coupon coupon = await _couponRepo.GetAsync(id);

                if (coupon == null)
                {
                    response.IsSuccess = false;
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.ErrorMessages.Add($"Coupon with id {id} not found.");
                    _logger.LogWarning("Coupon with id {id} not found.", id);
                    return Results.NotFound(response);
                }

                response.Result = coupon;
                response.IsSuccess = true;
                response.StatusCode = HttpStatusCode.OK;
                _logger.LogInformation("Retrieved coupon with id {id}.", id);
                return Results.Ok(response);
            }).WithName("GetCoupon").Produces<APIResponse>(200).Produces(404);

            // Создание нового купона
            app.MapPost("/api/coupon", async (
                ICouponRepository _couponRepo,
                [FromServices] IMapper _mapper,
                [FromServices] IValidator<CouponCreateDTO> _validation,
                [FromBody] CouponCreateDTO coupon_C_DTO) =>
            {

                APIResponse response = new APIResponse() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };

                var validationResult = await _validation.ValidateAsync(coupon_C_DTO);

                if (!validationResult.IsValid)
                {
                    response.ErrorMessages.AddRange(validationResult.Errors
                        .Select(error => error.ErrorMessage)
                        .Where(m => !string.IsNullOrEmpty(m)));
                    return Results.BadRequest(response);
                }

                if (await _couponRepo.GetAsync(coupon_C_DTO.Name) != null)
                {
                    response.ErrorMessages.Add("Coupon name already exists");
                    return Results.BadRequest(response);
                }

                Coupon coupon = _mapper.Map<Coupon>(coupon_C_DTO);
                coupon.Created = DateTime.UtcNow;
                coupon.LastUpdated = DateTime.UtcNow;
                await _couponRepo.CreateAsync(coupon);
                await _couponRepo.SaveAsync();

                CouponDTO couponDTO = _mapper.Map<CouponDTO>(coupon);
                couponDTO.URL = $"/api/coupon/{coupon.Id}";

                response.Result = couponDTO;
                response.IsSuccess = true;
                response.StatusCode = HttpStatusCode.Created;
                return Results.Created(couponDTO.URL, response);

            })
            .WithName("CreateCoupon")
            .Accepts<CouponCreateDTO>("application/json")
            .Produces<APIResponse>(201)
            .Produces(400);

            // Обновление существующего купона
            app.MapPut("/api/coupon", async (
                ICouponRepository _couponRepo,
                [FromServices] IMapper _mapper,
                [FromServices] IValidator<CouponPutDTO> _validation,
                [FromBody] CouponPutDTO coupon_P_DTO) =>
            {
                APIResponse response = new APIResponse() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };

                var validationResult = await _validation.ValidateAsync(coupon_P_DTO);
                if (!validationResult.IsValid)
                {
                    response.ErrorMessages.AddRange(validationResult.Errors
                        .Select(error => error.ErrorMessage)
                        .Where(m => !string.IsNullOrEmpty(m)));
                    return Results.BadRequest(response);
                }

                Coupon coupon = await _couponRepo.GetAsync(coupon_P_DTO.Id);
                if (coupon == null)
                {
                    response.ErrorMessages.Add($"Coupon with ID: {coupon_P_DTO.Id} not found.");
                    response.StatusCode = HttpStatusCode.NotFound;
                    return Results.NotFound(response);
                }

                // Check for duplicate name (excluding the current coupon)
                Coupon existingCouponWithName = await _couponRepo.GetAsync(coupon_P_DTO.Name);
                if (existingCouponWithName != null && existingCouponWithName.Id != coupon_P_DTO.Id)
                {
                    response.ErrorMessages.Add("A coupon with that name already exists.");
                    return Results.BadRequest(response);
                }

                // Update the existing coupon using AutoMapper
                _mapper.Map(coupon_P_DTO, coupon);
                coupon.LastUpdated = DateTime.UtcNow;

                // Update the coupon in repository
                await _couponRepo.UpdateAsync(coupon);
                await _couponRepo.SaveAsync();

                // Map to DTO to return
                CouponDTO coupon_U_DTO = _mapper.Map<CouponDTO>(coupon);
                coupon_U_DTO.URL = $"/api/coupon/{coupon_U_DTO.Id}";
                response.IsSuccess = true;
                response.StatusCode = HttpStatusCode.OK;
                response.Result = coupon_U_DTO;
                return Results.Ok(response);
            })
            .WithName("PutCoupon")
            .Accepts<CouponPutDTO>("application/json")
            .Produces<APIResponse>(200)
            .Produces(400)
            .Produces(404);

            // Удаление существующего купона
            app.MapDelete("/api/coupon/{id:int}", async (ICouponRepository _couponRepo, int id) =>
            {

                APIResponse response = new APIResponse() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };
                Coupon coupon = await _couponRepo.GetAsync(id);

                if (coupon == null)
                {
                    response.ErrorMessages.Add($"Coupon with ID: {id} not found.");
                    response.StatusCode = HttpStatusCode.NotFound;
                    return Results.NotFound(response);
                }

                await _couponRepo.RemoveAsync(coupon);
                await _couponRepo.SaveAsync();

                response.IsSuccess = true;
                response.StatusCode = HttpStatusCode.OK;
                return Results.Ok(response);

            })
            .WithName("DeleteCoupon")
            .Produces<APIResponse>(200)
            .Produces(400)
            .Produces(404);

        }

        private async static Task<IResult> GetAllCoupon(ICouponRepository _couponRepo, ILogger<Program> _logger)
        {
            APIResponse response = new APIResponse();
            _logger.Log(LogLevel.Information, "Getting all Coupons");
                response.Result = await _couponRepo.GetAllAsync();
            response.IsSuccess = true;
                response.StatusCode = HttpStatusCode.OK;
                return Results.Ok(response);
        }
            

}
}