using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using УчётУспеваемостиООШ.Models;
using УчётУспеваемостиООШ.Services;

namespace УчётУспеваемостиООШ.Data
{
    public class SchoolContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Attendance> Attendances { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = ConfigManager.Instance.GetConnectionString();
            optionsBuilder.UseSqlServer(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Существующие настройки
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<Grade>()
                .Property(g => g.GradeValue)
                .HasColumnName("Grade");

            // ========== ДОБАВИТЬ ИНДЕКСЫ ==========

            // Индексы для Students
            modelBuilder.Entity<Student>()
                .HasIndex(s => s.LastName)
                .HasDatabaseName("IX_Students_LastName");

            modelBuilder.Entity<Student>()
                .HasIndex(s => s.FirstName)
                .HasDatabaseName("IX_Students_FirstName");

            modelBuilder.Entity<Student>()
                .HasIndex(s => s.ClassID)
                .HasDatabaseName("IX_Students_ClassID");

            // Индексы для Grades
            modelBuilder.Entity<Grade>()
                .HasIndex(g => g.StudentID)
                .HasDatabaseName("IX_Grades_StudentID");

            modelBuilder.Entity<Grade>()
                .HasIndex(g => g.SubjectID)
                .HasDatabaseName("IX_Grades_SubjectID");

            modelBuilder.Entity<Grade>()
                .HasIndex(g => g.Date)
                .HasDatabaseName("IX_Grades_Date");

            // Индексы для Attendance
            modelBuilder.Entity<Attendance>()
                .HasIndex(a => a.StudentID)
                .HasDatabaseName("IX_Attendance_StudentID");

            modelBuilder.Entity<Attendance>()
                .HasIndex(a => a.Date)
                .HasDatabaseName("IX_Attendance_Date");

            // Индексы для Subjects
            modelBuilder.Entity<Subject>()
                .HasIndex(s => s.SubjectName)
                .HasDatabaseName("IX_Subjects_SubjectName");

            modelBuilder.Entity<Subject>()
                .HasIndex(s => s.TeacherID)
                .HasDatabaseName("IX_Subjects_TeacherID");

            base.OnModelCreating(modelBuilder);
        }

        // Логирование времени выполнения запросов
        public override int SaveChanges()
        {
            var stopwatch = Stopwatch.StartNew();
            var result = base.SaveChanges();
            stopwatch.Stop();

            // ВСЕГДА логируем время выполнения
            Logger.Metrics("Query executed", $"{stopwatch.ElapsedMilliseconds} ms");

            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                Logger.Warning($"Slow query detected: {stopwatch.ElapsedMilliseconds}ms", "DB");
            }

            return result;
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = await base.SaveChangesAsync(cancellationToken);
            stopwatch.Stop();

            Logger.Metrics("Query executed", $"{stopwatch.ElapsedMilliseconds} ms");

            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                Logger.Warning($"Slow query detected: {stopwatch.ElapsedMilliseconds}ms", "DB");
            }

            return result;
        }
    }
}