using Application.Dtos.SignIn;
using Application.Dtos.SignUp;
using Application.Services.AccountServices;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController(UserManager<User> userManager ,IAccountServices accountService,SignInManager<User> signInManager) : ControllerBase
    {
        [HttpPost]
        [Route("[Action]")]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid Request");

            if(await userManager.FindByEmailAsync(request.Email) != null)
                return BadRequest("Email already exists");

            if (request.Password != request.ConfirmPassword)
                return BadRequest("Passwords do not match");

            var Accesstoken = await accountService.SignUpAsync(request);

            return Ok(Accesstoken);
        }

        [HttpPost]
        [Route("[Action]")]
        public async Task<IActionResult> SignIn([FromBody] SignInRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid Request");

            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return Unauthorized("Invalid Email Or Password");

            var passwordValid = await signInManager.CheckPasswordSignInAsync(
                user,
                request.Password,
                lockoutOnFailure: true
            );

            if (!passwordValid.Succeeded)
            {
                if (passwordValid.IsLockedOut)
                    return Unauthorized("Account temporarily locked due to multiple failed login attempts.");

                return BadRequest("Invalid Email Or Password");
            }

            var Tokens = await accountService.SignInAsync(user);

            return Ok(Tokens);
        }

        [HttpGet]
        [Route("[Action]")]
        [Authorize]
        public async Task<IActionResult> TestAuth()
        {
            return Ok("You are authorized to access this endpoint.");
        }

    }
}
