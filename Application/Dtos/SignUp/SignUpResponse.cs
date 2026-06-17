using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Dtos.SignUp
{

    public class SignUpResponse
    {
        public Guid? UserId { get; set; }
        public string? Message { get; set; }
    }

}
