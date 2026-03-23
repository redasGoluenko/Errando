using Microsoft.EntityFrameworkCore;

namespace Errando.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<TodoTask> Tasks { get; set; } = null!;
        public DbSet<TaskItem> TaskItems { get; set; } = null!;
        public DbSet<StatusLog> StatusLogs { get; set; } = null!;
        public DbSet<Complaint> Complaints { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TodoTask>()
                .HasOne(t => t.Client)
                .WithMany()
                .HasForeignKey(t => t.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TodoTask>()
                .HasOne(t => t.Runner)
                .WithMany()
                .HasForeignKey(t => t.RunnerId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<TaskItem>()
                .HasOne(ti => ti.Task)
                .WithMany(t => t.TaskItems)
                .HasForeignKey(ti => ti.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StatusLog>()
                .HasOne(sl => sl.TaskItem)
                .WithMany()
                .HasForeignKey(sl => sl.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StatusLog>()
                .HasOne(sl => sl.Runner)
                .WithMany()
                .HasForeignKey(sl => sl.RunnerId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Complaint>()
                .HasOne(c => c.Task)
                .WithMany()
                .HasForeignKey(c => c.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Complaint>()
                .HasOne(c => c.Client)
                .WithMany()
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Complaint>()
                .HasOne(c => c.Runner)
                .WithMany()
                .HasForeignKey(c => c.RunnerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
