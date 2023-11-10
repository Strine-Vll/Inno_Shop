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

namespace UserManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private const string JWT_SECURITY_KEY = "sOU4NBaGhcXHqxeOTbp7EclOOndeTvgi";
        private const int JWT_TOKEN_VALIDITY_MINS = 30;

        private readonly UserDbContext _userDbContext;

        public UserController(UserDbContext userDbContext)
        {
            _userDbContext = userDbContext;
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
            if(_userDbContext.Users.Any(u => u.EmailAddress == user.EmailAddress))
            {
                return BadRequest("Something went wrong when creating the user, check the entered data");
            }

            user.Password = CreatePasswordHash(user.Password, out byte[] passwordSalt);
            user.PasswordSalt = passwordSalt;

            await _userDbContext.Users.AddAsync(user);
            await _userDbContext.SaveChangesAsync();

            SendEmail();

            return Ok();
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] AuthenticationRequest request)
        {

            var userAccount = await _userDbContext.Users.FirstOrDefaultAsync(account => account.EmailAddress == request.Email);

            if (userAccount == null)
            {
                return Unauthorized("Invalid login or password");
            }
            if(!VerifyPasswordHash(request.Password, userAccount.Password, userAccount.PasswordSalt))
            {
                return Unauthorized("Invalid login or password");
            }

            var token = GenerateJwtToken(userAccount);

            return Ok(token);
        }

        [HttpPost("verifyemail")]
        public async Task<ActionResult> VerifyEmail(string token)
        {
            var userAccount = await _userDbContext.Users.FirstOrDefaultAsync(account => account.VerificationToken == token);

            if (userAccount == null)
            {
                return BadRequest();
            }

            return Ok("Verification was successful");
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

        public void SendEmail()
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse("ramiro.trantow@ethereal.email"));
            email.To.Add(MailboxAddress.Parse("ramiro.trantow@ethereal.email"));
            email.Subject = "Test";
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = "<h1>Burden</h1>"
            };

            using var smtp = new SmtpClient();
            smtp.Connect("smtp.ethereal.email", 587, SecureSocketOptions.StartTls);
            smtp.Authenticate("ramiro.trantow@ethereal.email", "WwvNff8Mg9JPa4NXd6");
            smtp.Send(email);

            smtp.Disconnect(true);
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
