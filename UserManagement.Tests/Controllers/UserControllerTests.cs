using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Data;
using UserManagement.Dtos;
using UserManagement.Models;
using UserManagement.Services;
using UserManagement.Tests.Helpers;

namespace UserManagement.Tests.Controllers
{
    public class UserControllerTests : IntegrationTest
    {

        public UserControllerTests(IntegrationTestWebAppFactory factory) : base(factory) 
        {
        }

        [Fact]
        public async Task UserController_GetUser_ReturnUser()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                //Arrange
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<UserDbContext>();
                Utilities.ReinitializeDbForTests(db);

                //Act
                List<UserDto>? users = await _client.GetFromJsonAsync<List<UserDto>>("/api/User");

                //Assert
                Assert.True(users?[0].EmailAddress == "strine@mail.ru");
            }
        }

        [Fact]
        public async Task UserController_GetById_ShouldReturnUser()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                //Arrange
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<UserDbContext>();
                Utilities.ReinitializeDbForTests(db);

                var users0 = await db.Users.ToListAsync();
                //Act
                var response = await _client.GetFromJsonAsync<UserDto>("api/User/2");

                //Assert
                Assert.True(response?.Name == "Strine");
            }
        }

        [Fact]
        public async Task UserController_Create_ShouldAddNewUserToDb()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                //Arrange
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<UserDbContext>();

                var user = new User();
                user.Name = "Strine";
                user.EmailAddress = "strine@mail.ru";
                user.Password = "strine";
                user.Role = "dev";

                JsonContent jsonUser = JsonContent.Create(user);

                //Act
                var response = await _client.PostAsync("/api/User", jsonUser);
                var users = await db.Users.ToListAsync();

                //Assert
                response.EnsureSuccessStatusCode();
                Assert.NotEmpty(users);
            }

        }

        [Fact]
        public async Task UserController_Login_ShouldReturnOk()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                //Arrange
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<UserDbContext>();

                var user = new User();
                user.Name = "Strine";
                user.EmailAddress = "strine@mail.ru";
                user.Password = "strine";
                user.Role = "dev";
                user.IsVerified = true;

                await _client.PostAsJsonAsync("api/User", user);

                AuthenticationRequest auth = new();
                auth.Email = "strine@mail.ru";
                auth.Password = "strine";

                //Act
                var response = await _client.PostAsJsonAsync("api/User/login", auth);

                //Assert
                response.EnsureSuccessStatusCode();
            }
        }

        [Fact]
        public async Task UserController_DeleteUser_ShouldEmptyDb()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                //Arrange
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<UserDbContext>();
                Utilities.ReinitializeDbForTests(db);
                
                //Act
                var response = await _client.DeleteAsync("api/User/5");
                await db.SaveChangesAsync();
                var users = await db.Users.ToListAsync();

                //Assert
                response.EnsureSuccessStatusCode();
                Assert.Empty(users);
            }
        }
    }
}
