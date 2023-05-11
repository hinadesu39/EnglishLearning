using MediaEncoderDomain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace MediaEncoderInfrastructure
{
    public class MEDbContext:DbContext
    {
        public DbSet<EncodingItem> EncodingItems { get; private set; }

        public MEDbContext(DbContextOptions<MEDbContext> options)
        : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
            modelBuilder.EnableSoftDeletionGlobalFilter();
        }
    }
}