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
        private readonly string secretKey;

        public AuthRepository(ApplicationDbContext db, IMapper mapper, IConfiguration configuration)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
            secretKey = _configuration.GetValue<string>("ApiSettings:Secret");
        }

        public bool IsUniqueUser(string username)
        {
            var user = _db.LocalUsers.FirstOrDefault(u => u.UserName == username);

            if (user == null)
            {
                return true;
            }
            return false;
        }

        public Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO)
        {
            var user = _db.LocalUsers.FirstOrDefault(u => u.UserName == loginRequestDTO.UserName && u.Password == loginRequestDTO.Password);

            //User not found
            if (user == null)
            {
                return null;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                                    {
                                        new Claim(ClaimTypes.Name, user.UserName),
                                        new Claim(ClaimTypes.Role, user.Role),
                                    }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            LoginResponseDTO loginResponseDTO = new()
            {
                User = _mapper.Map<UserDTO>(user),
                Token = new JwtSecurityTokenHandler().WriteToken(token)
            };
            return Task.FromResult(loginResponseDTO);
            
        }


        public async Task<UserDTO> Register(RegistrationRequestDTO requestDTO)
        {
            LocalUser userObj = new()
            {
                UserName = requestDTO.UserName,
                Password = requestDTO.Password,
                Name = requestDTO.Name,
                Role = "admin"
            };

            await _db.LocalUsers.AddAsync(userObj);
            await _db.SaveChangesAsync();
            userObj.Password = "";
            return _mapper.Map<UserDTO>(userObj);
  
        }

    }
}