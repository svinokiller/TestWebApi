namespace WebApi.Controllers
{
    public partial class ReportController
    {
        public class UserStatisticsRequestInfo
        {
            public Guid QueryId { get; set; }
            public UserStatisticsRequest? Request { get; set; }
            public DateTime RequestDateTime { get; set; }
        }
    }
}
