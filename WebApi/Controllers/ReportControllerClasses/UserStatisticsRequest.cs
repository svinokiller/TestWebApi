namespace WebApi.Controllers
{
    public partial class ReportController
    {
        public class UserStatisticsRequest
        {
            public string? UserId { get; set; }
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
        }
    }
}
