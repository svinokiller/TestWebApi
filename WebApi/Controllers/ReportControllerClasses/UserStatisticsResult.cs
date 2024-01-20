namespace WebApi.Controllers
{
    public partial class ReportController
    {
        public class UserStatisticsResult
        {
            public string? UserId { get; set; }
            public int CountSignIn { get; set; }
        }
    }
}
