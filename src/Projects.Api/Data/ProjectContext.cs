using Microsoft.EntityFrameworkCore;
using Projects.Api.Models;

namespace Projects.Api.Data {
    public class ProjectContext : DbContext {
        public ProjectContext(DbContextOptions<ProjectContext> options)
            : base(options) { }
        public virtual DbSet<Project> Projects { get; set; } = null;
        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Project>().ToContainer("projects").HasPartitionKey(p => p.id);
        }
    }

}
