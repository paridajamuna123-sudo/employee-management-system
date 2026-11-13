using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EmployeeManagement.API.Models;

namespace EmployeeManagement.API.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Designation> Designations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Department Configuration
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.DepartmentName).IsRequired().HasMaxLength(100);
            entity.HasIndex(d => d.DepartmentName).IsUnique();
            entity.Property(d => d.Description).HasMaxLength(500);
            entity.Property(d => d.IsActive).HasDefaultValue(true);
            entity.Property(d => d.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // Designation Configuration
        modelBuilder.Entity<Designation>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.DesignationName).IsRequired().HasMaxLength(100);
            entity.HasIndex(d => d.DesignationName).IsUnique();
            entity.Property(d => d.Description).HasMaxLength(500);
            entity.Property(d => d.IsActive).HasDefaultValue(true);
            entity.Property(d => d.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // Employee Configuration
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EmployeeCode).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.EmployeeCode).IsUnique();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Gender).HasMaxLength(20);
            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.Salary).HasPrecision(18, 2);
            entity.Property(e => e.EmploymentType).IsRequired().HasMaxLength(30).HasDefaultValue("Full-Time");
            entity.Property(e => e.ProfileImageUrl).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Map LegacyDepartment property to the existing 'Department' column in the database
            entity.Property(e => e.LegacyDepartment).HasColumnName("Department");

            // Safe Restrict foreign key deletion
            entity.HasOne(e => e.Department)
                  .WithMany(d => d.Employees)
                  .HasForeignKey(e => e.DepartmentId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Designation)
                  .WithMany(d => d.Employees)
                  .HasForeignKey(e => e.DesignationId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ApplicationUser Configuration
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.HasOne(u => u.Employee)
                  .WithMany()
                  .HasForeignKey(u => u.EmployeeId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Enforce 1:0..1 unique relationship
            entity.HasIndex(u => u.EmployeeId)
                  .IsUnique()
                  .HasFilter("[EmployeeId] IS NOT NULL");
        });
    }
}
