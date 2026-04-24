using Application.Dtos.SignIn;
using Application.Dtos.SignUp;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services.AccountServices
{
    public interface IAccountServices
    {
        Task<SignInResponse> SignUpAsync(SignUpRequest request);
        Task<SignInResponse> SignInAsync(User user);
    }
}
