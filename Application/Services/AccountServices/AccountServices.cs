using Application.Dtos.SignIn;
using Application.Dtos.SignUp;
using Domain.Interfaces.IModelsRepo;
using Domain.Models;
using Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Application.Services.AccountServices
{
    public class AccountServices( IAccountsRepo accountRepo, UserManager<User> userManager, IConfiguration configuration) : IAccountServices
    {
        public async Task<SignInResponse> SignInAsync(User user)
        {
            var Token = await CreateAccessToken(user);


            return new SignInResponse
            {
                Token = Token,

            };
        }

        public async Task<SignInResponse> SignUpAsync(SignUpRequest request)
        {

            var user = new User
            {
                UserName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Nationality = request.Nationality
            };
            
            var result = await userManager.CreateAsync(user,request.Password);

            if (result.Succeeded)
            {
                var role = "Customer";
                var roleResult = await userManager.AddToRoleAsync(user, role);
                

                var Token = await CreateAccessToken(user);

                return new SignInResponse
                {
                    Token = Token,
                };

            }

            else return null;
        }

        private async Task<string> CreateAccessToken(User user)
        {
            var userroles = await userManager.GetRolesAsync(user);
            var userRole = userroles.FirstOrDefault();

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName ?? string.Empty),
                new(ClaimTypes.Email, user.Email ?? string.Empty),
                new(ClaimTypes.Role, userRole ?? "Customer"),
                new(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())

            };

            var JWT = configuration.GetSection("JWT");
            // SigningCredentials
            var Key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes
                (JWT.GetSection("SigningKey").Value));

            var SigningCredential = new Microsoft.IdentityModel.Tokens.SigningCredentials(Key, SecurityAlgorithms.HmacSha256);

            // Token
            var Token = new JwtSecurityToken
            (
                claims: claims,
                issuer: JWT.GetSection("Issuer").Value,
                audience: JWT.GetSection("Audience").Value,
                expires: DateTime.Now.AddMinutes(int.Parse(JWT.GetSection("LifeTime").Value)),
                signingCredentials: SigningCredential
            );

            var JWTTOKEN = new JwtSecurityTokenHandler().WriteToken(Token);

            return JWTTOKEN;
        }

    }
}
