using Microsoft.EntityFrameworkCore;

namespace Projects.Api.Models {
    public class ProjectContext : DbContext {
        public ProjectContext(DbContextOptions<ProjectContext> options)
            : base(options) { }
        public DbSet<Project> Projects { get; set; } = null;
        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Project>().ToContainer("projects").HasPartitionKey(p => p.id);
        }
    }

}
