using System;
using Microsoft.EntityFrameworkCore;
using AspNetWeek3.Mvc.Models;

namespace AspNetWeek3.Mvc.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<CourseCategory> CourseCategories => Set<CourseCategory>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<EnrollmentDetail> EnrollmentDetails => Set<EnrollmentDetail>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CourseCategory>(entity =>
        {
            entity.ToTable("CourseCategories");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.ToTable("Courses");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(150);
            entity.Property(p => p.Price).HasColumnType("decimal(18,2)");
            entity.Property(p => p.CourseCode).IsRequired().HasMaxLength(20).HasDefaultValue(""); // Added in Feature 3
            entity.HasIndex(p => p.CourseCode).IsUnique();
            entity.Property(p => p.RowVersion).IsConcurrencyToken();
            entity.HasQueryFilter(p => !p.IsDeleted);
            entity.HasOne(p => p.CourseCategory)
                  .WithMany(c => c.Courses)
                  .HasForeignKey(p => p.CourseCategoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("Students");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Email).IsRequired().HasMaxLength(150);
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.ToTable("Enrollments");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
            entity.HasOne(o => o.Student)
                  .WithMany(c => c.Enrollments)
                  .HasForeignKey(o => o.StudentId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<EnrollmentDetail>(entity =>
        {
            entity.ToTable("EnrollmentDetails");
            entity.HasKey(oi => oi.Id);
            entity.Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)");
            entity.HasOne(oi => oi.Enrollment)
                  .WithMany(o => o.EnrollmentDetails)
                  .HasForeignKey(oi => oi.EnrollmentId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(oi => oi.Course)
                  .WithMany()
                  .HasForeignKey(oi => oi.CourseId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Seed initial data
        modelBuilder.Entity<CourseCategory>().HasData(
            new CourseCategory { Id = 1, Name = "Programming" },
            new CourseCategory { Id = 2, Name = "Design" }
        );

        modelBuilder.Entity<Course>().HasData(
            new Course { Id = 1, CourseCode = "CS101", Name = "Introduction to C#", Price = 1200000, AvailableSeats = 15, CourseCategoryId = 1, CreatedAt = new DateTime(2026, 6, 12, 12, 0, 0), IsDeleted = false, RowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 } },
            new Course { Id = 2, CourseCode = "CS201", Name = "ASP.NET Core Web MVC", Price = 3500000, AvailableSeats = 3, CourseCategoryId = 1, CreatedAt = new DateTime(2026, 6, 12, 12, 0, 0), IsDeleted = false, RowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 } },
            new Course { Id = 3, CourseCode = "DS101", Name = "UI/UX Design Fundamentals", Price = 2000000, AvailableSeats = 8, CourseCategoryId = 2, CreatedAt = new DateTime(2026, 6, 12, 12, 0, 0), IsDeleted = false, RowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 } }
        );

        modelBuilder.Entity<Student>().HasData(
            new Student { Id = 1, Name = "Nguyen Van A", Email = "vana@gmail.com" },
            new Student { Id = 2, Name = "Tran Thi B", Email = "thib@gmail.com" }
        );


    }

    public override int SaveChanges()
    {
        GenerateRowVersionsAndAudits();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        GenerateRowVersionsAndAudits();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void GenerateRowVersionsAndAudits()
    {
        var courseEntries = ChangeTracker.Entries<Course>();
        foreach (var entry in courseEntries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.Now;
                entry.Entity.RowVersion = Guid.NewGuid().ToByteArray();
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.Now;
                entry.Entity.RowVersion = Guid.NewGuid().ToByteArray();
            }
        }


    }
}
