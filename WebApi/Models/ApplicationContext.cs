using Microsoft.EntityFrameworkCore;

namespace WebApi.Models
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Query> Queries { get; set; } = null!;
        public DbSet<UserLoginAttempt> UserLoginAttempts { get; set; } = null!;
        
    
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
            Database.EnsureCreated();   
        }
    }
}
