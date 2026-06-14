using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Dtos.SignUp
{
    public class SignUpRequest
    {
       public required string FullName { get; set; }
       public required string Email { get; set; }
       public string PhoneNumber { get; set; }
       public required string Nationality { get; set; }
       
       public required string Password { get; set; }
       public required string ConfirmPassword { get; set; }

    }
}
