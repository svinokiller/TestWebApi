using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Generic;
using WebApi.Controllers;
using static WebApi.QueryFileManager;
using WebApi.Models;
using static WebApi.Controllers.ReportController;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

var builder = WebApplication.CreateBuilder(args);

string connection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(connection), ServiceLifetime.Transient);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();


app.MapControllers();


// Обработка незавершенных запросов 
var unfinishedRequests = LoadPendingUserStatisticsRequests();
foreach (var requestInfo in unfinishedRequests)
{
    Task.Run(async () =>
    {
        using (var scope = app.Services.CreateScope())
        {
            var serviceProvider = scope.ServiceProvider;

            var logger = serviceProvider.GetRequiredService<ILogger<ReportController>>();
            var dbContext = serviceProvider.GetRequiredService<ApplicationContext>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

            var reportController = new ReportController(logger, dbContext, configuration, serviceScopeFactory);

            await reportController.ProcessUserStatistics(requestInfo.QueryId, requestInfo.Request.UserId, requestInfo.Request.Start, requestInfo.Request.End, requestInfo.RequestDateTime);

            //Удаляем обработанный запрос из незавершенных
            await RemoveProcessedRequestAsync(requestInfo.QueryId);

        }
    });
}

app.Run();
