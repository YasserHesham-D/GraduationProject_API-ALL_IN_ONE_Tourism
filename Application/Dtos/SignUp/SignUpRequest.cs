using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Dtos.SignUp
{
    public class SignUpRequest
    {
       public string? FullName { get; set; }
       public string? Email { get; set; }
       public string? PhoneNumber { get; set; }
       public string? Nationality { get; set; }

       public string? Password { get; set; }
       public string? ConfirmPassword { get; set; }

    }
}
