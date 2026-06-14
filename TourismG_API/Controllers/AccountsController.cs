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
using System.Text.RegularExpressions;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController(UserManager<User> userManager ,IAccountServices accountService,SignInManager<User> signInManager) : ControllerBase
    {
        // sign up : any user register as customer then ask admin for being service provider from settings

        [HttpPost]
        [Route("[Action]")]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid Request");

            if ( string.IsNullOrEmpty(request.Password) || string.IsNullOrEmpty(request.FullName) || string.IsNullOrEmpty(request.Email)
                || string.IsNullOrEmpty(request.PhoneNumber) || string.IsNullOrEmpty(request.Nationality))
                return BadRequest("Fields Required");

            if(await userManager.FindByEmailAsync(request.Email) != null)
                return BadRequest("Email already exists");

            if (request.Password != request.ConfirmPassword)
                return BadRequest("Passwords do not match");


            var pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$";

            var result = Regex.IsMatch(request.Password, pattern);

            if (!result)
                return BadRequest("Password must be at least 8 characters long and include uppercase, lowercase, digit, and special character.");

            var Accesstoken = await accountService.SignUpAsync(request);
            
            if (Accesstoken == null)
            {
                return StatusCode(500, "Server Error");
            }

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
                return BadRequest("Email And Password Required");

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

            if (Tokens.Message == "success") 
            return Ok(Tokens);

            return StatusCode(500, "ServerError");
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
            return user is null ? NotFound() : Ok(new { username = user.UserName });
        }

        [HttpPost]
        [Route("[Action]")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null) return NotFound("Email Not Found");

            if (!user.UserName.Equals(request.Fullname) || !user.Nationality.Equals(request.Nationality))
                return Conflict("Not Match");


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
            var Tokens = await accountService.SignInAsync(user);

            var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.ResetToken));
            var result = await userManager.ResetPasswordAsync(user, token, request.NewPassword);
            return result.Succeeded ? Ok(new {Token = Tokens , message = "Password reset successfully." }) : BadRequest(result.Errors.Select(e => e.Description));
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

    public record ForgotPasswordRequest(string Fullname ,string Email,string Nationality);
    public record ResetPasswordRequest(string Email, string ResetToken, string NewPassword, string ConfirmPassword);
    public record ChangePasswordRequest(string CurrentPassword, string NewPassword, string ConfirmPassword);
}
