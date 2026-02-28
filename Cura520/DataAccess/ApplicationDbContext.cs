using Cura520.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Cura520.DataAccess
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<MedicalHistory> MedicalHistory { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<PrescriptionItem> PrescriptionItems { get; set; }
        public DbSet<ApplicationUserOTP> ApplicationUserOTPs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // IMPORTANT: Always call the base method first for Identity tables
            base.OnModelCreating(modelBuilder);

            // 1. One-to-One Relationships (Configured as Optional to avoid Filter Warnings)
            modelBuilder.Entity<Patient>()
                .HasOne(p => p.MedicalHistory)
                .WithOne(mh => mh.Patient)
                .HasForeignKey<MedicalHistory>(mh => mh.PatientId)
                .IsRequired(false);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Invoice)
                .WithOne(i => i.Appointment)
                .HasForeignKey<Invoice>(i => i.AppointmentId)
                .IsRequired(false);

            // 2. Decimal Precision (ERP Financial Accuracy)
            modelBuilder.Entity<Doctor>()
                .Property(d => d.ConsultationFee)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Invoice>()
                .Property(i => i.SubTotal).HasPrecision(18, 2);
            modelBuilder.Entity<Invoice>()
                .Property(i => i.Tax).HasPrecision(18, 2);
            modelBuilder.Entity<Invoice>()
                .Property(i => i.Total).HasPrecision(18, 2);

            // 3. Multi-path Relationship Restrictions (Preventing Cyclic Deletes)
            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.Patient)
                .WithMany(pt => pt.Prescriptions)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MedicalHistory>()
                .HasOne(mh => mh.Appointment)
                .WithMany()
                .HasForeignKey(mh => mh.AppointmentId)
                .OnDelete(DeleteBehavior.NoAction);

            // 4. Global Query Filters for Soft Delete
            // Filters the main entities
            modelBuilder.Entity<Patient>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<Appointment>().HasQueryFilter(a => !a.IsDeleted);

            // Propagates the filter to dependent entities to avoid orphaned data warnings
            modelBuilder.Entity<MedicalHistory>().HasQueryFilter(mh => !mh.Patient.IsDeleted);
            modelBuilder.Entity<Invoice>().HasQueryFilter(i => !i.Appointment.IsDeleted);
            modelBuilder.Entity<Prescription>().HasQueryFilter(p => !p.Patient.IsDeleted);
        }
    }
}