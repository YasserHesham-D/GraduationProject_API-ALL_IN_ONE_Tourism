using Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services.AccountServices
{
    public interface IAccountServices
    {
        Task<SignInResponse> SignInAsync(User user);
    }
}
