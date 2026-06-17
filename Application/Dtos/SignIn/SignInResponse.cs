using Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Dtos.SignIn
{
    public class SignInResponse
    {
        public string? Token { get; set; }
        public string? Message { get; set; }

    }
}
