using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Dtos.SignUp
{
    public class SignUpRequest
    {
       public string FullName { get; set; }
       public string Email { get; set; }
       
       public string Password { get; set; }
       public string Address { get; set; } = string.Empty;

       public string ConfirmPassword { get; set; }



    }
}
