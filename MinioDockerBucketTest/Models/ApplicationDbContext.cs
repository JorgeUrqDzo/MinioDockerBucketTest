using Microsoft.EntityFrameworkCore;

namespace MinioDockerBucketTest.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=database.db");

        public DbSet<UserFiles> UserFiles { get; set; }
    }
}
