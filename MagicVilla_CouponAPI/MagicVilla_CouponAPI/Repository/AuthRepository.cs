using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using AutoMapper;
using Azure.Identity;
using MagicVilla_CouponAPI.Data;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTO;
using MagicVilla_CouponAPI.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;

namespace MagicVilla_CouponAPI.Repository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly string secretKey;

        public AuthRepository(ApplicationDbContext db, IMapper mapper, IConfiguration configuration,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            _userManager = userManager;
            _roleManager = roleManager;
            secretKey = _configuration.GetValue<string>("ApiSettings:Secret");
        }

        public bool IsUniqueUser(string username)
        {
            var user = _db.ApplicationUsers.FirstOrDefault(u => u.UserName == username);

            if (user == null)
            {
                return true;
            }
            return false;
        }

        public async Task<LoginResponseDTO?> Login(LoginRequestDTO loginRequestDTO)
{
    var user = _db.ApplicationUsers.FirstOrDefault(
        u => u.UserName == loginRequestDTO.UserName);
            bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);

            if (user == null || isValid == false)
            {
                return null;
                //return Task.FromResult<LoginResponseDTO?>(null); // Явно указываем тип
            }
    var roles = await _userManager.GetRolesAsync(user);

    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.ASCII.GetBytes(secretKey);

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Role, roles.FirstOrDefault()),
        }),
        Expires = DateTime.UtcNow.AddDays(7),
        SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);

    var loginResponseDTO = new LoginResponseDTO
    {
        User = _mapper.Map<UserDTO>(user),
        Token = tokenHandler.WriteToken(token)
    };

            return await Task.FromResult(loginResponseDTO).ConfigureAwait(false);
           // return LoginResponseDTO;
}


        public async Task<UserDTO> Register(RegistrationRequestDTO requestDTO)
        {
            ApplicationUser userObj = new()
            {
                UserName = requestDTO.UserName,
                NormalizedEmail = requestDTO.UserName.ToUpper(),
                Email = requestDTO.UserName,
                Name = requestDTO.Name
            };

            try
            {
                var result = await _userManager.CreateAsync(userObj, requestDTO.Password);
                if (result.Succeeded)
                {
                    if (!_roleManager.RoleExistsAsync("admin").GetAwaiter().GetResult())
                    {
                        await _roleManager.CreateAsync(new IdentityRole("admin"));
                        await _roleManager.CreateAsync(new IdentityRole("customer"));
                    }
                    await _userManager.AddToRoleAsync(userObj, "admin");

                    var user = _db.ApplicationUsers.FirstOrDefault(u => u.UserName == requestDTO.UserName);
                    return _mapper.Map<UserDTO>(user);
                }
            }
            catch (Exception e)
            {

            }
            return null;

        }

    }
}