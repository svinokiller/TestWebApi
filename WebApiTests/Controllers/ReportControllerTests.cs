using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebApi.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static WebApi.Controllers.ReportController;
using WebApi.Models;
using Moq;

namespace WebApi.Controllers.Tests
{
    [TestClass()]
    public class ReportControllerTests
    {


        [TestMethod]
        public async Task GetQueryInfo_ReturnsNotFound_WhenQueryNotFound()
        {
            
            var options = new DbContextOptionsBuilder<ApplicationContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var dbContext = new ApplicationContext(options))
            {
                var controller = new ReportController(Mock.Of<ILogger<ReportController>>(), dbContext, Mock.Of<Microsoft.Extensions.Configuration.IConfiguration>(), Mock.Of<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>());

                
                var result = await controller.GetQueryInfo(Guid.NewGuid());

                
                Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
            }
        }

        [TestMethod]
        public async Task GetQueryInfo_ReturnsQueryInfoResponse_WhenQueryFound()
        {
            var options = new DbContextOptionsBuilder<ApplicationContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var dbContext = new ApplicationContext(options))
            {
                var queryId = Guid.NewGuid();
                var query = new Query { QueryId = queryId, Start = DateTime.UtcNow };
                dbContext.Queries.Add(query);
                dbContext.SaveChanges();

                var controller = new ReportController(Mock.Of<ILogger<ReportController>>(), dbContext, Mock.Of<Microsoft.Extensions.Configuration.IConfiguration>(), Mock.Of<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>());

                var result = await controller.GetQueryInfo(queryId);

                Assert.IsInstanceOfType(result.Value, typeof(QueryInfoResponse));
                Assert.AreEqual(queryId, result.Value.Query);
            }
        }
    }
}