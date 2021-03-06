using Microsoft.EntityFrameworkCore;
using DataAccessLayer.Models;

namespace DataAccessLayer.Contexts
{
    public class ApplicationContext : DbContext
    {
        private string databasePath;

        public DbSet<Eating> Eatings { get; set; }
        public DbSet<EatingType> EatingTypes { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<ExerciseType> ExerciseTypes { get; set; }
        public DbSet<InsulinType> InsulinTypes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<MenstrualCycle> MenstrualCycles { get; set; }
        public DbSet<Injection> Injections { get; set; }
        public DbSet<IntermediateDimension> IntermediateDimensions { get; set; }

        public ApplicationContext(string databasePath) =>
            this.databasePath = databasePath;

        public ApplicationContext() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.UseSqlite($"Filename={databasePath}");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InsulinType>()
                .HasMany(b => b.BasalUsers)
                .WithOne(p => p.BasalType);

            modelBuilder.Entity<InsulinType>()
                .HasMany(b => b.BolusUsers)
                .WithOne(p => p.BolusType);

            modelBuilder.Entity<InsulinType>()
                .HasMany(b => b.BasalEatings)
                .WithOne(p => p.BasalType);

            modelBuilder.Entity<InsulinType>()
                .HasMany(b => b.BolusEatings)
                .WithOne(p => p.BolusType);
        }
    }
}