namespace WebApi.Controllers
{
    public partial class ReportController
    {
        public class QueryInfoResponse
        {
            public Guid Query { get; set; }
            public int Percent { get; set; }
            public UserStatisticsResult? Result { get; set; }
        }
    }
}
