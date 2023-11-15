using ProductManagement.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProductManagement.Models;
using ProductManagement.Controllers;
using FluentAssertions;
using ProductManagement.Tests.Helpers;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;

namespace ProductManagement.Tests.Controllers
{
    public class ProductControllerTests : IntegrationTest
    {
        public ProductControllerTests(IntegrationTestWebAppFactory factory) : base(factory)
        {
        }

        private string jwtToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6InN0cmluZUBtYWlsLnJ1IiwiSWQiOiIxIiwiUm9sZSI6ImRldiIsIm5iZiI6MTcwMDA3NDQyNywiZXhwIjoxNzAwMDc2MjI3LCJpYXQiOjE3MDAwNzQ0Mjd9.w54U8MFg_qcBrasS9O50vhywpUT-ILFIOQyLRLkbifo";

        [Fact]
        public async Task ProductController_GetProducts_ReturnProduct()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                //Arrange
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ProductDbContext>();
                Utilities.ReinitializeDbForTests(db);

                //Act
                List<Product>? products = await _client.GetFromJsonAsync<List<Product>>("/api/Product");

                //Assert
                Assert.True(products?[0].ProductName == "Rock");
            }
        }

        [Fact]
        public async Task ProductController_GetProductsWithFilter_ReturnEmpty()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                //Arrange
                //_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ProductDbContext>();
                Utilities.ReinitializeDbForTests(db);

                //Act
                List<Product>? products = await _client.GetFromJsonAsync<List<Product>>("/api/Product/?searchTerm=f");

                //Assert
                Assert.Empty(products);
            }
        }

        [Fact]
        public async Task ProductController_GetById_ReturnProduct()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                //Arrange
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ProductDbContext>();
                Utilities.ReinitializeDbForTests(db);

                //Act
                var responce = await _client.GetFromJsonAsync<Product>("api/Product/1");

                //Assert
                Assert.True(responce?.ProductName == "Rock");
            }
        }

        [Fact]
        public async Task ProductController_Create_NewProductToDbAndFullfillUserId()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                //Arrange
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ProductDbContext>();

                var product = new Product();
                product.ProductName = "Rock";
                product.Description = "Just rock... Nothing special";
                product.Price = 300;
                product.IsAvailable = true;

                JsonContent jsonProduct = JsonContent.Create(product);

                //Act
                var responce = await _client.PostAsync("/api/Product", jsonProduct);
                var products = await db.Products.ToListAsync();

                //Assert
                responce.EnsureSuccessStatusCode();
                Assert.NotEmpty(products);
                Assert.True(products[0].UserId == 1);
            }
        }

        [Fact]
        public async Task ProductController_Delete_ShouldEmptyDb()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                //Arrange
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<ProductDbContext>();
                Utilities.ReinitializeDbForTests(db);

                var products0 = await db.Products.ToListAsync();

                //Act
                var responce = await _client.DeleteAsync("api/Product/3");
                await db.SaveChangesAsync();
                var products = await db.Products.ToListAsync();

                //Assert
                responce.EnsureSuccessStatusCode();
                Assert.Empty(products);
            }
        }
    }
}
