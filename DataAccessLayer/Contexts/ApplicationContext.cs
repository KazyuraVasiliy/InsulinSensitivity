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

        public ApplicationContext(string databasePath) =>
            this.databasePath = databasePath;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.UseSqlite($"Filename={databasePath}");
    }
}