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
        public DbSet<DoctorSchedule> DoctorSchedules { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<MedicalService> MedicalServices { get; set; }
        public DbSet<MedicalHistory> MedicalHistories { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Receptionist> Receptionists { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<PrescriptionItem> PrescriptionItems { get; set; }
        public DbSet<ApplicationUserOTP> ApplicationUserOTPs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Identity Links (Profile Pattern)
            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.ApplicationUser).WithOne().HasForeignKey<Doctor>(d => d.ApplicationUserId);

            modelBuilder.Entity<Patient>()
                .HasOne(p => p.ApplicationUser).WithOne().HasForeignKey<Patient>(p => p.ApplicationUserId);

            modelBuilder.Entity<Receptionist>()
                .HasOne(r => r.ApplicationUser).WithOne().HasForeignKey<Receptionist>(r => r.ApplicationUserId);

            // 2. Financial Precision (18,2)
            modelBuilder.Entity<Doctor>().Property(d => d.ConsultationFee).HasPrecision(18, 2);
            modelBuilder.Entity<MedicalService>().Property(ms => ms.DefaultPrice).HasPrecision(18, 2);
            modelBuilder.Entity<Invoice>().Property(i => i.SubTotal).HasPrecision(18, 2);
            modelBuilder.Entity<Invoice>().Property(i => i.Tax).HasPrecision(18, 2);
            modelBuilder.Entity<Invoice>().Property(i => i.Total).HasPrecision(18, 2);
            modelBuilder.Entity<InvoiceItem>().Property(ii => ii.UnitPrice).HasPrecision(18, 2);

            // 3. Relationships & Delete Behaviors
            // One Appointment has One Invoice
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Invoice).WithOne(i => i.Appointment)
                .HasForeignKey<Invoice>(i => i.AppointmentId).IsRequired(false);

            // Prevent cyclic deletes for Patients and Doctors
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient).WithMany(p => p.Appointments).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor).WithMany(d => d.Appointments).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Prescription>()
                .HasOne(p => p.Patient).WithMany(pt => pt.Prescriptions).OnDelete(DeleteBehavior.Restrict);

            // 4. Global Query Filters (Soft Delete)
            modelBuilder.Entity<Doctor>().HasQueryFilter(d => !d.IsDeleted);
            modelBuilder.Entity<Patient>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<Receptionist>().HasQueryFilter(r => !r.IsDeleted);
            modelBuilder.Entity<Appointment>().HasQueryFilter(a => !a.IsDeleted);
            modelBuilder.Entity<MedicalService>().HasQueryFilter(ms => !ms.IsDeleted);

            // Propagation filters
            modelBuilder.Entity<Invoice>().HasQueryFilter(i => !i.Appointment.IsDeleted);
            modelBuilder.Entity<MedicalHistory>().HasQueryFilter(mh => !mh.Patient.IsDeleted);
        }
    }
}