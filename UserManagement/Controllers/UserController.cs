using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using UserManagement.Models;
using UserManagement.Repositories;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Azure.Core;
using MimeKit;
using MimeKit.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using UserManagement.Services;

namespace UserManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private const string JWT_SECURITY_KEY = "sOU4NBaGhcXHqxeOTbp7EclOOndeTvgi";
        private const int JWT_TOKEN_VALIDITY_MINS = 30;

        private readonly UserDbContext _userDbContext;
        private readonly IEmailService _emailService;

        public UserController(UserDbContext userDbContext, IEmailService emailService)
        {
            _userDbContext = userDbContext;
            _emailService = emailService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<User>> GetUser()
        {
            return _userDbContext.Users;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<User>> GetById(int id)
        {
            var user = await _userDbContext.Users.FindAsync(id);

            if (user is null)
            {
                return NotFound();
            }

            return user;
        }

        [HttpPost]
        public async Task<ActionResult> Create(User user)
        {
            if (_userDbContext.Users.Any(u => u.EmailAddress == user.EmailAddress && u.IsVerified == true))
            {
                return BadRequest("Something went wrong when creating the user, check the entered data");
            }

            user.Password = CreatePasswordHash(user.Password, out byte[] passwordSalt);
            user.PasswordSalt = passwordSalt;

            do
            {
                user.VerificationToken = GenerateRandomCode();
            }
            while (_userDbContext.Users.Any(u => u.VerificationToken == user.VerificationToken));
            
            await _userDbContext.Users.AddAsync(user);
            await _userDbContext.SaveChangesAsync();

            EmailDto emailDto = new()
            {
                To = user.EmailAddress,
                Subject = "Email confirmation",
                Body = $"Please, confirm your email address by entering this code {user.VerificationToken}"
            };

            await _emailService.SendEmail(emailDto);

            return Ok();
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] AuthenticationRequest request)
        {

            var userAccount = await _userDbContext.Users.FirstOrDefaultAsync(account => account.EmailAddress == request.Email && account.IsVerified == true);

            if (userAccount == null)
            {
                return Unauthorized("Something went wrong. Try to check the correctness of the entered data or complete account verification");
            }
            if (!VerifyPasswordHash(request.Password, userAccount.Password, userAccount.PasswordSalt))
            {
                return Unauthorized("Invalid login or password");
            }

            var token = GenerateJwtToken(userAccount);

            return Ok(token);
        }

        [HttpPost("verify-email")]
        public async Task<ActionResult> VerifyEmail(int token)
        {
            var userAccount = await _userDbContext.Users.FirstOrDefaultAsync(account => account.VerificationToken == token);

            if (userAccount == null)
            {
                return BadRequest();
            }

            userAccount.VerificationToken = null;
            userAccount.IsVerified = true;

            return Ok("Verification was successful");
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult> ForgotPassword(string email)
        {
            var userAccount = await _userDbContext.Users.FirstOrDefaultAsync(account => account.EmailAddress == email);

            if (userAccount == null)
            {
                return BadRequest();
            }

            userAccount.PasswordResetToken = GenerateRandomCode();
            userAccount.ResetTokenExpires = DateTime.UtcNow.AddMinutes(5);

            await _userDbContext.SaveChangesAsync();

            EmailDto emailDto = new()
            {
                To = email,
                Subject = "Password reset code",
                Body = $"Please, enter this code to reset your password {userAccount.PasswordResetToken}"
            };

            await _emailService.SendEmail(emailDto);

            return Ok("An email has been sent");
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult> ResetPassword(ResetPasswordRequest request)
        {
            var userAccount = await _userDbContext.Users.FirstOrDefaultAsync(account => account.PasswordResetToken == request.ResetToken);

            if (userAccount == null || userAccount.ResetTokenExpires >= DateTime.UtcNow)
            {
                return BadRequest("Invalid token");
            }

            userAccount.Password = CreatePasswordHash(request.Password, out byte[] passwordSalt);
            userAccount.PasswordSalt = passwordSalt;
            userAccount.PasswordResetToken = null;
            userAccount.ResetTokenExpires = null;

            await _userDbContext.SaveChangesAsync();

            return Ok("Succesfully reset password");
        }

        private string GenerateJwtToken(User userAccount)
        {
            var tokenExpiryTimeStamp = DateTime.Now.AddMinutes(JWT_TOKEN_VALIDITY_MINS);
            var tokenKey = Encoding.ASCII.GetBytes(JWT_SECURITY_KEY);
            var claimsIdentity = new ClaimsIdentity(new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Email, userAccount.EmailAddress),
                new Claim("Role", userAccount.Role)
            });

            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(tokenKey),
                SecurityAlgorithms.HmacSha256Signature);

            var securityTokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                Expires = tokenExpiryTimeStamp,
                SigningCredentials = signingCredentials
            };

            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var securityToken = jwtSecurityTokenHandler.CreateToken(securityTokenDescriptor);
            var token = jwtSecurityTokenHandler.WriteToken(securityToken);

            return token;
        }

        private string CreatePasswordHash(string password, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                byte[] passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                password = Encoding.UTF8.GetString(passwordHash);

                return password;
            }
        }

        private bool VerifyPasswordHash(string password, string passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                var translatedHash = Encoding.UTF8.GetString(computedHash);

                return translatedHash.Equals(passwordHash);
            }
        }

        private int GenerateRandomCode()
        {
            return RandomNumberGenerator.GetInt32(100000, 1000000);
        }

        [HttpPut]
        public async Task<ActionResult> Update(User user)
        {
            if (!_userDbContext.Users.Any(existingUser => existingUser.Id == user.Id))
            {
                return NotFound();
            }

            _userDbContext.Users.Update(user);
            await _userDbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var user = await _userDbContext.Users.FindAsync(id);

            if (user is null)
            {
                return NotFound();
            }

            _userDbContext.Users.Remove(user);
            await _userDbContext.SaveChangesAsync();

            return Ok();
        }
    }
}
