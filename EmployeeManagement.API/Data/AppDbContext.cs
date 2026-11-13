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
    public DbSet<Attendance> Attendances { get; set; }
    public DbSet<AttendanceCorrection> AttendanceCorrections { get; set; }
    public DbSet<LeaveType> LeaveTypes { get; set; }
    public DbSet<LeaveBalance> LeaveBalances { get; set; }
    public DbSet<LeaveRequest> LeaveRequests { get; set; }
    public DbSet<LeaveApproval> LeaveApprovals { get; set; }

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

        // Attendance Configuration
        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.AttendanceDate).HasColumnType("date");
            entity.Property(a => a.WorkingHours).HasPrecision(5, 2);
            entity.Property(a => a.OvertimeHours).HasPrecision(5, 2);
            entity.Property(a => a.Status).HasMaxLength(20).HasDefaultValue("Present");
            entity.Property(a => a.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(a => a.Employee)
                  .WithMany()
                  .HasForeignKey(a => a.EmployeeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(a => new { a.EmployeeId, a.AttendanceDate }).IsUnique();
        });

        // AttendanceCorrection Configuration
        modelBuilder.Entity<AttendanceCorrection>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.AttendanceDate).HasColumnType("date");
            entity.Property(c => c.Reason).IsRequired().HasMaxLength(500);
            entity.Property(c => c.Status).HasMaxLength(20).HasDefaultValue("Pending");
            entity.Property(c => c.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(c => c.Employee)
                  .WithMany()
                  .HasForeignKey(c => c.EmployeeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.ReviewedBy)
                  .WithMany()
                  .HasForeignKey(c => c.ReviewedById)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // LeaveType Configuration
        modelBuilder.Entity<LeaveType>(entity =>
        {
            entity.HasKey(lt => lt.Id);
            entity.Property(lt => lt.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(lt => lt.Name).IsUnique();
            entity.Property(lt => lt.Code).IsRequired().HasMaxLength(20);
            entity.HasIndex(lt => lt.Code).IsUnique();
            entity.Property(lt => lt.DefaultDaysPerYear).HasPrecision(4, 1);
            entity.Property(lt => lt.IsActive).HasDefaultValue(true);
            entity.Property(lt => lt.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // LeaveBalance Configuration
        modelBuilder.Entity<LeaveBalance>(entity =>
        {
            entity.HasKey(lb => lb.Id);
            entity.Property(lb => lb.TotalDays).HasPrecision(4, 1);
            entity.Property(lb => lb.UsedDays).HasPrecision(4, 1);
            entity.Property(lb => lb.RemainingDays).HasPrecision(4, 1);

            entity.HasOne(lb => lb.Employee)
                  .WithMany()
                  .HasForeignKey(lb => lb.EmployeeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(lb => lb.LeaveType)
                  .WithMany(lt => lt.LeaveBalances)
                  .HasForeignKey(lb => lb.LeaveTypeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(lb => new { lb.EmployeeId, lb.LeaveTypeId, lb.Year }).IsUnique();
        });

        // LeaveRequest Configuration
        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.HasKey(lr => lr.Id);
            entity.Property(lr => lr.StartDate).HasColumnType("date");
            entity.Property(lr => lr.EndDate).HasColumnType("date");
            entity.Property(lr => lr.TotalDays).HasPrecision(4, 1);
            entity.Property(lr => lr.Reason).IsRequired().HasMaxLength(1000);
            entity.Property(lr => lr.Status).HasMaxLength(20).HasDefaultValue("Pending");
            entity.Property(lr => lr.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(lr => lr.Employee)
                  .WithMany()
                  .HasForeignKey(lr => lr.EmployeeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(lr => lr.LeaveType)
                  .WithMany(lt => lt.LeaveRequests)
                  .HasForeignKey(lr => lr.LeaveTypeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // LeaveApproval Configuration
        modelBuilder.Entity<LeaveApproval>(entity =>
        {
            entity.HasKey(la => la.Id);
            entity.Property(la => la.Action).IsRequired().HasMaxLength(20);
            entity.Property(la => la.Comments).HasMaxLength(500);
            entity.Property(la => la.ActionDate).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(la => la.LeaveRequest)
                  .WithMany(lr => lr.Approvals)
                  .HasForeignKey(la => la.LeaveRequestId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(la => la.ApproverUser)
                  .WithMany()
                  .HasForeignKey(la => la.ApproverUserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
