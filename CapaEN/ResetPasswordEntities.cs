using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaEN
{
    public class ResetTokenRequest
    {
        public string usernameOrEmail { get; set; }
    }

    public class ResetTokenResponse
    {
        public int codigo { get; set; }
        public TokenData[] data { get; set; }
        public string mensaje { get; set; }
        public string codigoSys { get; set; }
    }

    public class TokenData
    {
        public string token { get; set; }
        public string email { get; set; }
        public DateTime expiration { get; set; }
        public bool isValid { get; set; }
    }

    public class ResetPasswordRequest
    {
        public string token { get; set; }
        public string newPassword { get; set; }
    }

    public class ResetPasswordResponse
    {
        public int codigo { get; set; }
        public string mensaje { get; set; }
        public string codigoSys { get; set; }
        public bool resultado { get; set; }
    }
}