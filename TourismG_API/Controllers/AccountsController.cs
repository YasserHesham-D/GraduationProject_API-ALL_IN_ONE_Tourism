using Application.Dtos.SignIn;
using Application.Dtos.SignUp;
using Application.Services.AccountServices;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;
using System.Text;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController(UserManager<User> userManager ,IAccountServices accountService,SignInManager<User> signInManager) : ControllerBase
    {
        // sign up : any user register as customer then ask admin for being service provider from settings
        //


        [HttpPost]
        [Route("[Action]")]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest request) //  delete address , add phone number 
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

        [HttpPost]
        [Route("[Action]")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return Ok(new { message = "Logged out successfully. Remove the JWT token from the client." });
        }

        [HttpGet]
        [Route("[Action]")]
        [Authorize]
        public async Task<IActionResult> GetUsername()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = userId is null ? null : await userManager.FindByIdAsync(userId);
            return user is null ? NotFound() : Ok(new { username = user.UserName, email = user.Email });
        }

        [HttpPost]
        [Route("[Action]")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                return Ok(new { message = "If the email exists, a reset token will be generated." });
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            return Ok(new
            {
                message = "Password reset token generated.",
                resetToken = encodedToken
            });
        }

        [HttpPost]
        [Route("[Action]")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                return BadRequest("Invalid reset request.");
            }

            if (request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest("Passwords do not match.");
            }

            var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.ResetToken));
            var result = await userManager.ResetPasswordAsync(user, token, request.NewPassword);
            return result.Succeeded ? Ok(new { message = "Password reset successfully." }) : BadRequest(result.Errors.Select(e => e.Description));
        }

        [HttpPost]
        [Route("[Action]")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest("Passwords do not match.");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = userId is null ? null : await userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return NotFound();
            }

            var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            return result.Succeeded ? Ok(new { message = "Password changed successfully." }) : BadRequest(result.Errors.Select(e => e.Description));
        }

    }

    public record ForgotPasswordRequest(string Email);
    public record ResetPasswordRequest(string Email, string ResetToken, string NewPassword, string ConfirmPassword);
    public record ChangePasswordRequest(string CurrentPassword, string NewPassword, string ConfirmPassword);
}
