using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Data;
using UserManagement.Models;

namespace UserManagement.Tests.Helpers
{
    public static class Utilities
    {
        public static void InitializeDbForTests(UserDbContext db)
        {
            db.Users.AddRange(GetSeedingUser());
            db.SaveChanges();
        }

        public static void ReinitializeDbForTests(UserDbContext db)
        {
            db.Users.RemoveRange(db.Users);
            InitializeDbForTests(db);
        }

        public static User GetSeedingUser()
        {
            var user = new User();
            user.Name = "Strine";
            user.EmailAddress = "strine@mail.ru";
            user.Password = "strine";
            user.Role = "dev";
            return user;
        }
    }
}
