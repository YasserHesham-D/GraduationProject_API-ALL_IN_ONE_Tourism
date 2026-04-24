using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Dtos.SignIn
{
    public class SignInRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
