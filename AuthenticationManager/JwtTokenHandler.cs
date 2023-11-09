using AuthenticationManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationManager
{
    public class JwtTokenHandler
    {
        public const string JWT_SECURITY_KEY = "nMFtfY0AOynmyM1WhbaRzYiPWhZR4s6r";
        private const int JWT_TOKEN_VALIDITY_MINS = 30;

        JwtTokenHandler() 
        {

        }

        public AuthenticationResponce? GenerateJwtToken(AuthenticationRequest authenticationRequest)
        {
            if(string.IsNullOrWhiteSpace(authenticationRequest.UserName) || string.IsNullOrWhiteSpace(authenticationRequest.Password))
            {
                return null;
            }

            var userAccount = 
        }
    }
}
