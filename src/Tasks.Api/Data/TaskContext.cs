using Microsoft.EntityFrameworkCore;
using Tasks.Api.Models;

namespace Tasks.Api.Data {
    public class TaskContext: DbContext {
        public TaskContext(DbContextOptions<TaskContext> options)
          : base(options) { }
        public virtual DbSet<TaskItem> TaskItems { get; set; } = null;
        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<TaskItem>().ToContainer("tasks").HasPartitionKey(p => p.projectId);
        }
    }
}
