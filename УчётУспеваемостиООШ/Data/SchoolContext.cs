using Microsoft.EntityFrameworkCore;
using УчётУспеваемостиООШ.Models;

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
            // Строка подключения к SQL Server
            optionsBuilder.UseSqlServer(@"Server=DESKTOP-MQ9HMGB;Database=УчетУспеваемостиООШ;Trusted_Connection=True;TrustServerCertificate=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Настройка уникальности Username
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Настройка для Grade (чтобы свойство GradeValue соответствовало колонке Grade в БД)
            modelBuilder.Entity<Grade>()
                .Property(g => g.GradeValue)
                .HasColumnName("Grade");

            base.OnModelCreating(modelBuilder);
        }
    }
}