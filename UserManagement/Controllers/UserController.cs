using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Models;

namespace UserManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
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
            await _userDbContext.Users.AddAsync(user);
            await _userDbContext.SaveChangesAsync();

            return Ok();
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
