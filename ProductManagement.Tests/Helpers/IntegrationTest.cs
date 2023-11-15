using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using ProductManagement.Tests.Controllers;
using ProductManagement.Tests.Helpers;

namespace ProductManagement.Tests.Helpers
{
    public abstract class IntegrationTest : IClassFixture<IntegrationTestWebAppFactory>
    {
        protected readonly HttpClient _client;
        protected readonly WebApplicationFactory<Program> _factory;

        protected IntegrationTest(IntegrationTestWebAppFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }
    }
}
