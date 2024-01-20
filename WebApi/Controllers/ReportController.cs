using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebApi.Models;
using static WebApi.QueryFileManager;

namespace WebApi.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public partial class ReportController : ControllerBase
    {
        private readonly ILogger<ReportController> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IConfiguration _configuration;
        private ApplicationContext _dbContext;
        private readonly int _processingTimeoutMilliseconds;
        public ReportController(ILogger<ReportController> logger, ApplicationContext context, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
        {
            _processingTimeoutMilliseconds = configuration.GetValue<int>("AppSettings:ProcessingTimeoutMilliseconds", 60000);
            _configuration = configuration;
            _logger = logger;
            _dbContext = context;
            _serviceScopeFactory = serviceScopeFactory;
        }

        [HttpPost("user_statistics")]
        public async Task<ActionResult<Guid>> GetUserStatistics([FromBody] UserStatisticsRequest request)
        {
            Guid queryId = Guid.NewGuid();

            var query = new Models.Query
            {
                QueryId = queryId,
                Start = DateTime.UtcNow
            };

            _dbContext.Queries.Add(query);
            await _dbContext.SaveChangesAsync();

            // cохраненяем информацию о запросе в файл
            var requestInfo = new UserStatisticsRequestInfo
            {
                QueryId = queryId,
                Request = request,
                RequestDateTime = query.Start
            };
            SaveUserStatisticsRequest(requestInfo);

            
            Task.Run(async () =>
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var scopedServiceProvider = scope.ServiceProvider;
                    var dbContext = scopedServiceProvider.GetRequiredService<ApplicationContext>();
                    var controller = new ReportController(_logger, dbContext, _configuration, _serviceScopeFactory);
                    
                    await controller.ProcessUserStatistics(queryId, request.UserId, request.Start, request.End,null);

                    await RemoveProcessedRequestAsync(requestInfo.QueryId);
                    
                }
            });

            return queryId;
        }

        [HttpGet("info")]
        public async Task<ActionResult<QueryInfoResponse>> GetQueryInfo(Guid queryId)
        {
            var query = await _dbContext.Queries.FindAsync(queryId);

            if (query == null)
            {
                return NotFound();
            }

            var percent = CalcPercent(query.Start);
            var response = new QueryInfoResponse
            {
                Query = queryId,
                Percent = percent,
                Result = null
            };

            if (percent == 100)
            {
                response.Result = await GetQueryResult(queryId);
            }

            return response;
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task ProcessUserStatistics(Guid queryId, string userId, DateTime start, DateTime end, DateTime? queryStart)
        {
            // имитация длительной обработки
            if (queryStart == null)// Если новый запрос
            {
                await Task.Delay(_processingTimeoutMilliseconds);
            }
            else if (CalcPercent((DateTime)queryStart) < 100)// Если незавершенный запрос и ещё есть что обрабатывать
            {
                var elapsedMilliseconds = (int)(DateTime.UtcNow - (DateTime)queryStart).TotalMilliseconds;
                await Task.Delay(_processingTimeoutMilliseconds - elapsedMilliseconds);
            }

            int countSignIn = await _dbContext.UserLoginAttempts
                    .Where(ula => ula.UserId == userId && ula.Datetime >= start && ula.Datetime <= end)
                    .CountAsync();

            var result = new UserStatisticsResult
            {
                UserId = userId,
                CountSignIn = countSignIn
            };

            var query = await _dbContext.Queries.FindAsync(queryId);
            query.Result = JsonConvert.SerializeObject(result);
            await _dbContext.SaveChangesAsync();
        }

        private int CalcPercent(DateTime start)
        {
            var elapsedMilliseconds = (int)(DateTime.UtcNow - start).TotalMilliseconds;
            return Math.Min(100, (elapsedMilliseconds * 100) / _processingTimeoutMilliseconds);
        }

        private async Task<UserStatisticsResult> GetQueryResult(Guid queryId)
        {
            var query = await _dbContext.Queries.FindAsync(queryId);

            if (query == null || string.IsNullOrEmpty(query.Result))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<UserStatisticsResult>(query.Result);
        }
    }
}
