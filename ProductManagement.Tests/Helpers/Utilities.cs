using ProductManagement.Data;
using ProductManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductManagement.Tests.Helpers
{
    public static class Utilities
    {
        public static void InitializeDbForTests(ProductDbContext db)
        {
            db.Products.AddRange(GetSeedingProduct());
            db.SaveChanges();
        }

        public static void ReinitializeDbForTests(ProductDbContext db)
        {
            db.Products.RemoveRange(db.Products);
            InitializeDbForTests(db);
        }

        public static Product GetSeedingProduct()
        {
            var product = new Product();
            product.ProductName = "Rock";
            product.Description = "Just rock... Nothing special";
            product.Price = 300;
            product.IsAvailable = true;
            product.UserId = 1;

            return product;
        }
    }
}
