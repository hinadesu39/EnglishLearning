using ListeningDomain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ListeningInfrastructure
{
    public class ListeningDbContext:DbContext
    {
        private readonly DbContextOptions<ListeningDbContext> options;

        public DbSet<Category> Categories { get; private set; }//不要忘了写set，否则拿到的DbContext的Categories为null
        public DbSet<Album> Albums { get; private set; }
        public DbSet<Episode> Episodes { get; private set; }
        public ListeningDbContext(DbContextOptions<ListeningDbContext> options):base(options)
        {
            
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
            modelBuilder.Entity<Category>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<Album>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<Episode>().HasQueryFilter(p => !p.IsDeleted);
        }
    }
}